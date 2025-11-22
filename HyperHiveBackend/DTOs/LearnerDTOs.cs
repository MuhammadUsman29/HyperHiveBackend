namespace HyperHiveBackend.DTOs
{
    // Request to create a new learner
    public class CreateLearnerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime JoinedDate { get; set; }
        public string? Bio { get; set; }
        public LearnerAIProfile? AIProfile { get; set; }
    }

    // Request to update learner
    public class UpdateLearnerRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }
        public DateTime? JoinedDate { get; set; }
        public string? Bio { get; set; }
        public LearnerAIProfile? AIProfile { get; set; }
    }

    // AI Profile data structure
    public class LearnerAIProfile
    {
        public List<string> Skills { get; set; } = new();
        public List<string> Interests { get; set; } = new();
        public List<string> Goals { get; set; } = new();
        public string CurrentLevel { get; set; } = string.Empty; // "beginner", "intermediate", "advanced"
        public string LearningStyle { get; set; } = string.Empty; // "visual", "hands-on", "reading", "auditory"
        public int AvailableHoursPerWeek { get; set; }
        public string PreferredLearningTime { get; set; } = string.Empty; // "morning", "afternoon", "evening"
        public string YearsOfExperience { get; set; } = string.Empty;
        public List<string> PreferredTopics { get; set; } = new();
        public List<string> WeakAreas { get; set; } = new();
    }

    // Response with learner data
    public class LearnerResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime JoinedDate { get; set; }
        public string? Bio { get; set; }
        public LearnerAIProfile? AIProfile { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // List response
    public class LearnersListResponse
    {
        public List<LearnerResponse> Learners { get; set; } = new();
        public int TotalCount { get; set; }
    }
}

