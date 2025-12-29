using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleAuthApi.Data;
using SimpleAuthApi.Models;

namespace SimpleAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="SuperAdmin,Admin")]
    public class SettingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public SettingsController(AppDbContext context) => _context = context;
        [HttpGet]
        public async Task<ActionResult<GlobalSettings>> GetSettings()
        {
            var settings = await _context.GlobalSettings.FirstOrDefaultAsync();
            return settings == null ? NotFound() : Ok(settings);
        }

        [HttpPut]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateSettings(GlobalSettings updatedSettings)
        {
            var existing = await _context.GlobalSettings.FirstOrDefaultAsync();
            if (existing == null) return NotFound();
            existing.SmtpServer = updatedSettings.SmtpServer;
            existing.SmtpPort = updatedSettings.SmtpPort;
            existing.SenderEmail = updatedSettings.SenderEmail;
            existing.SmtpPassword = updatedSettings.SmtpPassword;
            existing.IsEmailNotificationEnabled = updatedSettings.IsEmailNotificationEnabled;
            existing.CompanyName = updatedSettings.CompanyName;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPatch("toggle-notifications")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleNotif([FromBody] bool isEnabled)
        {
            var existing = await _context.GlobalSettings.FirstOrDefaultAsync();
            if (existing == null) return NotFound();
           
            existing.IsEmailNotificationEnabled =isEnabled;
          
            await _context.SaveChangesAsync();
            return Ok(new { Message = $"Notifications {(isEnabled ? "activées" : "désactivées")}" });
        }

    }
}
