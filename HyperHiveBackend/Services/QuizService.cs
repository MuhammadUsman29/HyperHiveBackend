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
    }
}

