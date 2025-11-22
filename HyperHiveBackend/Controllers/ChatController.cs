using Microsoft.AspNetCore.Mvc;
using HyperHiveBackend.DTOs;
using HyperHiveBackend.Services;

namespace HyperHiveBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IOpenAIService openAIService, ILogger<ChatController> logger)
        {
            _openAIService = openAIService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { message = "Message cannot be empty" });
                }

                _logger.LogInformation("Received chat request: {Message}", request.Message);

                var response = await _openAIService.GetChatResponseAsync(request.Message);

                return Ok(new ChatResponse
                {
                    Response = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }
    }
}

