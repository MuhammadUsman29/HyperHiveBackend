using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HyperHiveBackend.DataAccess;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace HyperHiveBackend.Services;

public interface IMigrationService
{
    Task RunMigrationsAsync();
}

public class MigrationService : IMigrationService
{
    private readonly string _connectionString;
    private readonly ILogger<MigrationService> _logger;

    public MigrationService(IConfiguration configuration, ILogger<MigrationService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _logger = logger;
    }

    public async Task RunMigrationsAsync()
    {
        try
        {
            // Ensure database exists
            await EnsureDatabaseExistsAsync();

            // Get migration files
            var migrationPath = Path.Combine(Directory.GetCurrentDirectory(), "Migrations");
            if (!Directory.Exists(migrationPath))
            {
                _logger.LogWarning("Migrations directory not found. Skipping migrations.");
                return;
            }

            var migrationFiles = Directory.GetFiles(migrationPath, "*.sql")
                .OrderBy(f => f)
                .ToList();

            foreach (var migrationFile in migrationFiles)
            {
                _logger.LogInformation($"Running migration: {Path.GetFileName(migrationFile)}");
                await ExecuteMigrationFileAsync(migrationFile);
            }

            _logger.LogInformation("All migrations completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running migrations");
            throw;
        }
    }

    private async Task EnsureDatabaseExistsAsync()
    {
        // Extract database name from connection string
        var builder = new MySqlConnectionStringBuilder(_connectionString);
        var databaseName = builder.Database;
        builder.Database = ""; // Connect to MySQL server without specifying database

        using var connection = new MySqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        var createDbQuery = $"CREATE DATABASE IF NOT EXISTS `{databaseName}`;";
        using var command = new MySqlCommand(createDbQuery, connection);
        await command.ExecuteNonQueryAsync();

        _logger.LogInformation($"Database '{databaseName}' ensured to exist.");
    }

    private async Task ExecuteMigrationFileAsync(string filePath)
    {
        var sql = await File.ReadAllTextAsync(filePath);
        
        // Split by semicolons but preserve them for MySQL
        var statements = sql.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s) && !s.StartsWith("--"))
            .ToList();

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        foreach (var statement in statements)
        {
            if (string.IsNullOrWhiteSpace(statement)) continue;
            
            using var command = new MySqlCommand(statement, connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}

