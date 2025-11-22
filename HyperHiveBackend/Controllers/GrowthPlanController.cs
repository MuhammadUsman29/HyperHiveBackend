using HyperHiveBackend.DTOs;
using HyperHiveBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace HyperHiveBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GrowthPlanController : ControllerBase
    {
        private readonly IGrowthPlanService _growthPlanService;
        private readonly ILogger<GrowthPlanController> _logger;

        public GrowthPlanController(
            IGrowthPlanService growthPlanService,
            ILogger<GrowthPlanController> logger)
        {
            _growthPlanService = growthPlanService;
            _logger = logger;
        }

        /// <summary>
        /// Generate a comprehensive AI-powered growth plan for a learner
        /// Analyzes learner profile, quiz results, and career level to create personalized learning path
        /// </summary>
        [HttpPost("generate")]
        public async Task<ActionResult<GrowthPlanResponse>> GenerateGrowthPlan(
            [FromBody] GenerateGrowthPlanRequest request)
        {
            try
            {
                _logger.LogInformation("Generating growth plan for learner {LearnerId}", request.LearnerId);

                var growthPlan = await _growthPlanService.GenerateGrowthPlanAsync(request.LearnerId);

                return Ok(growthPlan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating growth plan");
                return StatusCode(500, new
                {
                    error = "Failed to generate growth plan",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Generate growth plan using path parameter
        /// </summary>
        [HttpGet("generate/{learnerId}")]
        public async Task<ActionResult<GrowthPlanResponse>> GenerateGrowthPlanByPath(int learnerId)
        {
            try
            {
                _logger.LogInformation("Generating growth plan for learner {LearnerId}", learnerId);

                var growthPlan = await _growthPlanService.GenerateGrowthPlanAsync(learnerId);

                return Ok(growthPlan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating growth plan");
                return StatusCode(500, new
                {
                    error = "Failed to generate growth plan",
                    details = ex.Message
                });
            }
        }
    }
}

