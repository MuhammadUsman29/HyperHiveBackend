using System.Text.Json;
using System.Text.RegularExpressions;
using HyperHiveBackend.Models;
using Microsoft.Extensions.Configuration;

namespace HyperHiveBackend.Services;

public interface IGitHubService
{
    Task<DeveloperStrongAreas> AnalyzeDeveloperStrongAreasAsync(GitHubAnalysisRequest request);
    Task<List<GitHubCommit>> GetUserCommitsAsync(string owner, string repository, string username, string accessToken, DateTime? since = null, DateTime? until = null);
    Task<List<GitHubPullRequest>> GetUserPullRequestsAsync(string owner, string repository, string username, string accessToken);
}

public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GitHubService> _logger;
    private readonly IConfigurationService _configService;

    // Technology patterns
    private readonly Dictionary<string, string[]> _technologyPatterns = new()
    {
        { "ASP.NET Core", new[] { "Startup.cs", "Program.cs", "appsettings.json", "Microsoft.AspNetCore", "UseRouting", "MapControllers" } },
        { "Entity Framework", new[] { "DbContext", "DbSet", "OnModelCreating", "Migration", "EntityFrameworkCore" } },
        { "Dapper", new[] { "Dapper", "QueryAsync", "ExecuteAsync", "IDbRepository" } },
        { "MySQL", new[] { "MySqlConnection", "MySql.Data", "CREATE TABLE", "INSERT INTO" } },
        { "JWT Authentication", new[] { "JwtBearer", "JwtSecurityToken", "TokenValidationParameters", "GenerateToken" } },
        { "REST API", new[] { "ApiController", "HttpGet", "HttpPost", "HttpPut", "HttpDelete", "Route" } },
        { "Swagger", new[] { "SwaggerGen", "SwaggerUI", "OpenApiInfo", "AddSwaggerGen" } },
        { "Dependency Injection", new[] { "AddScoped", "AddTransient", "AddSingleton", "IServiceCollection" } },
        { "BCrypt", new[] { "BCrypt", "HashPassword", "VerifyPassword" } },
        { "Serilog", new[] { "Serilog", "Log.Logger", "WriteTo" } },
        { "AWS SDK", new[] { "AWSSDK", "Amazon", "S3", "DynamoDB", "Lambda" } },
        { "MediatR", new[] { "MediatR", "IRequest", "IRequestHandler", "SendAsync" } },
        { "Docker", new[] { "Dockerfile", "docker-compose", ".dockerignore" } },
        { "Unit Testing", new[] { "xUnit", "NUnit", "Moq", "Test", "Assert" } },
        { "GraphQL", new[] { "GraphQL", "Query", "Mutation", "Schema" } },
        { "gRPC", new[] { "Grpc", "proto", "ServiceDefinition" } },
        { "Redis", new[] { "Redis", "StackExchange.Redis", "IDatabase" } },
        { "RabbitMQ", new[] { "RabbitMQ", "IModel", "QueueDeclare" } },
        { "Kafka", new[] { "Kafka", "Confluent", "Producer", "Consumer" } },
        { "React", new[] { "React", "useState", "useEffect", "Component", ".jsx" } },
        { "Angular", new[] { "Angular", "@Component", "@Injectable", ".ts" } },
        { "Vue.js", new[] { "Vue", "vue", ".vue", "VueComponent" } },
        { "Node.js", new[] { "Node.js", "express", "require(", "module.exports" } }
    };

    // Domain area patterns
    private readonly Dictionary<string, string[]> _domainPatterns = new()
    {
        { "Backend API", new[] { "Controllers", "Services", "Repositories", "ApiController" } },
        { "Database", new[] { "DataAccess", "Repository", "DbContext", "Migration", "SQL" } },
        { "Authentication", new[] { "Auth", "Login", "Signup", "JWT", "Token", "Password" } },
        { "Frontend", new[] { ".jsx", ".tsx", ".vue", "React", "Angular", "Vue", "Component" } },
        { "Infrastructure", new[] { "Infrastructure", "Config", "Startup", "Program.cs" } },
        { "Testing", new[] { "Tests", "Test", "Spec", "Mock", "Fixture" } },
        { "DevOps", new[] { "Dockerfile", "CI/CD", "pipeline", "deploy", "kubernetes", ".github" } },
        { "Documentation", new[] { "README", ".md", "docs", "Documentation" } }
    };

    // Programming language file extensions
    private readonly Dictionary<string, string[]> _languageExtensions = new()
    {
        { "C#", new[] { ".cs", ".csx" } },
        { "JavaScript", new[] { ".js", ".jsx", ".mjs" } },
        { "TypeScript", new[] { ".ts", ".tsx" } },
        { "Python", new[] { ".py", ".pyw" } },
        { "Java", new[] { ".java" } },
        { "SQL", new[] { ".sql" } },
        { "HTML", new[] { ".html", ".htm" } },
        { "CSS", new[] { ".css", ".scss", ".sass" } },
        { "JSON", new[] { ".json" } },
        { "XML", new[] { ".xml", ".config" } },
        { "YAML", new[] { ".yml", ".yaml" } },
        { "Shell", new[] { ".sh", ".bash", ".ps1" } },
        { "Docker", new[] { "Dockerfile", ".dockerignore" } },
        { "Markdown", new[] { ".md", ".markdown" } },
        { "Go", new[] { ".go" } },
        { "Rust", new[] { ".rs" } },
        { "PHP", new[] { ".php" } },
        { "Ruby", new[] { ".rb" } }
    };

    // Code concept patterns
    private readonly Dictionary<string, string[]> _conceptPatterns = new()
    {
        { "Async/Await", new[] { "async", "await", "Task", "Task<", "async Task" } },
        { "LINQ", new[] { ".Where(", ".Select(", ".FirstOrDefault(", ".Any(", ".ToList(" } },
        { "Dependency Injection", new[] { "AddScoped", "AddTransient", "AddSingleton", "IServiceProvider" } },
        { "Repository Pattern", new[] { "IRepository", "Repository", "IDbRepository" } },
        { "Unit of Work", new[] { "IUnitOfWork", "UnitOfWork", "SaveChanges" } },
        { "Factory Pattern", new[] { "Factory", "Create", "IFactory" } },
        { "Strategy Pattern", new[] { "IStrategy", "Strategy", "Execute" } },
        { "Observer Pattern", new[] { "IObserver", "Subscribe", "Notify" } },
        { "Middleware", new[] { "UseMiddleware", "IMiddleware", "InvokeAsync" } },
        { "Attribute Routing", new[] { "[Route(", "[HttpGet(", "[HttpPost(" } },
        { "Model Validation", new[] { "[Required]", "[EmailAddress]", "[StringLength]", "ModelState" } },
        { "Error Handling", new[] { "try", "catch", "throw", "Exception", "ErrorHandler" } },
        { "Logging", new[] { "ILogger", "LogInformation", "LogError", "LogWarning" } },
        { "Configuration", new[] { "IConfiguration", "appsettings", "GetSection", "GetValue" } },
        { "Caching", new[] { "IMemoryCache", "Cache", "GetOrCreate", "Set" } }
    };

    public GitHubService(HttpClient httpClient, IConfiguration configuration, ILogger<GitHubService> logger, IConfigurationService configService)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _configService = configService;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "HyperHiveBackend/1.0");
    }

    public async Task<DeveloperStrongAreas> AnalyzeDeveloperStrongAreasAsync(GitHubAnalysisRequest request)
    {
        try
        {
            var accessToken = _configuration["GitHub:AccessToken"] 
                ?? throw new InvalidOperationException("GitHub AccessToken not configured");

            _logger.LogInformation($"Analyzing developer strong areas for user: {request.Username}");

            // Fetch commits and pull requests
            var commits = await GetUserCommitsAsync(
                request.Owner, 
                request.Repository, 
                request.Username, 
                accessToken,
                request.Since,
                request.Until
            );

            var pullRequests = await GetUserPullRequestsAsync(
                request.Owner,
                request.Repository,
                request.Username,
                accessToken
            );

            // Analyze the data
            var analysis = new DeveloperStrongAreas
            {
                DeveloperUsername = request.Username,
                DeveloperName = commits.FirstOrDefault()?.Commit?.Author?.Name ?? request.Username,
                TotalCommits = commits.Count,
                TotalPullRequests = pullRequests.Count,
                TotalLinesAdded = commits.Where(c => c.Stats != null).Sum(c => c.Stats!.Additions),
                TotalLinesDeleted = commits.Where(c => c.Stats != null).Sum(c => c.Stats!.Deletions)
            };

            // Analyze languages
            analysis.Languages = AnalyzeLanguages(commits);

            // Analyze technologies
            analysis.Technologies = AnalyzeTechnologies(commits, pullRequests);

            // Analyze domain areas
            analysis.DomainAreas = AnalyzeDomainAreas(commits, pullRequests);

            // Analyze concepts
            analysis.Concepts = AnalyzeConcepts(commits);

            _logger.LogInformation($"Analysis completed for {request.Username}. Found {analysis.Languages.Count} languages, {analysis.Technologies.Count} technologies.");

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error analyzing developer strong areas for {request.Username}");
            throw;
        }
    }

    public async Task<List<GitHubCommit>> GetUserCommitsAsync(
        string owner, 
        string repository, 
        string username, 
        string accessToken,
        DateTime? since = null,
        DateTime? until = null)
    {
        try
        {
            var commits = new List<GitHubCommit>();
            var page = 1;
            const int perPage = 100;

            while (true)
            {
                var url = $"https://api.github.com/repos/{owner}/{repository}/commits";
                var queryParams = new List<string>
                {
                    $"per_page={perPage}",
                    $"page={page}",
                    $"author={username}"
                };

                if (since.HasValue)
                {
                    queryParams.Add($"since={since.Value:yyyy-MM-ddTHH:mm:ssZ}");
                }

                if (until.HasValue)
                {
                    queryParams.Add($"until={until.Value:yyyy-MM-ddTHH:mm:ssZ}");
                }

                url += "?" + string.Join("&", queryParams);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                }

                var response = await _httpClient.SendAsync(request);
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"Repository {owner}/{repository} not found");
                    break;
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var pageCommits = JsonSerializer.Deserialize<List<GitHubCommit>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<GitHubCommit>();

                if (!pageCommits.Any())
                    break;

                commits.AddRange(pageCommits);

                // Check if there are more pages
                var hasMorePages = false;
                if (response.Headers.TryGetValues("Link", out var linkHeaders))
                {
                    var linkHeader = linkHeaders.FirstOrDefault();
                    hasMorePages = !string.IsNullOrEmpty(linkHeader) && linkHeader.Contains("rel=\"next\"");
                }
                
                if (!hasMorePages)
                    break;

                page++;
            }

            // Fetch detailed stats for each commit
            foreach (var commit in commits.Take(50)) // Limit to avoid too many API calls
            {
                try
                {
                    var statsUrl = $"https://api.github.com/repos/{owner}/{repository}/commits/{commit.Sha}";
                    var statsRequest = new HttpRequestMessage(HttpMethod.Get, statsUrl);
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        statsRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    }

                    var statsResponse = await _httpClient.SendAsync(statsRequest);
                    if (statsResponse.IsSuccessStatusCode)
                    {
                        var statsContent = await statsResponse.Content.ReadAsStringAsync();
                        var commitWithStats = JsonSerializer.Deserialize<GitHubCommit>(statsContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (commitWithStats?.Stats != null)
                        {
                            commit.Stats = commitWithStats.Stats;
                        }
                        if (commitWithStats?.Files != null)
                        {
                            commit.Files = commitWithStats.Files;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to fetch stats for commit {commit.Sha}");
                }
            }

            return commits;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching commits for user {username}");
            throw;
        }
    }

    public async Task<List<GitHubPullRequest>> GetUserPullRequestsAsync(
        string owner, 
        string repository, 
        string username, 
        string accessToken)
    {
        try
        {
            var pullRequests = new List<GitHubPullRequest>();
            var page = 1;
            const int perPage = 100;

            while (true)
            {
                var url = $"https://api.github.com/repos/{owner}/{repository}/pulls?state=all&per_page={perPage}&page={page}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                }

                var response = await _httpClient.SendAsync(request);
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"Repository {owner}/{repository} not found");
                    break;
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var pagePullRequests = JsonSerializer.Deserialize<List<GitHubPullRequest>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<GitHubPullRequest>();

                // Filter by author
                var userPullRequests = pagePullRequests.Where(pr => 
                    pr.User?.Login?.Equals(username, StringComparison.OrdinalIgnoreCase) == true).ToList();

                if (!pagePullRequests.Any())
                    break;

                pullRequests.AddRange(userPullRequests);

                // Check if there are more pages
                var hasMorePages = false;
                if (response.Headers.TryGetValues("Link", out var linkHeaders))
                {
                    var linkHeader = linkHeaders.FirstOrDefault();
                    hasMorePages = !string.IsNullOrEmpty(linkHeader) && linkHeader.Contains("rel=\"next\"");
                }
                
                if (!hasMorePages)
                    break;

                page++;
            }

            return pullRequests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching pull requests for user {username}");
            throw;
        }
    }

    private List<LanguageUsage> AnalyzeLanguages(List<GitHubCommit> commits)
    {
        var languageCounts = new Dictionary<string, int>();
        var languageLines = new Dictionary<string, int>();

        foreach (var commit in commits)
        {
            if (commit.Files != null)
            {
                foreach (var file in commit.Files)
                {
                    var language = DetectLanguage(file.Filename);
                    if (!string.IsNullOrEmpty(language))
                    {
                        languageCounts[language] = languageCounts.GetValueOrDefault(language, 0) + 1;
                        languageLines[language] = languageLines.GetValueOrDefault(language, 0) + file.Additions;
                    }
                }
            }
            else
            {
                // Fallback: extract from commit message
                var filePaths = ExtractFilePaths(commit.Commit?.Message ?? "");
                foreach (var filePath in filePaths)
                {
                    var language = DetectLanguage(filePath);
                    if (!string.IsNullOrEmpty(language))
                    {
                        languageCounts[language] = languageCounts.GetValueOrDefault(language, 0) + 1;
                        if (commit.Stats != null)
                        {
                            languageLines[language] = languageLines.GetValueOrDefault(language, 0) + commit.Stats.Additions;
                        }
                    }
                }
            }
        }

        var totalFiles = languageCounts.Values.Sum();
        var totalLines = languageLines.Values.Sum();

        return languageCounts.Select(kvp => new LanguageUsage
        {
            Language = kvp.Key,
            FileCount = kvp.Value,
            LinesOfCode = languageLines.GetValueOrDefault(kvp.Key, 0),
            Percentage = totalFiles > 0 ? (double)kvp.Value / totalFiles * 100 : 0
        })
        .OrderByDescending(l => l.FileCount)
        .ToList();
    }

    private List<TechnologyUsage> AnalyzeTechnologies(List<GitHubCommit> commits, List<GitHubPullRequest> pullRequests)
    {
        var technologyCounts = new Dictionary<string, int>();
        var technologyFiles = new Dictionary<string, HashSet<string>>();

        var allText = string.Join(" ", commits.Select(c => (c.Commit?.Message ?? "") + " " + (c.Commit?.Message ?? "")));
        allText += " " + string.Join(" ", pullRequests.Select(pr => pr.Title + " " + (pr.Body ?? "")));

        // Also analyze file names from commits
        foreach (var commit in commits)
        {
            if (commit.Files != null)
            {
                foreach (var file in commit.Files)
                {
                    allText += " " + file.Filename;
                }
            }
        }

        foreach (var tech in _technologyPatterns)
        {
            var count = 0;
            var files = new HashSet<string>();

            foreach (var pattern in tech.Value)
            {
                var matches = Regex.Matches(allText, Regex.Escape(pattern), RegexOptions.IgnoreCase);
                count += matches.Count;

                foreach (var commit in commits)
                {
                    if (commit.Files != null)
                    {
                        foreach (var file in commit.Files)
                        {
                            if (file.Filename.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                            {
                                files.Add(file.Filename);
                            }
                        }
                    }
                }
            }

            if (count > 0)
            {
                technologyCounts[tech.Key] = count;
                technologyFiles[tech.Key] = files;
            }
        }

        var total = technologyCounts.Values.Sum();

        return technologyCounts.Select(kvp => new TechnologyUsage
        {
            Technology = kvp.Key,
            UsageCount = kvp.Value,
            Percentage = total > 0 ? (double)kvp.Value / total * 100 : 0,
            Files = technologyFiles.GetValueOrDefault(kvp.Key, new HashSet<string>()).Take(10).ToList()
        })
        .OrderByDescending(t => t.UsageCount)
        .ToList();
    }

    private List<DomainArea> AnalyzeDomainAreas(List<GitHubCommit> commits, List<GitHubPullRequest> pullRequests)
    {
        var areaCounts = new Dictionary<string, int>();
        var areaExamples = new Dictionary<string, HashSet<string>>();

        var allText = string.Join(" ", commits.Select(c => (c.Commit?.Message ?? "") + " " + (c.Commit?.Message ?? "")));
        allText += " " + string.Join(" ", pullRequests.Select(pr => pr.Title + " " + (pr.Body ?? "")));

        // Include file paths
        foreach (var commit in commits)
        {
            if (commit.Files != null)
            {
                allText += " " + string.Join(" ", commit.Files.Select(f => f.Filename));
            }
        }

        foreach (var area in _domainPatterns)
        {
            var count = 0;
            var examples = new HashSet<string>();

            foreach (var pattern in area.Value)
            {
                var matches = Regex.Matches(allText, Regex.Escape(pattern), RegexOptions.IgnoreCase);
                count += matches.Count;

                foreach (var commit in commits)
                {
                    var commitMessage = commit.Commit?.Message ?? "";
                    if (commitMessage.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        examples.Add(commitMessage.Split('\n').FirstOrDefault() ?? "");
                    }
                }

                foreach (var pr in pullRequests)
                {
                    if (pr.Title.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        examples.Add(pr.Title);
                    }
                }
            }

            if (count > 0)
            {
                areaCounts[area.Key] = count;
                areaExamples[area.Key] = examples;
            }
        }

        var total = areaCounts.Values.Sum();

        return areaCounts.Select(kvp => new DomainArea
        {
            Area = kvp.Key,
            ContributionCount = kvp.Value,
            Percentage = total > 0 ? (double)kvp.Value / total * 100 : 0,
            Examples = areaExamples.GetValueOrDefault(kvp.Key, new HashSet<string>()).Take(5).ToList()
        })
        .OrderByDescending(a => a.ContributionCount)
        .ToList();
    }

    private List<ConceptUsage> AnalyzeConcepts(List<GitHubCommit> commits)
    {
        var conceptCounts = new Dictionary<string, int>();
        var conceptExamples = new Dictionary<string, HashSet<string>>();

        var allText = string.Join(" ", commits.Select(c => c.Commit?.Message ?? ""));

        foreach (var concept in _conceptPatterns)
        {
            var count = 0;
            var examples = new HashSet<string>();

            foreach (var pattern in concept.Value)
            {
                var matches = Regex.Matches(allText, Regex.Escape(pattern), RegexOptions.IgnoreCase);
                count += matches.Count;

                foreach (var commit in commits)
                {
                    var commitMessage = commit.Commit?.Message ?? "";
                    if (commitMessage.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        examples.Add(commitMessage.Split('\n').FirstOrDefault() ?? "");
                    }
                }
            }

            if (count > 0)
            {
                conceptCounts[concept.Key] = count;
                conceptExamples[concept.Key] = examples;
            }
        }

        var total = conceptCounts.Values.Sum();

        return conceptCounts.Select(kvp => new ConceptUsage
        {
            Concept = kvp.Key,
            OccurrenceCount = kvp.Value,
            Percentage = total > 0 ? (double)kvp.Value / total * 100 : 0,
            Examples = conceptExamples.GetValueOrDefault(kvp.Key, new HashSet<string>()).Take(5).ToList()
        })
        .OrderByDescending(c => c.OccurrenceCount)
        .ToList();
    }

    private string DetectLanguage(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var fileName = Path.GetFileName(filePath);

        foreach (var lang in _languageExtensions)
        {
            if (lang.Value.Any(ext => extension == ext || fileName == ext))
            {
                return lang.Key;
            }
        }

        return "Unknown";
    }

    private List<string> ExtractFilePaths(string text)
    {
        var filePaths = new List<string>();
        
        var patterns = new[]
        {
            @"[\w/\\]+\.\w+",
            @"[A-Za-z]:\\[^\s]+",
            @"/[^\s]+",
        };

        foreach (var pattern in patterns)
        {
            var matches = Regex.Matches(text, pattern);
            foreach (Match match in matches)
            {
                if (match.Success && !filePaths.Contains(match.Value))
                {
                    filePaths.Add(match.Value);
                }
            }
        }

        return filePaths;
    }
}

