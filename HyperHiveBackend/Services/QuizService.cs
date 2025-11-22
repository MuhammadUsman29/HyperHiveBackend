using HyperHiveBackend.Data;
using HyperHiveBackend.DTOs;
using HyperHiveBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HyperHiveBackend.Services
{
    public interface IQuizService
    {
        Task<GenerateQuizResponse> GenerateQuizAsync(GenerateQuizRequest request);
        Task<SubmitQuizResponse> SubmitQuizAsync(SubmitQuizRequest request);
        Task<SubmitQuizResponse> GetQuizAttemptResultsAsync(int attemptId);
        Task<List<QuizAttemptSummary>> GetLearnerQuizAttemptsAsync(int learnerId);
        Task<LearnerQuizStatistics> GetLearnerQuizStatisticsAsync(int learnerId);
        Task<QuizDetailsResponse> GetQuizDetailsAsync(int quizId);
        Task<bool> HasLearnerAttemptedQuizAsync(int quizId, int learnerId);
    }

    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<QuizService> _logger;

        public QuizService(
            ApplicationDbContext context,
            IOpenAIService openAIService,
            ILogger<QuizService> logger)
        {
            _context = context;
            _openAIService = openAIService;
            _logger = logger;
        }

        public async Task<GenerateQuizResponse> GenerateQuizAsync(GenerateQuizRequest request)
        {
            // Get learner profile
            var learner = await _context.Learners.FindAsync(request.LearnerId);
            if (learner == null)
            {
                throw new Exception($"Learner with ID {request.LearnerId} not found");
            }

            // Get AI profile data
            var profileData = learner.AIProfileData ?? "{}";
            
            _logger.LogInformation("Generating quiz for learner {LearnerId} with profile: {Profile}", 
                request.LearnerId, profileData);

            // If profile is empty, create a basic one
            if (string.IsNullOrWhiteSpace(profileData) || profileData == "{}")
            {
                profileData = $@"{{
                    ""name"": ""{learner.Name}"",
                    ""position"": ""{learner.Position}"",
                    ""department"": ""{learner.Department}"",
                    ""skills"": [""General Software Development""],
                    ""currentLevel"": ""intermediate""
                }}";
                
                _logger.LogInformation("Using default profile: {Profile}", profileData);
            }

            // Generate quiz using OpenAI
            var quizResult = await _openAIService.GenerateQuizAsync(
                profileData,
                request.QuizType,
                request.NumberOfQuestions
            );

            // Store quiz in database
            var quiz = new Quiz
            {
                LearnerId = request.LearnerId,
                Title = quizResult.Title,
                QuizType = request.QuizType,
                Difficulty = request.Difficulty ?? "intermediate",
                QuizData = JsonSerializer.Serialize(quizResult.Questions),
                GeneratedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30) // Optional: quiz expires after 30 days
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Quiz {QuizId} created successfully", quiz.Id);

            // Prepare response (without correct answers)
            var response = new GenerateQuizResponse
            {
                QuizId = quiz.Id,
                Title = quiz.Title,
                Questions = quizResult.Questions.Select(q => new QuizQuestionDto
                {
                    QuestionId = q.QuestionId,
                    Question = q.Question,
                    Options = q.Options,
                    Type = "multiple-choice"
                }).ToList()
            };

            return response;
        }

        public async Task<SubmitQuizResponse> SubmitQuizAsync(SubmitQuizRequest request)
        {
            // Get quiz from database
            var quiz = await _context.Quizzes.FindAsync(request.QuizId);
            if (quiz == null)
            {
                throw new Exception($"Quiz with ID {request.QuizId} not found");
            }

            // ⭐ CHECK: Has this learner already attempted this quiz?
            var existingAttempt = await _context.QuizAttempts
                .FirstOrDefaultAsync(a => a.QuizId == request.QuizId && a.LearnerId == request.LearnerId);

            if (existingAttempt != null)
            {
                throw new Exception($"You have already attempted this quiz. Each quiz can only be taken once.");
            }

            // Deserialize quiz data to get correct answers
            var quizQuestions = JsonSerializer.Deserialize<List<QuizQuestionData>>(quiz.QuizData);
            if (quizQuestions == null)
            {
                throw new Exception("Invalid quiz data");
            }

            // Calculate score
            int score = 0;
            var results = new List<QuizResultDetail>();

            foreach (var question in quizQuestions)
            {
                var userAnswer = request.Answers
                    .FirstOrDefault(a => a.QuestionId == question.QuestionId);

                var isCorrect = userAnswer?.SelectedAnswer == question.CorrectAnswer;
                if (isCorrect) score++;

                results.Add(new QuizResultDetail
                {
                    QuestionId = question.QuestionId,
                    Question = question.Question,
                    YourAnswer = userAnswer?.SelectedAnswer ?? "No answer",
                    CorrectAnswer = question.CorrectAnswer,
                    IsCorrect = isCorrect,
                    Explanation = question.Explanation
                });
            }

            int totalQuestions = quizQuestions.Count;
            decimal percentage = (decimal)score / totalQuestions * 100;

            // Store quiz attempt
            var attempt = new QuizAttempt
            {
                QuizId = request.QuizId,
                LearnerId = request.LearnerId,
                LearnerAnswers = JsonSerializer.Serialize(request.Answers),
                Score = score,
                TotalQuestions = totalQuestions,
                Percentage = percentage,
                StartedAt = DateTime.UtcNow, // In real app, track when quiz was opened
                CompletedAt = DateTime.UtcNow,
                TimeTakenSeconds = 0 // Frontend should send this
            };

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            // Generate feedback
            string feedback = GenerateFeedback(percentage);

            var response = new SubmitQuizResponse
            {
                AttemptId = attempt.Id,
                Score = score,
                TotalQuestions = totalQuestions,
                Percentage = percentage,
                Feedback = feedback,
                Results = results
            };

            return response;
        }

        private string GenerateFeedback(decimal percentage)
        {
            return percentage switch
            {
                >= 90 => "Excellent! You have a strong grasp of the material.",
                >= 75 => "Great job! You're doing well, but there's room for improvement.",
                >= 60 => "Good effort! Review the topics you missed and try again.",
                >= 50 => "You're getting there! More practice will help you improve.",
                _ => "Keep learning! Review the material and don't give up."
            };
        }

        public async Task<SubmitQuizResponse> GetQuizAttemptResultsAsync(int attemptId)
        {
            // Get quiz attempt from database
            var attempt = await _context.QuizAttempts
                .Include(a => a.Quiz)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null)
            {
                throw new Exception($"Quiz attempt with ID {attemptId} not found");
            }

            // Get quiz data
            var quizQuestions = JsonSerializer.Deserialize<List<QuizQuestionData>>(attempt.Quiz!.QuizData);
            if (quizQuestions == null)
            {
                throw new Exception("Invalid quiz data");
            }

            // Get learner answers
            var learnerAnswers = JsonSerializer.Deserialize<List<QuizAnswerDto>>(attempt.LearnerAnswers);
            if (learnerAnswers == null)
            {
                throw new Exception("Invalid learner answers data");
            }

            // Build results
            var results = new List<QuizResultDetail>();
            foreach (var question in quizQuestions)
            {
                var userAnswer = learnerAnswers.FirstOrDefault(a => a.QuestionId == question.QuestionId);
                var isCorrect = userAnswer?.SelectedAnswer == question.CorrectAnswer;

                results.Add(new QuizResultDetail
                {
                    QuestionId = question.QuestionId,
                    Question = question.Question,
                    YourAnswer = userAnswer?.SelectedAnswer ?? "No answer",
                    CorrectAnswer = question.CorrectAnswer,
                    IsCorrect = isCorrect,
                    Explanation = question.Explanation
                });
            }

            return new SubmitQuizResponse
            {
                AttemptId = attempt.Id,
                Score = attempt.Score,
                TotalQuestions = attempt.TotalQuestions,
                Percentage = attempt.Percentage,
                Feedback = GenerateFeedback(attempt.Percentage),
                Results = results
            };
        }

        public async Task<List<QuizAttemptSummary>> GetLearnerQuizAttemptsAsync(int learnerId)
        {
            var attempts = await _context.QuizAttempts
                .Include(a => a.Quiz)
                .Where(a => a.LearnerId == learnerId)
                .OrderByDescending(a => a.CompletedAt)
                .ToListAsync();

            return attempts.Select(a => new QuizAttemptSummary
            {
                AttemptId = a.Id,
                QuizId = a.QuizId,
                QuizTitle = a.Quiz!.Title,
                QuizType = a.Quiz.QuizType,
                Score = a.Score,
                TotalQuestions = a.TotalQuestions,
                Percentage = a.Percentage,
                CompletedAt = a.CompletedAt ?? DateTime.UtcNow,
                TimeTakenSeconds = a.TimeTakenSeconds
            }).ToList();
        }

        public async Task<LearnerQuizStatistics> GetLearnerQuizStatisticsAsync(int learnerId)
        {
            var attempts = await _context.QuizAttempts
                .Include(a => a.Quiz)
                .Where(a => a.LearnerId == learnerId)
                .ToListAsync();

            if (!attempts.Any())
            {
                return new LearnerQuizStatistics
                {
                    LearnerId = learnerId,
                    TotalQuizzesTaken = 0,
                    AverageScore = 0,
                    BestScore = 0,
                    TotalQuestionsAnswered = 0,
                    TotalCorrectAnswers = 0,
                    RecentAttempts = new List<QuizAttemptSummary>()
                };
            }

            var recentAttempts = attempts
                .OrderByDescending(a => a.CompletedAt)
                .Take(10)
                .Select(a => new QuizAttemptSummary
                {
                    AttemptId = a.Id,
                    QuizId = a.QuizId,
                    QuizTitle = a.Quiz!.Title,
                    QuizType = a.Quiz.QuizType,
                    Score = a.Score,
                    TotalQuestions = a.TotalQuestions,
                    Percentage = a.Percentage,
                    CompletedAt = a.CompletedAt ?? DateTime.UtcNow,
                    TimeTakenSeconds = a.TimeTakenSeconds
                }).ToList();

            return new LearnerQuizStatistics
            {
                LearnerId = learnerId,
                TotalQuizzesTaken = attempts.Count,
                AverageScore = attempts.Average(a => a.Percentage),
                BestScore = attempts.Max(a => a.Score),
                TotalQuestionsAnswered = attempts.Sum(a => a.TotalQuestions),
                TotalCorrectAnswers = attempts.Sum(a => a.Score),
                RecentAttempts = recentAttempts
            };
        }

        public async Task<QuizDetailsResponse> GetQuizDetailsAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.QuizAttempts)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                throw new Exception($"Quiz with ID {quizId} not found");
            }

            var quizQuestions = JsonSerializer.Deserialize<List<QuizQuestionData>>(quiz.QuizData);

            return new QuizDetailsResponse
            {
                QuizId = quiz.Id,
                Title = quiz.Title,
                QuizType = quiz.QuizType,
                Difficulty = quiz.Difficulty,
                GeneratedAt = quiz.GeneratedAt,
                TotalQuestions = quizQuestions?.Count ?? 0,
                TimesAttempted = quiz.QuizAttempts.Count
            };
        }

        public async Task<bool> HasLearnerAttemptedQuizAsync(int quizId, int learnerId)
        {
            return await _context.QuizAttempts
                .AnyAsync(a => a.QuizId == quizId && a.LearnerId == learnerId);
        }
    }
}

