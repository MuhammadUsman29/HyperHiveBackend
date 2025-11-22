using HyperHiveBackend.Data;
using HyperHiveBackend.DTOs;
using HyperHiveBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HyperHiveBackend.Services
{
    public interface IGrowthPlanService
    {
        Task<GrowthPlanResponse> GenerateGrowthPlanAsync(int learnerId);
    }

    public class GrowthPlanService : IGrowthPlanService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<GrowthPlanService> _logger;

        // Career progression mapping
        private readonly Dictionary<string, string> _careerProgression = new()
        {
            { "beginner", "intermediate" },
            { "junior", "mid-level" },
            { "mid-level", "senior" },
            { "intermediate", "senior" },
            { "senior", "team lead" },
            { "team lead", "architect" },
            { "lead", "architect" }
        };

        // Required skills for each level
        private readonly Dictionary<string, List<string>> _levelRequiredSkills = new()
        {
            { "senior", new List<string> { 
                "System Design", "Architecture Patterns", "Design Patterns", 
                "Clean Architecture", "Microservices", "Performance Optimization",
                "Code Review", "Mentoring", "Technical Documentation"
            }},
            { "team lead", new List<string> { 
                "Leadership", "Team Management", "Project Planning", 
                "Agile/Scrum", "Stakeholder Communication", "Technical Strategy",
                "Conflict Resolution", "Performance Management", "Roadmap Planning"
            }},
            { "architect", new List<string> { 
                "Enterprise Architecture", "Solution Architecture", "Cloud Architecture",
                "Scalability Design", "Security Architecture", "Technology Evaluation",
                "Cross-functional Collaboration", "Architecture Documentation", "Technical Vision"
            }}
        };

        public GrowthPlanService(
            ApplicationDbContext context,
            IOpenAIService openAIService,
            ILogger<GrowthPlanService> logger)
        {
            _context = context;
            _openAIService = openAIService;
            _logger = logger;
        }

        public async Task<GrowthPlanResponse> GenerateGrowthPlanAsync(int learnerId)
        {
            _logger.LogInformation("Generating growth plan for learner {LearnerId}", learnerId);

            // 1. Gather learner data
            var learner = await _context.Learners.FindAsync(learnerId);
            if (learner == null)
            {
                throw new Exception($"Learner with ID {learnerId} not found");
            }

            // 2. Parse learner profile
            LearnerAIProfile? profile = null;
            if (!string.IsNullOrEmpty(learner.AIProfileData))
            {
                profile = JsonSerializer.Deserialize<LearnerAIProfile>(learner.AIProfileData);
            }

            if (profile == null)
            {
                throw new Exception("Learner profile data is missing");
            }

            // 3. Get quiz results
            var quizAttempts = await _context.QuizAttempts
                .Where(a => a.LearnerId == learnerId)
                .OrderByDescending(a => a.CompletedAt)
                .Take(5)
                .ToListAsync();

            // 4. Determine current and target levels
            var currentLevel = DetermineCurrentLevel(profile.CurrentLevel);
            var targetLevel = GetNextCareerLevel(currentLevel);

            // 5. Identify skill gaps
            var skillGaps = IdentifySkillGaps(profile, targetLevel, quizAttempts);

            // 6. Generate AI-powered growth plan
            var aiGrowthPlan = await GenerateAIGrowthPlanAsync(
                learner, 
                profile, 
                currentLevel, 
                targetLevel, 
                skillGaps,
                quizAttempts);

            return aiGrowthPlan;
        }

        private string DetermineCurrentLevel(string? claimedLevel)
        {
            if (string.IsNullOrEmpty(claimedLevel))
                return "intermediate";

            var normalized = claimedLevel.ToLower().Trim();
            
            // Map variations to standard levels
            if (normalized.Contains("junior") || normalized.Contains("beginner"))
                return "mid-level";
            if (normalized.Contains("mid") || normalized.Contains("intermediate"))
                return "senior";
            if (normalized.Contains("senior"))
                return "team lead";
            if (normalized.Contains("lead"))
                return "architect";

            return "senior"; // Default
        }

        private string GetNextCareerLevel(string currentLevel)
        {
            var normalized = currentLevel.ToLower();
            return _careerProgression.GetValueOrDefault(normalized, "senior");
        }

        private List<SkillGap> IdentifySkillGaps(
            LearnerAIProfile profile, 
            string targetLevel,
            List<QuizAttempt> quizAttempts)
        {
            var gaps = new List<SkillGap>();
            var currentSkills = profile.Skills ?? new List<string>();
            var requiredSkills = _levelRequiredSkills.GetValueOrDefault(targetLevel.ToLower(), new List<string>());

            // Find missing skills
            foreach (var requiredSkill in requiredSkills)
            {
                var hasSkill = currentSkills.Any(s => 
                    s.Contains(requiredSkill, StringComparison.OrdinalIgnoreCase) ||
                    requiredSkill.Contains(s, StringComparison.OrdinalIgnoreCase));

                if (!hasSkill)
                {
                    gaps.Add(new SkillGap
                    {
                        SkillName = requiredSkill,
                        CurrentProficiency = "None",
                        TargetProficiency = "Advanced",
                        Priority = "High",
                        Reasoning = $"Required for {targetLevel} level"
                    });
                }
            }

            // Add weak areas from profile
            if (profile.WeakAreas != null)
            {
                foreach (var weakArea in profile.WeakAreas)
                {
                    if (!gaps.Any(g => g.SkillName.Equals(weakArea, StringComparison.OrdinalIgnoreCase)))
                    {
                        gaps.Add(new SkillGap
                        {
                            SkillName = weakArea,
                            CurrentProficiency = "Basic",
                            TargetProficiency = "Intermediate",
                            Priority = "Medium",
                            Reasoning = "Self-identified weak area"
                        });
                    }
                }
            }

            return gaps;
        }

        private async Task<GrowthPlanResponse> GenerateAIGrowthPlanAsync(
            Learner learner,
            LearnerAIProfile profile,
            string currentLevel,
            string targetLevel,
            List<SkillGap> skillGaps,
            List<QuizAttempt> quizAttempts)
        {
            var quizSummary = quizAttempts.Any()
                ? $"Average quiz score: {quizAttempts.Average(q => q.Percentage):F1}%"
                : "No quiz data available";

            var prompt = $@"
You are an expert career development advisor for software engineers. Generate a comprehensive growth plan.

LEARNER PROFILE:
- Name: {learner.Name}
- Current Level: {currentLevel}
- Target Level: {targetLevel}
- Position: {learner.Position}
- Department: {learner.Department}
- Years of Experience: {profile.YearsOfExperience}

CURRENT SKILLS:
{string.Join(", ", profile.Skills ?? new List<string>())}

INTERESTS:
{string.Join(", ", profile.Interests ?? new List<string>())}

GOALS:
{string.Join(", ", profile.Goals ?? new List<string>())}

LEARNING STYLE: {profile.LearningStyle}
AVAILABLE HOURS PER WEEK: {profile.AvailableHoursPerWeek}

IDENTIFIED SKILL GAPS:
{string.Join("\n", skillGaps.Select(g => $"- {g.SkillName} ({g.Priority} priority): {g.Reasoning}"))}

QUIZ PERFORMANCE:
{quizSummary}

TASK:
Create a detailed growth plan to progress from {currentLevel} to {targetLevel}.

CAREER PROGRESSION RULES:
- If mid-level → focus on: System Design, Architecture, Code Quality, Mentoring
- If senior → focus on: Leadership, Team Management, Technical Strategy, Cross-team Collaboration  
- If team lead → focus on: Enterprise Architecture, Vision Setting, Stakeholder Management, Technical Direction

REQUIREMENTS:
1. Create 2-3 learning phases with TOTAL DURATION OF MAXIMUM 6 WEEKS
2. Each phase should be 2-3 weeks long
3. For each phase include: title, description, skills to cover, practical projects, success metrics
4. Focus on identified skill gaps (especially: {string.Join(", ", skillGaps.Take(5).Select(g => g.SkillName))})
5. Recommend SPECIFIC learning resources with:
   - Real course/book/article names
   - Providers (Udemy, Coursera, Microsoft Learn, Pluralsight, etc.)
   - URLs (use realistic URLs even if approximate)
   - Difficulty level
   - Estimated hours
6. Include 3-5 key milestones
7. Define success criteria
8. CRITICAL: Total duration must not exceed 6 weeks

Return ONLY valid JSON in this EXACT format:
{{
  ""overview"": ""Brief overview of the growth plan..."",
  ""estimatedDurationMonths"": 2,
  ""learningPhases"": [
    {{
      ""phaseNumber"": 1,
      ""title"": ""Phase Title"",
      ""description"": ""Phase description..."",
      ""durationWeeks"": 2,
      ""skillsToCover"": [""Skill 1"", ""Skill 2""],
      ""learningObjectives"": [""Objective 1"", ""Objective 2""],
      ""practicalProjects"": [""Project 1"", ""Project 2""],
      ""successMetrics"": ""How to measure success""
    }}
  ],
  ""recommendedResources"": [
    {{
      ""title"": ""Clean Architecture Course"",
      ""type"": ""Course"",
      ""url"": ""https://www.udemy.com/course/clean-architecture"",
      ""provider"": ""Udemy"",
      ""description"": ""Learn clean architecture principles"",
      ""skillsCovered"": [""Clean Architecture"", ""SOLID Principles""],
      ""difficulty"": ""Intermediate"",
      ""isFree"": false,
      ""estimatedHours"": 12
    }}
  ],
  ""keyMilestones"": [
    ""Milestone 1"",
    ""Milestone 2""
  ],
  ""successCriteria"": ""What defines success for this plan""
}}";

            try
            {
                _logger.LogInformation("Requesting AI growth plan generation");
                var aiResult = await _openAIService.GenerateGrowthPlanAsync(prompt);

                // Validate and enforce 6-week limit
                var totalWeeks = aiResult.LearningPhases.Sum(p => p.DurationWeeks);
                if (totalWeeks > 6)
                {
                    _logger.LogWarning("AI generated plan exceeds 6 weeks ({TotalWeeks} weeks). Adjusting...", totalWeeks);
                    aiResult = AdjustPlanTo6Weeks(aiResult);
                }

                // Combine with our data
                var response = new GrowthPlanResponse
                {
                    LearnerId = learner.Id,
                    LearnerName = learner.Name,
                    CurrentLevel = currentLevel,
                    TargetLevel = targetLevel,
                    EstimatedDurationMonths = aiResult.EstimatedDurationMonths,
                    Overview = aiResult.Overview,
                    SkillGaps = skillGaps,
                    LearningPhases = aiResult.LearningPhases,
                    RecommendedResources = aiResult.RecommendedResources,
                    KeyMilestones = aiResult.KeyMilestones,
                    SuccessCriteria = aiResult.SuccessCriteria,
                    GeneratedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Growth plan generated successfully for learner {LearnerId}", learner.Id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI growth plan");
                
                // Fallback to template-based plan
                return CreateFallbackGrowthPlan(learner, profile, currentLevel, targetLevel, skillGaps);
            }
        }

        private AIGrowthPlanResult AdjustPlanTo6Weeks(AIGrowthPlanResult plan)
        {
            var totalWeeks = plan.LearningPhases.Sum(p => p.DurationWeeks);
            var scaleFactor = 6.0 / totalWeeks;

            // Scale down each phase proportionally
            foreach (var phase in plan.LearningPhases)
            {
                phase.DurationWeeks = Math.Max(1, (int)Math.Round(phase.DurationWeeks * scaleFactor));
            }

            // Ensure total is exactly 6 weeks
            var adjustedTotal = plan.LearningPhases.Sum(p => p.DurationWeeks);
            if (adjustedTotal < 6 && plan.LearningPhases.Any())
            {
                plan.LearningPhases.First().DurationWeeks += (6 - adjustedTotal);
            }
            else if (adjustedTotal > 6 && plan.LearningPhases.Any())
            {
                plan.LearningPhases.Last().DurationWeeks -= (adjustedTotal - 6);
            }

            // Update estimated duration in months (6 weeks = 1.5 months)
            plan.EstimatedDurationMonths = 2; // Round up to 2 months

            return plan;
        }

        private GrowthPlanResponse CreateFallbackGrowthPlan(
            Learner learner,
            LearnerAIProfile profile,
            string currentLevel,
            string targetLevel,
            List<SkillGap> skillGaps)
        {
            return new GrowthPlanResponse
            {
                LearnerId = learner.Id,
                LearnerName = learner.Name,
                CurrentLevel = currentLevel,
                TargetLevel = targetLevel,
                EstimatedDurationMonths = 2,
                Overview = $"Intensive 6-week growth plan to progress from {currentLevel} to {targetLevel} level, focusing on key skill gaps and career development.",
                SkillGaps = skillGaps,
                LearningPhases = CreateDefaultPhases(targetLevel, skillGaps),
                RecommendedResources = CreateDefaultResources(skillGaps),
                KeyMilestones = new List<string>
                {
                    "Complete foundational courses in identified skill gaps",
                    "Build and deploy 2-3 practical projects",
                    "Contribute to team knowledge sharing",
                    "Take on increased responsibilities",
                    $"Demonstrate {targetLevel}-level competencies"
                },
                SuccessCriteria = $"Successfully transition to {targetLevel} role with demonstrated competency in all required skills",
                GeneratedAt = DateTime.UtcNow
            };
        }

        private List<LearningPhase> CreateDefaultPhases(string targetLevel, List<SkillGap> skillGaps)
        {
            var phases = new List<LearningPhase>();
            var numPhases = 3;
            var weeksPerPhase = 2; // 3 phases × 2 weeks = 6 weeks total
            var skillsPerPhase = Math.Max(1, skillGaps.Count / numPhases);

            for (int i = 0; i < numPhases; i++)
            {
                var phaseSkills = skillGaps.Skip(i * skillsPerPhase).Take(skillsPerPhase).Select(g => g.SkillName).ToList();
                
                if (!phaseSkills.Any())
                    break;

                phases.Add(new LearningPhase
                {
                    PhaseNumber = i + 1,
                    Title = $"Week {(i * 2) + 1}-{(i + 1) * 2}: {GetPhaseTitle(i, targetLevel)}",
                    Description = $"Intensive focus on {string.Join(", ", phaseSkills)}",
                    DurationWeeks = weeksPerPhase,
                    SkillsToCover = phaseSkills,
                    LearningObjectives = phaseSkills.Select(s => $"Gain foundational knowledge in {s}").ToList(),
                    PracticalProjects = new List<string> { $"Quick project applying {phaseSkills.FirstOrDefault()}" },
                    SuccessMetrics = "Complete objectives and mini-project"
                });
            }

            return phases;
        }

        private string GetPhaseTitle(int phaseIndex, string targetLevel)
        {
            return phaseIndex switch
            {
                0 => "Foundation & Fundamentals",
                1 => "Core Concepts & Practice",
                2 => $"Application & {targetLevel} Skills",
                _ => "Advanced Topics"
            };
        }

        private List<RecommendedResource> CreateDefaultResources(List<SkillGap> skillGaps)
        {
            var resources = new List<RecommendedResource>();

            foreach (var gap in skillGaps.Take(5))
            {
                resources.Add(new RecommendedResource
                {
                    Title = $"Learning {gap.SkillName}",
                    Type = "Course",
                    Url = $"https://learn.microsoft.com/search/?terms={gap.SkillName.Replace(" ", "+")}",
                    Provider = "Microsoft Learn",
                    Description = $"Comprehensive guide to {gap.SkillName}",
                    SkillsCovered = new List<string> { gap.SkillName },
                    Difficulty = "Intermediate",
                    IsFree = true,
                    EstimatedHours = 10
                });
            }

            return resources;
        }
    }
}

