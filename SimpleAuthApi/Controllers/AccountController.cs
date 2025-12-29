using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleAuthApi.Dto;
using SimpleAuthApi.Services;
using System.Runtime.CompilerServices;

namespace SimpleAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        public AccountController(UserManager<IdentityUser> userManager, IEmailService emailService, IConfiguration config)
        {
            _userManager = userManager;
            _emailService = emailService;
            _config = config;

        }
        //Demande de reset (Génère le mail)
        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Ok("Si l'email existe , un lien a été envoyé");//ne pas dire le mail existe ou pas : Sécurité 
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // En vrai, on envoie un lien vers le FRONTEND avec le token
            var frontendUrl = _config["AppSettings:FrontendUrl"];
            var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={email}";
            await _emailService.SendResetPasswordEmailAsync(email,
            $"Cliquez ici pour réinitialiser : {resetLink}");
            return Ok("Lien de réinitialisation envoyé");
        }
        //Reset de new mdp
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return BadRequest("Requete Invalide");
            if (model.NewPassword != model.ConfirmNewPassword) return BadRequest("Les mots de passe ne correspondent pas.");
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded) return Ok("Mot de passe modifié");
            return BadRequest();
        }

    }
}
