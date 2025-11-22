namespace HyperHiveBackend.Services
{
    public interface IConfigurationService
    {
        string GetOpenAIApiKey();
        string GetOpenAIBaseUrl();
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly Dictionary<string, string> _keys;
        private readonly ILogger<ConfigurationService> _logger;

        public ConfigurationService(ILogger<ConfigurationService> logger)
        {
            _logger = logger;
            _keys = new Dictionary<string, string>();
            LoadKeys();
        }

        private void LoadKeys()
        {
            try
            {
                var keysFilePath = Path.Combine(Directory.GetCurrentDirectory(), "keys.txt");
                
                if (!File.Exists(keysFilePath))
                {
                    _logger.LogWarning("keys.txt file not found at {Path}", keysFilePath);
                    return;
                }

                var lines = File.ReadAllLines(keysFilePath);
                
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();
                        _keys[key] = value;
                    }
                }

                _logger.LogInformation("Loaded {Count} keys from keys.txt", _keys.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading keys from keys.txt");
            }
        }

        public string GetOpenAIApiKey()
        {
            if (_keys.TryGetValue("OPENAI_API_KEY", out var apiKey) && !string.IsNullOrWhiteSpace(apiKey))
            {
                return apiKey;
            }

            throw new InvalidOperationException("OPENAI_API_KEY not found in keys.txt");
        }

        public string GetOpenAIBaseUrl()
        {
            if (_keys.TryGetValue("OPENAI_BASE_URL", out var baseUrl) && !string.IsNullOrWhiteSpace(baseUrl))
            {
                return baseUrl;
            }

            return "https://api.openai.com/v1"; // Default OpenAI URL
        }
    }
}

