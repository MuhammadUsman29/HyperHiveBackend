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

        /// <summary>
        /// Get detailed results for a specific quiz attempt
        /// </summary>
        [HttpGet("attempt/{attemptId}")]
        public async Task<ActionResult<SubmitQuizResponse>> GetQuizAttemptResults(int attemptId)
        {
            try
            {
                _logger.LogInformation("Retrieving results for attempt {AttemptId}", attemptId);
                
                var response = await _quizService.GetQuizAttemptResultsAsync(attemptId);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quiz attempt results");
                
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(new { error = ex.Message });
                }
                
                return StatusCode(500, new { error = "Failed to retrieve quiz attempt results", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all quiz attempts for a specific learner
        /// </summary>
        [HttpGet("learner/{learnerId}/attempts")]
        public async Task<ActionResult<List<QuizAttemptSummary>>> GetLearnerQuizAttempts(int learnerId)
        {
            try
            {
                _logger.LogInformation("Retrieving quiz attempts for learner {LearnerId}", learnerId);
                
                var attempts = await _quizService.GetLearnerQuizAttemptsAsync(learnerId);
                
                return Ok(attempts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving learner quiz attempts");
                return StatusCode(500, new { error = "Failed to retrieve quiz attempts", details = ex.Message });
            }
        }

        /// <summary>
        /// Get quiz statistics for a specific learner
        /// </summary>
        [HttpGet("learner/{learnerId}/statistics")]
        public async Task<ActionResult<LearnerQuizStatistics>> GetLearnerQuizStatistics(int learnerId)
        {
            try
            {
                _logger.LogInformation("Retrieving quiz statistics for learner {LearnerId}", learnerId);
                
                var statistics = await _quizService.GetLearnerQuizStatisticsAsync(learnerId);
                
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving learner quiz statistics");
                return StatusCode(500, new { error = "Failed to retrieve quiz statistics", details = ex.Message });
            }
        }

        /// <summary>
        /// Get quiz details (metadata only, no questions)
        /// </summary>
        [HttpGet("{quizId}")]
        public async Task<ActionResult<QuizDetailsResponse>> GetQuizDetails(int quizId)
        {
            try
            {
                _logger.LogInformation("Retrieving quiz details for quiz {QuizId}", quizId);
                
                var quizDetails = await _quizService.GetQuizDetailsAsync(quizId);
                
                return Ok(quizDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quiz details");
                
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(new { error = ex.Message });
                }
                
                return StatusCode(500, new { error = "Failed to retrieve quiz details", details = ex.Message });
            }
        }
    }
}

