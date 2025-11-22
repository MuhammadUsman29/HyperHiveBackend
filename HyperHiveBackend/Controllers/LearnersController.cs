using HyperHiveBackend.DTOs;
using HyperHiveBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace HyperHiveBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LearnersController : ControllerBase
    {
        private readonly ILearnerService _learnerService;
        private readonly ILogger<LearnersController> _logger;

        public LearnersController(ILearnerService learnerService, ILogger<LearnersController> logger)
        {
            _learnerService = learnerService;
            _logger = logger;
        }

        /// <summary>
        /// Get all learners with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<LearnersListResponse>> GetAllLearners(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var response = await _learnerService.GetAllLearnersAsync(page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learners");
                return StatusCode(500, new { error = "Failed to get learners", details = ex.Message });
            }
        }

        /// <summary>
        /// Get learner by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<LearnerResponse>> GetLearnerById(int id)
        {
            try
            {
                var learner = await _learnerService.GetLearnerByIdAsync(id);
                
                if (learner == null)
                {
                    return NotFound(new { error = $"Learner with ID {id} not found" });
                }

                return Ok(learner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learner {LearnerId}", id);
                return StatusCode(500, new { error = "Failed to get learner", details = ex.Message });
            }
        }

        /// <summary>
        /// Get learner by email
        /// </summary>
        [HttpGet("by-email/{email}")]
        public async Task<ActionResult<LearnerResponse>> GetLearnerByEmail(string email)
        {
            try
            {
                var learner = await _learnerService.GetLearnerByEmailAsync(email);
                
                if (learner == null)
                {
                    return NotFound(new { error = $"Learner with email {email} not found" });
                }

                return Ok(learner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learner by email {Email}", email);
                return StatusCode(500, new { error = "Failed to get learner", details = ex.Message });
            }
        }

        /// <summary>
        /// Create a new learner
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<LearnerResponse>> CreateLearner([FromBody] CreateLearnerRequest request)
        {
            try
            {
                var learner = await _learnerService.CreateLearnerAsync(request);
                return CreatedAtAction(nameof(GetLearnerById), new { id = learner.Id }, learner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating learner");
                return StatusCode(500, new { error = "Failed to create learner", details = ex.Message });
            }
        }

        /// <summary>
        /// Update learner
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<LearnerResponse>> UpdateLearner(int id, [FromBody] UpdateLearnerRequest request)
        {
            try
            {
                var learner = await _learnerService.UpdateLearnerAsync(id, request);
                return Ok(learner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating learner {LearnerId}", id);
                
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(new { error = ex.Message });
                }
                
                return StatusCode(500, new { error = "Failed to update learner", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete learner
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteLearner(int id)
        {
            try
            {
                var success = await _learnerService.DeleteLearnerAsync(id);
                
                if (!success)
                {
                    return NotFound(new { error = $"Learner with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting learner {LearnerId}", id);
                return StatusCode(500, new { error = "Failed to delete learner", details = ex.Message });
            }
        }
    }
}

