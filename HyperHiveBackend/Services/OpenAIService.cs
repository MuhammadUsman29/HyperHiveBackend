using OpenAI.Chat;
using System.Text.Json;
using System.ClientModel;

namespace HyperHiveBackend.Services
{
    public interface IOpenAIService
    {
        Task<QuizGenerationResult> GenerateQuizAsync(string learnerProfileData, string quizType, int numberOfQuestions);
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
                    new SystemChatMessage("You are an expert educational content creator specializing in creating personalized quizzes for software engineers. Always respond with valid JSON only."),
                    new UserChatMessage(prompt)
                };

                var response = await _chatClient.CompleteChatAsync(messages);
                var content = response.Value.Content[0].Text;

                _logger.LogInformation("OpenAI Response: {Response}", content);

                // Parse the JSON response
                var quizResult = JsonSerializer.Deserialize<QuizGenerationResult>(content);
                
                if (quizResult == null || quizResult.Questions == null || quizResult.Questions.Count == 0)
                {
                    throw new Exception("Failed to parse quiz from OpenAI response");
                }

                return quizResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quiz with OpenAI");
                throw;
            }
        }

        private string BuildQuizPrompt(string learnerProfileData, string quizType, int numberOfQuestions)
        {
            return $@"
Based on this learner profile:
{learnerProfileData}

Generate a {quizType} quiz with {numberOfQuestions} multiple-choice questions to validate their skills and knowledge.

Requirements:
- Questions should be relevant to their current skill level
- Each question should have 4 options (A, B, C, D)
- Include the correct answer
- Provide a brief explanation for the correct answer
- Make questions practical and scenario-based when possible

Return ONLY valid JSON in this EXACT format (no markdown, no extra text):
{{
  ""title"": ""Quiz Title Here"",
  ""questions"": [
    {{
      ""questionId"": 1,
      ""question"": ""Question text here?"",
      ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
      ""correctAnswer"": ""Option B"",
      ""explanation"": ""Brief explanation why this is correct""
    }}
  ]
}}
";
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
}

