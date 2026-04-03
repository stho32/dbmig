using System.Data;

namespace dbmig.BL.Database;

public interface IDatabaseAccessor : IDisposable
{
    Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null);
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null);
    Task<int> ExecuteAsync(string sql, object? parameters = null);
    Task<bool> ExecuteInTransactionAsync(Func<IDatabaseAccessor, Task<bool>> operation);
    Task<bool> TableExistsAsync(string tableName, string? schemaName = null);
    Task<bool> DatabaseExistsAsync();
}
