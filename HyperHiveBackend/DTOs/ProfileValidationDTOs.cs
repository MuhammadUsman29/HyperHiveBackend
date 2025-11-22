namespace HyperHiveBackend.DTOs
{
    // Request to validate learner profile with GitHub
    public class ValidateLearnerProfileRequest
    {
        public int LearnerId { get; set; }
        public string GitHubUsername { get; set; } = string.Empty;
    }

    // GitHub profile validation result
    public class ProfileValidationResponse
    {
        public int LearnerId { get; set; }
        public string GitHubUsername { get; set; } = string.Empty;
        public int ValidationScore { get; set; } // 0-100
        public string ValidationLevel { get; set; } = string.Empty; // "Excellent", "Good", "Fair", "Poor"
        public GitHubProfileSummary GitHubProfile { get; set; } = new();
        public SkillsComparison SkillsComparison { get; set; } = new();
        public string AIAnalysis { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
        public DateTime ValidatedAt { get; set; }
    }

    // GitHub profile summary
    public class GitHubProfileSummary
    {
        public string Username { get; set; } = string.Empty;
        public int PublicRepos { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
        public List<string> TopLanguages { get; set; } = new();
        public List<string> TopicInterests { get; set; } = new();
        public string Bio { get; set; } = string.Empty;
        public int TotalCommits { get; set; }
        public int YearsActive { get; set; }
    }

    // Skills comparison result
    public class SkillsComparison
    {
        public List<string> ClaimedSkills { get; set; } = new();
        public List<string> GitHubSkills { get; set; } = new();
        public List<string> MatchedSkills { get; set; } = new();
        public List<string> UnverifiedSkills { get; set; } = new();
        public List<string> AdditionalGitHubSkills { get; set; } = new();
        public decimal MatchPercentage { get; set; }
    }
}

