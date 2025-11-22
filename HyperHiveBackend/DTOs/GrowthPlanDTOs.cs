namespace HyperHiveBackend.DTOs
{
    // Request to generate growth plan
    public class GenerateGrowthPlanRequest
    {
        public int LearnerId { get; set; }
    }

    // Growth plan response
    public class GrowthPlanResponse
    {
        public int LearnerId { get; set; }
        public string LearnerName { get; set; } = string.Empty;
        public string CurrentLevel { get; set; } = string.Empty;
        public string TargetLevel { get; set; } = string.Empty;
        public int EstimatedDurationMonths { get; set; }
        public string Overview { get; set; } = string.Empty;
        public List<SkillGap> SkillGaps { get; set; } = new();
        public List<LearningPhase> LearningPhases { get; set; } = new();
        public List<RecommendedResource> RecommendedResources { get; set; } = new();
        public List<string> KeyMilestones { get; set; } = new();
        public string SuccessCriteria { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }

    // Skill gap identified
    public class SkillGap
    {
        public string SkillName { get; set; } = string.Empty;
        public string CurrentProficiency { get; set; } = string.Empty; // "None", "Basic", "Intermediate", "Advanced"
        public string TargetProficiency { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty; // "Critical", "High", "Medium", "Low"
        public string Reasoning { get; set; } = string.Empty;
    }

    // Learning phase/module
    public class LearningPhase
    {
        public int PhaseNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationWeeks { get; set; }
        public List<string> SkillsToCover { get; set; } = new();
        public List<string> LearningObjectives { get; set; } = new();
        public List<string> PracticalProjects { get; set; } = new();
        public string SuccessMetrics { get; set; } = string.Empty;
    }

    // Recommended learning resource
    public class RecommendedResource
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Course", "Book", "Article", "Video", "Documentation"
        public string Url { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty; // "Udemy", "Coursera", "Microsoft Learn", etc.
        public string Description { get; set; } = string.Empty;
        public List<string> SkillsCovered { get; set; } = new();
        public string Difficulty { get; set; } = string.Empty;
        public bool IsFree { get; set; }
        public int? EstimatedHours { get; set; }
    }

    // Career level info
    public class CareerLevelInfo
    {
        public string CurrentLevel { get; set; } = string.Empty;
        public string TargetLevel { get; set; } = string.Empty;
        public List<string> RequiredSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
    }
}

