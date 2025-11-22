namespace HyperHiveBackend.Models
{
    public class QuizAttempt
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int LearnerId { get; set; }
        
        // JSON column to store learner's answers
        // Structure: { answers: [ { questionId, selectedAnswer } ] }
        public string LearnerAnswers { get; set; } = string.Empty;
        
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public decimal Percentage { get; set; }
        
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public int? TimeTakenSeconds { get; set; }
        
        // Navigation properties
        public Quiz? Quiz { get; set; }
        public Learner? Learner { get; set; }
    }
}

