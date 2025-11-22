using OpenAI.Chat;
using System.Text.Json;
using System.ClientModel;
using HyperHiveBackend.DTOs;

namespace HyperHiveBackend.Services
{
    public interface IOpenAIService
    {
        Task<QuizGenerationResult> GenerateQuizAsync(string learnerProfileData, string quizType, int numberOfQuestions);
        Task<AIValidationResult> GenerateValidationAnalysisAsync(string prompt);
        Task<AIGrowthPlanResult> GenerateGrowthPlanAsync(string prompt);
        Task<string> GetChatResponseAsync(string userMessage);
    }

    public class OpenAIService : IOpenAIService
    {
        private readonly ChatClient _chatClient;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(IConfigurationService configService, ILogger<OpenAIService> logger)
        {
            _logger = logger;

            var apiKey = configService.GetOpenAIApiKey();
            var baseUrl = configService.GetOpenAIBaseUrl();

            _logger.LogInformation("Initializing OpenAI service with base URL: {BaseUrl}", baseUrl);

            var options = new OpenAI.OpenAIClientOptions
            {
                Endpoint = new Uri(baseUrl)
            };

            var client = new OpenAI.OpenAIClient(new ApiKeyCredential(apiKey), options);
            _chatClient = client.GetChatClient("gpt-4o-mini");
        }

        public async Task<QuizGenerationResult> GenerateQuizAsync(
            string learnerProfileData, 
            string quizType, 
            int numberOfQuestions)
        {
            try
            {
                var prompt = BuildQuizPrompt(learnerProfileData, quizType, numberOfQuestions);

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("You are an expert educational content creator specializing in creating personalized quizzes for software engineers. Always respond with valid JSON only, no markdown, no extra text."),
                    new UserChatMessage(prompt)
                };

                var response = await _chatClient.CompleteChatAsync(messages);
                var content = response.Value.Content[0].Text;

                _logger.LogInformation("OpenAI Raw Response: {Response}", content);

                // Clean the response - remove markdown code blocks if present
                content = CleanJsonResponse(content);

                _logger.LogInformation("Cleaned Response: {Response}", content);

                // Parse the JSON response with more lenient options
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                var quizResult = JsonSerializer.Deserialize<QuizGenerationResult>(content, options);
                
                if (quizResult == null || quizResult.Questions == null || quizResult.Questions.Count == 0)
                {
                    _logger.LogError("Failed to parse quiz. Response content: {Content}", content);
                    throw new Exception($"Failed to parse quiz from OpenAI response. Content: {content.Substring(0, Math.Min(200, content.Length))}");
                }

                return quizResult;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON parsing error in OpenAI response");
                throw new Exception($"Failed to parse OpenAI response as JSON: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quiz with OpenAI");
                throw;
            }
        }

        private string CleanJsonResponse(string content)
        {
            // Remove markdown code blocks if present
            content = content.Trim();
            
            // Remove ```json and ``` markers
            if (content.StartsWith("```json"))
            {
                content = content.Substring(7);
            }
            else if (content.StartsWith("```"))
            {
                content = content.Substring(3);
            }
            
            if (content.EndsWith("```"))
            {
                content = content.Substring(0, content.Length - 3);
            }
            
            return content.Trim();
        }

        private string BuildQuizPrompt(string learnerProfileData, string quizType, int numberOfQuestions)
        {
            return $@"Based on this learner profile:
{learnerProfileData}

Generate a {quizType} quiz with {numberOfQuestions} multiple-choice questions to validate their skills and knowledge.

Requirements:
- Questions should be relevant to their current skill level and interests
- Each question should have exactly 4 options
- Include the correct answer (must match exactly one of the options)
- Provide a brief explanation for the correct answer
- Make questions practical and scenario-based when possible

CRITICAL: Return ONLY valid JSON in this EXACT format. Do NOT include markdown, code blocks, or any extra text:

{{
  ""title"": ""C# Skills Assessment"",
  ""questions"": [
    {{
      ""questionId"": 1,
      ""question"": ""What is dependency injection?"",
      ""options"": [""A design pattern for managing dependencies"", ""A type of SQL injection"", ""A testing framework"", ""A compiler optimization""],
      ""correctAnswer"": ""A design pattern for managing dependencies"",
      ""explanation"": ""Dependency injection is a design pattern that allows classes to receive their dependencies from external sources rather than creating them internally.""
    }},
    {{
      ""questionId"": 2,
      ""question"": ""Which keyword is used to define an asynchronous method in C#?"",
      ""options"": [""async"", ""await"", ""promise"", ""defer""],
      ""correctAnswer"": ""async"",
      ""explanation"": ""The async keyword is used to mark a method as asynchronous in C#, allowing the use of await within the method.""
    }}
  ]
}}

Generate {numberOfQuestions} questions following this exact structure.";
        }

        public async Task<AIValidationResult> GenerateValidationAnalysisAsync(string prompt)
        {
            try
            {
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("You are an expert at analyzing and validating software engineer profiles. Always respond with valid JSON only."),
                    new UserChatMessage(prompt)
                };

                var response = await _chatClient.CompleteChatAsync(messages);
                var content = response.Value.Content[0].Text;

                _logger.LogInformation("OpenAI Validation Response: {Response}", content);

                // Clean the response
                content = CleanJsonResponse(content);

                // Parse JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                var result = JsonSerializer.Deserialize<AIValidationResult>(content, options);

                if (result == null)
                {
                throw new Exception("Failed to parse AI validation response");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating validation analysis with OpenAI");
            throw;
        }
    }

    public async Task<AIGrowthPlanResult> GenerateGrowthPlanAsync(string prompt)
    {
        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an expert career development advisor and learning path designer for software engineers. Always respond with valid JSON only."),
                new UserChatMessage(prompt)
            };

            var response = await _chatClient.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            _logger.LogInformation("OpenAI Growth Plan Response: {Response}", content);

            // Clean the response
            content = CleanJsonResponse(content);

            // Parse JSON
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            var result = JsonSerializer.Deserialize<AIGrowthPlanResult>(content, options);

            if (result == null)
            {
                throw new Exception("Failed to parse AI growth plan response");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating growth plan with OpenAI");
            throw;
        }
    }

    public async Task<string> GetChatResponseAsync(string userMessage)
    {
        try
        {
            _logger.LogInformation("Processing chat request");

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an AI Career Coach and technical expert for software engineers. Provide concise, helpful answers to technical questions. Keep responses short and focused (2-3 paragraphs max)."),
                new UserChatMessage(userMessage)
            };

            var response = await _chatClient.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            _logger.LogInformation("Chat response generated successfully");
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chat response");
            throw new ApplicationException("Failed to generate chat response", ex);
        }
    }
}

    // Models for OpenAI response
    public class QuizGenerationResult
    {
        public string Title { get; set; } = string.Empty;
        public List<QuizQuestionData> Questions { get; set; } = new();
    }

    public class QuizQuestionData
    {
        public int QuestionId { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }

    // Helper class for AI growth plan result
    public class AIGrowthPlanResult
    {
        public string Overview { get; set; } = string.Empty;
        public int EstimatedDurationMonths { get; set; }
        public List<LearningPhase> LearningPhases { get; set; } = new();
        public List<RecommendedResource> RecommendedResources { get; set; } = new();
        public List<string> KeyMilestones { get; set; } = new();
        public string SuccessCriteria { get; set; } = string.Empty;
    }
}

