using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SimpleAuthApi.Data;

namespace SimpleAuthApi.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; // Ajout pour chercher l'email


        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendTaskNotificationAsync(List<string> emails, string taskTitle, string? attachmentPath, string adminId)
        {
            /*var smtpServer = _config["EmailSettings:SmtpServer"];
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var password = _config["EmailSettings:Password"];*/
            //Récupération dynamique des réglages en base de données
            var settings = await _context.GlobalSettings.FirstOrDefaultAsync();
            if (settings == null || !settings.IsEmailNotificationEnabled)
                return; // On ne fait rien si désactivé ou non configuré
            //  2.Vérification INDIVIDUELLE(L'Admin qui crée la tâche)
            // On va chercher l'admin dans la table des utilisateurs
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null || !settings.IsEmailNotificationEnabled) // !admin.IsEmailNotificationEnabled
            {
                return; // Si l'admin a décoché "Envoyer des mails" dans son profil, on s'arrête
            }

            try
            {
                using var client = new SmtpClient(settings.SmtpServer, 587)
                {
                    UseDefaultCredentials = false,// Ajoute cette ligne si elle manque
                    Credentials = new NetworkCredential(settings.SenderEmail, settings.SmtpPassword),
                    //Credentials = new NetworkCredential("yossr52@gmail.com", password),

                    EnableSsl = true

                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(settings.SenderEmail, "Task Manager"),
                    Subject = $"[{settings.CompanyName}] Nouvelle tâche : {taskTitle}",
                    Body = "Bonjour, Une nouvelle tâche est assignée."
                };

                // Ajouter les destinataires
                foreach (var email in emails)
                {
                    mailMessage.To.Add(email);
                }

                // Ajouter la pièce jointe si elle existe
                if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
                {
                    Attachment attachment = new Attachment(attachmentPath);
                    mailMessage.Attachments.Add(attachment);
                }

                await client.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                // On log l'erreur mais on ne bloque pas l'utilisateur
                Console.WriteLine($"Erreur SMTP : {ex.Message}");
                // Optionnel : throw ou gérer silencieusement
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur générale mail : {ex.Message}");
            }

            
        }
        public async Task SendResetPassswordEmailAsync(string email, string resetLink)
        {
            var settings = await _context.GlobalSettings.FirstOrDefaultAsync();
            if (settings == null) return;

            using var client = new SmtpClient(settings.SmtpServer, settings.SmtpPort)
            {
                Credentials = new NetworkCredential(settings.SenderEmail, settings.SmtpPassword),
                EnableSsl = true

            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress(settings.SenderEmail, settings.CompanyName),
                Subject = "Réinitialisation de mot de passe ",
                Body = $"Veuillez réinitialiser votre mot de passe en cliquent sur ce code {resetLink}",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);
            await client.SendMailAsync(mailMessage);
        }

        public Task SendResetPasswordEmailAsync(string email, string resetLink)
        {
            throw new NotImplementedException();
        }
    }
}
