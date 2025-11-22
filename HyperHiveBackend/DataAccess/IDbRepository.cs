using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HyperHiveBackend.DataAccess;

public interface IDbRepository
{
    int Execute(
        string query,
        object? param = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNo = 0
    );
    
    Task<int> ExecuteAsync(
        string query,
        object? param = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNo = 0
    );
    
    T? QuerySingleOrDefault<T>(
        string query,
        object? param = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNo = 0
    );
    
    IEnumerable<T> Query<T>(
        string query,
        object? param = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNo = 0
    );
    
    Task<T?> QuerySingleOrDefaultAsync<T>(
        string query,
        object? param = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNo = 0
    );
    
    Task<IEnumerable<T>> QueryAsync<T>(
        string query,
        object? param = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNo = 0
    );
}

