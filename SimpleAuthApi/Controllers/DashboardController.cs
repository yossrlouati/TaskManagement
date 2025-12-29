using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SimpleAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        [HttpGet("admin")]
        [Authorize(Roles ="Admin")]
        public IActionResult GetAdminDashboard()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            return Ok($"Bienvenue{username} sur le tableau de bord Admin !");
        }
        [HttpGet("employee")]
        [Authorize(Roles = "Employee")]
        public IActionResult GetEmployeeDashboard()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            return Ok($"Bienvenue {username} sur le tableau de bord Employé! Accès accordé.");
        }
        // Tous les utilisateurs authentifiés
        [HttpGet("user")]
        [Authorize]
        public IActionResult GetUserData()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            return Ok($"Données utilisateur pour {username}.");
        }
    }
}
