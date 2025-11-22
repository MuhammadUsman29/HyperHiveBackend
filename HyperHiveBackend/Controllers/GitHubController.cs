using HyperHiveBackend.Models;
using HyperHiveBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HyperHiveBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] // Temporarily disabled for testing - uncomment to enable authentication
public class GitHubController : ControllerBase
{
    private readonly IGitHubService _gitHubService;
    private readonly ILogger<GitHubController> _logger;

    public GitHubController(IGitHubService gitHubService, ILogger<GitHubController> logger)
    {
        _gitHubService = gitHubService;
        _logger = logger;
    }

    /// <summary>
    /// Analyze developer's strong areas and concepts used from GitHub contributions
    /// </summary>
    /// <param name="request">GitHub analysis request with repository and user details</param>
    /// <returns>Developer strong areas analysis</returns>
    [HttpPost("analyze-developer")]
    public async Task<ActionResult<DeveloperStrongAreas>> AnalyzeDeveloper([FromBody] GitHubAnalysisRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Owner))
            {
                return BadRequest(new { message = "Owner is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Repository))
            {
                return BadRequest(new { message = "Repository is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest(new { message = "Username is required" });
            }

            var analysis = await _gitHubService.AnalyzeDeveloperStrongAreasAsync(request);

            return Ok(analysis);
            // ytest
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing developer strong areas");
            return StatusCode(500, new { message = "An error occurred while analyzing developer data" });
        }
    }

    /// <summary>
    /// Get commits for a specific user in a GitHub repository
    /// </summary>
    [HttpGet("commits")]
    public async Task<ActionResult<List<GitHubCommit>>> GetCommits(
        [FromQuery] string owner,
        [FromQuery] string repository,
        [FromQuery] string username,
        [FromQuery] DateTime? since = null,
        [FromQuery] DateTime? until = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(owner) || 
                string.IsNullOrWhiteSpace(repository) || 
                string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new { message = "Owner, Repository, and Username are required" });
            }

            var commits = await _gitHubService.GetUserCommitsAsync(owner, repository, username, "", since, until);

            return Ok(commits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching commits");
            return StatusCode(500, new { message = "An error occurred while fetching commits" });
        }
    }

    /// <summary>
    /// Get pull requests for a specific user in a GitHub repository
    /// </summary>
    [HttpGet("pull-requests")]
    public async Task<ActionResult<List<GitHubPullRequest>>> GetPullRequests(
        [FromQuery] string owner,
        [FromQuery] string repository,
        [FromQuery] string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(owner) || 
                string.IsNullOrWhiteSpace(repository) || 
                string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new { message = "Owner, Repository, and Username are required" });
            }

            var pullRequests = await _gitHubService.GetUserPullRequestsAsync(owner, repository, username, "");

            return Ok(pullRequests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pull requests");
            return StatusCode(500, new { message = "An error occurred while fetching pull requests" });
        }
    }
}

