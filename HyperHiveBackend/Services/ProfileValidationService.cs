using HyperHiveBackend.Data;
using HyperHiveBackend.DTOs;
using HyperHiveBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using OpenAI.Chat;

namespace HyperHiveBackend.Services
{
    public interface IProfileValidationService
    {
        Task<ProfileValidationResponse> ValidateLearnerProfileAsync(int learnerId, string gitHubUsername);
    }

    public class ProfileValidationService : IProfileValidationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGitHubService _gitHubService;
        private readonly IOpenAIService _openAIService;
        private readonly IConfigurationService _configService;
        private readonly ILogger<ProfileValidationService> _logger;

        public ProfileValidationService(
            ApplicationDbContext context,
            IGitHubService gitHubService,
            IOpenAIService openAIService,
            IConfigurationService configService,
            ILogger<ProfileValidationService> logger)
        {
            _context = context;
            _gitHubService = gitHubService;
            _openAIService = openAIService;
            _configService = configService;
            _logger = logger;
        }

        public async Task<ProfileValidationResponse> ValidateLearnerProfileAsync(
            int learnerId, 
            string gitHubUsername)
        {
            // Get hardcoded repository information from configuration
            var repositoryOwner = _configService.GetGitHubRepoOwner();
            var repositoryName = _configService.GetGitHubRepoName();

            _logger.LogInformation(
                "Starting profile validation for learner {LearnerId} with GitHub: {Username}, Repo: {Owner}/{Repo}", 
                learnerId, gitHubUsername, repositoryOwner, repositoryName);

            // 1. Get learner profile
            var learner = await _context.Learners.FindAsync(learnerId);
            if (learner == null)
            {
                throw new Exception($"Learner with ID {learnerId} not found");
            }

            // 2. Parse learner's claimed skills from AI profile
            LearnerAIProfile? learnerProfile = null;
            List<string> claimedSkills = new();
            
            if (!string.IsNullOrEmpty(learner.AIProfileData))
            {
                try
                {
                    learnerProfile = JsonSerializer.Deserialize<LearnerAIProfile>(learner.AIProfileData);
                    claimedSkills = learnerProfile?.Skills ?? new List<string>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse learner AI profile");
                }
            }

            if (!claimedSkills.Any())
            {
                throw new Exception("Learner has no skills claimed in their profile");
            }

            // 3. Get GitHub profile data using existing service
            _logger.LogInformation("Analyzing GitHub contributions for {Username} in {Owner}/{Repo}", 
                gitHubUsername, repositoryOwner, repositoryName);
            
            // Use the analyze method to get comprehensive GitHub data
            var githubAnalysis = await _gitHubService.AnalyzeDeveloperStrongAreasAsync(new GitHubAnalysisRequest
            {
                Owner = repositoryOwner,
                Repository = repositoryName,
                Username = gitHubUsername
            });

            // Extract GitHub skills from analysis
            var githubSkills = ExtractSkillsFromGitHubAnalysis(githubAnalysis);

            // 5. Compare skills
            var skillsComparison = CompareSkills(claimedSkills, githubSkills);

            // 6. Use OpenAI to analyze and validate
            var aiValidation = await AnalyzeWithOpenAI(learnerProfile, githubAnalysis, skillsComparison);

            // 7. Create GitHub profile summary from analysis
            var githubProfileSummary = new GitHubProfileSummary
            {
                Username = gitHubUsername,
                PublicRepos = githubAnalysis.TotalCommits, // Approximate
                TopLanguages = githubAnalysis.Languages.Select(l => l.Language).ToList(),
                TopicInterests = githubAnalysis.Technologies.Select(t => t.Technology).Take(10).ToList(),
                TotalCommits = githubAnalysis.TotalCommits,
                YearsActive = 0 // We don't have this data from analysis
            };

            // 8. Build response
            var response = new ProfileValidationResponse
            {
                LearnerId = learnerId,
                GitHubUsername = gitHubUsername,
                ValidationScore = aiValidation.Score,
                ValidationLevel = GetValidationLevel(aiValidation.Score),
                GitHubProfile = githubProfileSummary,
                SkillsComparison = skillsComparison,
                AIAnalysis = aiValidation.Analysis,
                Recommendations = aiValidation.Recommendations,
                ValidatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Profile validation completed for learner {LearnerId}. Score: {Score}", 
                learnerId, aiValidation.Score);

            return response;
        }

        private List<string> ExtractSkillsFromGitHubAnalysis(DeveloperStrongAreas analysis)
        {
            var skills = new List<string>();

            // Add languages
            skills.AddRange(analysis.Languages.Select(l => l.Language));

            // Add technologies
            skills.AddRange(analysis.Technologies.Select(t => t.Technology));

            // Add concepts that are skill-related
            var skillConcepts = new[] { "Async/Await", "LINQ", "REST API", "GraphQL", "gRPC" };
            skills.AddRange(analysis.Concepts
                .Where(c => skillConcepts.Contains(c.Concept))
                .Select(c => c.Concept));

            return skills.Distinct().ToList();
        }

        private SkillsComparison CompareSkills(List<string> claimedSkills, List<string> githubSkills)
        {
            // Normalize skills for comparison (case-insensitive)
            var normalizedClaimed = claimedSkills.Select(s => s.ToLower().Trim()).ToList();
            var normalizedGitHub = githubSkills.Select(s => s.ToLower().Trim()).ToList();

            // Find matches (skills that appear in both)
            var matched = normalizedClaimed
                .Where(cs => normalizedGitHub.Any(gs => 
                    gs.Contains(cs) || cs.Contains(gs) || LevenshteinDistance(cs, gs) <= 2))
                .Select(s => claimedSkills.First(original => original.ToLower() == s))
                .ToList();

            // Unverified skills (claimed but not found in GitHub)
            var unverified = claimedSkills
                .Where(cs => !matched.Contains(cs, StringComparer.OrdinalIgnoreCase))
                .ToList();

            // Additional skills found in GitHub but not claimed
            var additional = githubSkills
                .Where(gs => !normalizedClaimed.Any(cs => 
                    cs.Contains(gs.ToLower()) || gs.ToLower().Contains(cs)))
                .ToList();

            // Calculate match percentage
            decimal matchPercentage = claimedSkills.Any() 
                ? (decimal)matched.Count / claimedSkills.Count * 100 
                : 0;

            return new SkillsComparison
            {
                ClaimedSkills = claimedSkills,
                GitHubSkills = githubSkills,
                MatchedSkills = matched,
                UnverifiedSkills = unverified,
                AdditionalGitHubSkills = additional,
                MatchPercentage = Math.Round(matchPercentage, 2)
            };
        }

        private async Task<AIValidationResult> AnalyzeWithOpenAI(
            LearnerAIProfile? learnerProfile, 
            DeveloperStrongAreas githubAnalysis, 
            SkillsComparison skillsComparison)
        {
            var prompt = $@"
You are an expert at validating software engineer profiles. Analyze the following data and provide a validation score.

LEARNER'S CLAIMED PROFILE:
{JsonSerializer.Serialize(learnerProfile, new JsonSerializerOptions { WriteIndented = true })}

GITHUB PROFILE ANALYSIS:
- Username: {githubAnalysis.DeveloperUsername}
- Total Commits: {githubAnalysis.TotalCommits}
- Total Pull Requests: {githubAnalysis.TotalPullRequests}
- Lines Added: {githubAnalysis.TotalLinesAdded}
- Lines Deleted: {githubAnalysis.TotalLinesDeleted}

LANGUAGES USED:
{string.Join(", ", githubAnalysis.Languages.Select(l => $"{l.Language} ({l.Percentage:F1}%)"))}

TECHNOLOGIES:
{string.Join(", ", githubAnalysis.Technologies.Take(10).Select(t => t.Technology))}

DOMAIN AREAS:
{string.Join(", ", githubAnalysis.DomainAreas.Select(d => d.Area))}

SKILLS COMPARISON:
- Claimed Skills: {string.Join(", ", skillsComparison.ClaimedSkills)}
- GitHub Skills/Languages: {string.Join(", ", skillsComparison.GitHubSkills)}
- Matched Skills: {string.Join(", ", skillsComparison.MatchedSkills)}
- Unverified Skills: {string.Join(", ", skillsComparison.UnverifiedSkills)}
- Match Percentage: {skillsComparison.MatchPercentage}%

TASK:
1. Provide a validation score from 0-100 based on how well the claimed profile matches GitHub activity
2. Consider: skill matches, GitHub activity level (commits, PRs), technology usage
3. Provide brief analysis (2-3 sentences)
4. Give 3 specific recommendations

Return ONLY valid JSON in this format:
{{
  ""score"": 85,
  ""analysis"": ""Brief analysis here..."",
  ""recommendations"": [
    ""Recommendation 1"",
    ""Recommendation 2"",
    ""Recommendation 3""
  ]
}}";

            try
            {
                var result = await _openAIService.GenerateValidationAnalysisAsync(prompt);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI validation");
                
                // Fallback to rule-based validation
                var score = CalculateRuleBasedScore(skillsComparison, githubAnalysis);
                return new AIValidationResult
                {
                    Score = score,
                    Analysis = $"Based on {githubAnalysis.TotalCommits} commits and {skillsComparison.MatchPercentage:F0}% skill match, the profile shows {(score >= 70 ? "good" : "moderate")} alignment with GitHub activity.",
                    Recommendations = GenerateRecommendations(skillsComparison, githubAnalysis)
                };
            }
        }

        private int CalculateRuleBasedScore(SkillsComparison comparison, DeveloperStrongAreas analysis)
        {
            int score = 0;

            // Skill match (40 points max)
            score += (int)((double)comparison.MatchPercentage * 0.4);

            // GitHub activity (30 points max)
            int commits = analysis.TotalCommits;
            score += Math.Min(commits / 10, 30);

            // Technology diversity (15 points max)
            int techCount = analysis.Technologies.Count;
            score += Math.Min(techCount, 15);

            // Domain coverage (15 points max)
            int domainCount = analysis.DomainAreas.Count;
            score += Math.Min(domainCount * 3, 15);

            return Math.Min(score, 100);
        }

        private List<string> GenerateRecommendations(SkillsComparison comparison, DeveloperStrongAreas analysis)
        {
            var recommendations = new List<string>();

            if (comparison.UnverifiedSkills.Any())
            {
                recommendations.Add($"Create public projects showcasing: {string.Join(", ", comparison.UnverifiedSkills.Take(3))}");
            }

            if (comparison.AdditionalGitHubSkills.Any())
            {
                recommendations.Add($"Consider adding these skills to your profile: {string.Join(", ", comparison.AdditionalGitHubSkills.Take(3))}");
            }

            if (analysis.TotalCommits < 50)
            {
                recommendations.Add("Increase your GitHub activity by contributing to open-source projects or creating personal projects");
            }
            else
            {
                recommendations.Add("Continue maintaining consistent GitHub activity to strengthen your profile");
            }

            // Ensure we always have 3 recommendations
            while (recommendations.Count < 3)
            {
                recommendations.Add("Keep learning and building projects in your areas of interest");
            }

            return recommendations.Take(3).ToList();
        }

        private int CalculateYearsActive(DateTime? createdAt)
        {
            if (!createdAt.HasValue) return 0;
            return (int)((DateTime.UtcNow - createdAt.Value).TotalDays / 365);
        }

        private string GetValidationLevel(int score)
        {
            return score switch
            {
                >= 85 => "Excellent",
                >= 70 => "Good",
                >= 50 => "Fair",
                _ => "Needs Improvement"
            };
        }

        private int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }
    }

    // Helper class for AI validation result
    public class AIValidationResult
    {
        public int Score { get; set; }
        public string Analysis { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }
}

