using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;

namespace HyperHiveBackend.DataAccess;

public class DbRepository : IDbRepository
{
    private readonly string _connectionString;

    public DbRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private DbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }

    public int Execute(
        string query,
        object? param = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNo = 0
    )
    {
        using var connection = CreateConnection();
        connection.Open();
        return connection.Execute(query, param);
    }

    public async Task<int> ExecuteAsync(
        string query,
        object? param = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNo = 0
    )
    {
        using var connection = CreateConnection();
        connection.Open();
        return await connection.ExecuteAsync(query, param);
    }

    public T? QuerySingleOrDefault<T>(
        string query,
        object? param = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNo = 0
    )
    {
        using var connection = CreateConnection();
        connection.Open();
        return connection.QuerySingleOrDefault<T>(query, param);
    }

    public IEnumerable<T> Query<T>(
        string query,
        object? param = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNo = 0
    )
    {
        using var connection = CreateConnection();
        connection.Open();
        return connection.Query<T>(query, param);
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string query,
        object? param = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNo = 0
    )
    {
        using var connection = CreateConnection();
        connection.Open();
        return await connection.QuerySingleOrDefaultAsync<T>(query, param);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(
        string query,
        object? param = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNo = 0
    )
    {
        using var connection = CreateConnection();
        connection.Open();
        return await connection.QueryAsync<T>(query, param);
    }
}

