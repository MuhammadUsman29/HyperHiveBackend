namespace HyperHiveBackend.DTOs
{
    // Request to generate a quiz
    public class GenerateQuizRequest
    {
        public int LearnerId { get; set; }
        public string QuizType { get; set; } = string.Empty; // "SkillAssessment", "KnowledgeCheck"
        public string? Difficulty { get; set; }
        public int NumberOfQuestions { get; set; } = 5;
    }

    // Response after generating a quiz
    public class GenerateQuizResponse
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<QuizQuestionDto> Questions { get; set; } = new();
    }

    // Quiz question without correct answer (sent to frontend)
    public class QuizQuestionDto
    {
        public int QuestionId { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public string Type { get; set; } = "multiple-choice";
    }

    // Request to submit quiz answers
    public class SubmitQuizRequest
    {
        public int QuizId { get; set; }
        public int LearnerId { get; set; }
        public List<QuizAnswerDto> Answers { get; set; } = new();
    }

    // Individual answer
    public class QuizAnswerDto
    {
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; } = string.Empty;
    }

    // Response after submitting quiz
    public class SubmitQuizResponse
    {
        public int AttemptId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public decimal Percentage { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public List<QuizResultDetail> Results { get; set; } = new();
    }

    // Detailed result for each question
    public class QuizResultDetail
    {
        public int QuestionId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string YourAnswer { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string? Explanation { get; set; }
    }
}

