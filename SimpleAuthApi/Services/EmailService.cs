/*namespace SimpleAuthApi.Services
{
    public class EmailService :IEmailService
    {
        public Task SendAssignementNotifiactionAsync(string recipientEmail, string taskTitle, string assignedBy)
        {
            // --- Cette section simule l'envoi d'email (ex: via SendGrid ou SmtpClient) ---

            Console.WriteLine("---------------------------------------------");
            Console.WriteLine($"[EMAIL SIMULÉ] Envoi à: {recipientEmail}");
            Console.WriteLine($"Sujet: Nouvelle Tâche Assignée: {taskTitle}");
            Console.WriteLine($"Corps: Vous avez été assigné(e) à la tâche '{taskTitle}' par {assignedBy}.");
            Console.WriteLine("---------------------------------------------");

            // Task.CompletedTask retourne une Task terminée immédiatement.
            // Il est utilisé ici pour respecter la signature asynchrone de l'interface.
            return Task.CompletedTask;
        }


    }
}
*/