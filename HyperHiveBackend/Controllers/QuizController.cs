using HyperHiveBackend.DTOs;
using HyperHiveBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace HyperHiveBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly ILogger<QuizController> _logger;

        public QuizController(IQuizService quizService, ILogger<QuizController> logger)
        {
            _quizService = quizService;
            _logger = logger;
        }

        /// <summary>
        /// Generate a new quiz for a learner using AI
        /// </summary>
        [HttpPost("generate")]
        public async Task<ActionResult<GenerateQuizResponse>> GenerateQuiz([FromBody] GenerateQuizRequest request)
        {
            try
            {
                _logger.LogInformation("Generating quiz for learner {LearnerId}", request.LearnerId);
                
                var response = await _quizService.GenerateQuizAsync(request);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quiz");
                return StatusCode(500, new { error = "Failed to generate quiz", details = ex.Message });
            }
        }

        /// <summary>
        /// Submit quiz answers and get results
        /// </summary>
        [HttpPost("submit")]
        public async Task<ActionResult<SubmitQuizResponse>> SubmitQuiz([FromBody] SubmitQuizRequest request)
        {
            try
            {
                _logger.LogInformation("Submitting quiz {QuizId} for learner {LearnerId}", 
                    request.QuizId, request.LearnerId);
                
                var response = await _quizService.SubmitQuizAsync(request);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting quiz");
                return StatusCode(500, new { error = "Failed to submit quiz", details = ex.Message });
            }
        }
    }
}

