using System.Text.Json.Serialization;

namespace HyperHiveBackend.Models;

public class GitHubUser
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }
}

public class GitHubCommit
{
    [JsonPropertyName("sha")]
    public string Sha { get; set; } = string.Empty;

    [JsonPropertyName("commit")]
    public GitHubCommitDetails? Commit { get; set; }

    [JsonPropertyName("author")]
    public GitHubUser? Author { get; set; }

    [JsonPropertyName("committer")]
    public GitHubUser? Committer { get; set; }

    [JsonPropertyName("stats")]
    public GitHubCommitStats? Stats { get; set; }

    [JsonPropertyName("files")]
    public List<GitHubCommitFile>? Files { get; set; }
}

public class GitHubCommitDetails
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public GitHubCommitAuthor? Author { get; set; }

    [JsonPropertyName("committer")]
    public GitHubCommitAuthor? Committer { get; set; }
}

public class GitHubCommitAuthor
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
}

public class GitHubCommitStats
{
    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class GitHubCommitFile
{
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }

    [JsonPropertyName("changes")]
    public int Changes { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public class GitHubPullRequest
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("user")]
    public GitHubUser? User { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("head")]
    public GitHubBranch? Head { get; set; }

    [JsonPropertyName("base")]
    public GitHubBranch? Base { get; set; }
}

public class GitHubBranch
{
    [JsonPropertyName("ref")]
    public string Ref { get; set; } = string.Empty;

    [JsonPropertyName("sha")]
    public string Sha { get; set; } = string.Empty;
}

public class DeveloperStrongAreas
{
    public string DeveloperUsername { get; set; } = string.Empty;
    public string DeveloperName { get; set; } = string.Empty;
    public int TotalCommits { get; set; }
    public int TotalPullRequests { get; set; }
    public int TotalLinesAdded { get; set; }
    public int TotalLinesDeleted { get; set; }
    public List<LanguageUsage> Languages { get; set; } = new();
    public List<TechnologyUsage> Technologies { get; set; } = new();
    public List<DomainArea> DomainAreas { get; set; } = new();
    public List<ConceptUsage> Concepts { get; set; } = new();
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
}

public class LanguageUsage
{
    public string Language { get; set; } = string.Empty;
    public int FileCount { get; set; }
    public int LinesOfCode { get; set; }
    public double Percentage { get; set; }
}

public class TechnologyUsage
{
    public string Technology { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public double Percentage { get; set; }
    public List<string> Files { get; set; } = new();
}

public class DomainArea
{
    public string Area { get; set; } = string.Empty;
    public int ContributionCount { get; set; }
    public double Percentage { get; set; }
    public List<string> Examples { get; set; } = new();
}

public class ConceptUsage
{
    public string Concept { get; set; } = string.Empty;
    public int OccurrenceCount { get; set; }
    public double Percentage { get; set; }
    public List<string> Examples { get; set; } = new();
}

public class GitHubAnalysisRequest
{
    public string Owner { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime? Since { get; set; }
    public DateTime? Until { get; set; }
}

