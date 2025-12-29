using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace SimpleAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="SuperAdmin")]
    public class UserManagementController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        public UserManagementController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        // Le SuperAdmin change le mot de passe sans avoir besoin de l'ancien ni d'un mail
        [HttpPost("admin-force-reset")]
        public async Task<IActionResult> ForceReset(string email, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("Utilisateur Introuvable");
            // user not null donc supprime son ancien mdp
            await _userManager.RemovePasswordAsync(user);
            //mettre le nouveau mdp
            var result = await _userManager.AddPasswordAsync(user, newPassword);
            if (result.Succeeded) return Ok("Mot de passe réinitialisé avec succès ! ");
            return BadRequest(result.Errors);

        }


    }
}
