using HyperHiveBackend.DTOs;
using HyperHiveBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace HyperHiveBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileValidationController : ControllerBase
    {
        private readonly IProfileValidationService _validationService;
        private readonly ILogger<ProfileValidationController> _logger;

        public ProfileValidationController(
            IProfileValidationService validationService,
            ILogger<ProfileValidationController> logger)
        {
            _validationService = validationService;
            _logger = logger;
        }

        /// <summary>
        /// Validate learner profile by comparing claimed skills with GitHub profile
        /// Uses hardcoded repository from configuration
        /// </summary>
        [HttpPost("validate")]
        public async Task<ActionResult<ProfileValidationResponse>> ValidateLearnerProfile(
            [FromBody] ValidateLearnerProfileRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Validating profile for learner {LearnerId} with GitHub username {Username}",
                    request.LearnerId,
                    request.GitHubUsername);

                var result = await _validationService.ValidateLearnerProfileAsync(
                    request.LearnerId,
                    request.GitHubUsername);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating learner profile");
                return StatusCode(500, new 
                { 
                    error = "Failed to validate profile", 
                    details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Validate learner profile using path parameters (alternative endpoint)
        /// Uses hardcoded repository from configuration
        /// </summary>
        [HttpGet("validate/{learnerId}/{githubUsername}")]
        public async Task<ActionResult<ProfileValidationResponse>> ValidateLearnerProfileByPath(
            int learnerId,
            string githubUsername)
        {
            try
            {
                _logger.LogInformation(
                    "Validating profile for learner {LearnerId} with GitHub username {Username}",
                    learnerId,
                    githubUsername);

                var result = await _validationService.ValidateLearnerProfileAsync(
                    learnerId,
                    githubUsername);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating learner profile");
                return StatusCode(500, new 
                { 
                    error = "Failed to validate profile", 
                    details = ex.Message 
                });
            }
        }
    }
}

