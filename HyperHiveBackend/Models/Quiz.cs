namespace HyperHiveBackend.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public int LearnerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string QuizType { get; set; } = string.Empty; // "SkillAssessment", "KnowledgeCheck", etc.
        public string Difficulty { get; set; } = string.Empty; // "beginner", "intermediate", "advanced"
        
        // JSON column to store quiz questions with correct answers
        // Structure: { questions: [ { question, options, correctAnswer, explanation } ] }
        public string QuizData { get; set; } = string.Empty;
        
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        
        // Navigation property
        public Learner? Learner { get; set; }
        
        // Collection of attempts for this quiz
        public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    }
}

