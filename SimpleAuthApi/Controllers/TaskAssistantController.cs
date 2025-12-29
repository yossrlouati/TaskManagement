using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleAuthApi.Services;

namespace SimpleAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class TaskAssistantController : ControllerBase
    {
        private readonly GeminiService _geminiService;

        public TaskAssistantController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }
        [HttpGet("generate-description")]
        public async Task<IActionResult> GetDescription(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Le titre de la tâche est obligatoire.");

            try
            {
                var result = await _geminiService.GenerateTaskDescrip(title);
                return Ok(new { description = result });
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Erreur{ex.Message}");
            }
        }
    }
}
