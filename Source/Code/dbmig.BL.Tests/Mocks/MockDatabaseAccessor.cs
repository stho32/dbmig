using dbmig.BL.Database;

namespace dbmig.BL.Tests.Mocks;

public class MockDatabaseAccessor : IDatabaseAccessor
{
    public bool ShouldThrowException { get; set; } = false;
    public string ExceptionMessage { get; set; } = "Mock exception";
    public bool ShouldTableExist { get; set; } = false;
    public bool ShouldDatabaseExist { get; set; } = false;
    public int ExecuteAsyncReturnValue { get; set; } = 1;
    public bool ExecuteInTransactionAsyncReturnValue { get; set; } = true;

    public List<string> QuerySingleAsyncCalls { get; } = new();
    public List<string> QueryAsyncCalls { get; } = new();
    public List<string> ExecuteAsyncCalls { get; } = new();
    public List<string> TableExistsAsyncCalls { get; } = new();
    public int DatabaseExistsAsyncCallCount { get; private set; }
    public int ExecuteInTransactionAsyncCallCount { get; private set; }

    public Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null)
    {
        if (ShouldThrowException)
            throw new Exception(ExceptionMessage);

        QuerySingleAsyncCalls.Add(sql);

        // Return appropriate values based on type and ExecuteAsyncReturnValue
        if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
        {
            if (ExecuteAsyncReturnValue == 0)
                return Task.FromResult(default(T));
            return Task.FromResult((T?)(object)ExecuteAsyncReturnValue);
        }

        return Task.FromResult(default(T));
    }

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
    {
        if (ShouldThrowException)
            throw new Exception(ExceptionMessage);

        QueryAsyncCalls.Add(sql);

        if (typeof(T) == typeof(string))
        {
            var tables = new[] { "Table1", "Table2", "Table3" };
            return Task.FromResult((IEnumerable<T>)(object)tables);
        }
        
        // For dynamic queries (like the INFORMATION_SCHEMA query), return table objects
        if (typeof(T) == typeof(object) && sql.Contains("INFORMATION_SCHEMA.TABLES"))
        {
            dynamic table1 = new System.Dynamic.ExpandoObject();
            table1.TABLE_SCHEMA = "dbo";
            table1.TABLE_NAME = "Table1";
            
            dynamic table2 = new System.Dynamic.ExpandoObject();
            table2.TABLE_SCHEMA = "dbo";
            table2.TABLE_NAME = "Table2";
            
            dynamic table3 = new System.Dynamic.ExpandoObject();
            table3.TABLE_SCHEMA = "dbo";
            table3.TABLE_NAME = "Table3";
            
            var tables = new[] { table1, table2, table3 };
            return Task.FromResult((IEnumerable<T>)(object)tables);
        }

        return Task.FromResult(Enumerable.Empty<T>());
    }

    public Task<int> ExecuteAsync(string sql, object? parameters = null)
    {
        if (ShouldThrowException)
            throw new Exception(ExceptionMessage);

        ExecuteAsyncCalls.Add(sql);
        return Task.FromResult(ExecuteAsyncReturnValue);
    }

    public Task<bool> ExecuteInTransactionAsync(Func<IDatabaseAccessor, Task<bool>> operation)
    {
        if (ShouldThrowException)
            throw new Exception(ExceptionMessage);

        ExecuteInTransactionAsyncCallCount++;
        return Task.FromResult(ExecuteInTransactionAsyncReturnValue);
    }

    public Task<bool> TableExistsAsync(string tableName, string? schemaName = null)
    {
        if (ShouldThrowException)
            throw new Exception(ExceptionMessage);

        TableExistsAsyncCalls.Add($"{tableName}:{schemaName ?? "null"}");
        return Task.FromResult(ShouldTableExist);
    }

    public Task<bool> DatabaseExistsAsync()
    {
        if (ShouldThrowException)
            throw new Exception(ExceptionMessage);

        DatabaseExistsAsyncCallCount++;
        return Task.FromResult(ShouldDatabaseExist);
    }

    public void Dispose()
    {
        // Mock disposal - nothing to do
    }

    public void Reset()
    {
        ShouldThrowException = false;
        ExceptionMessage = "Mock exception";
        ShouldTableExist = false;
        ShouldDatabaseExist = false;
        ExecuteAsyncReturnValue = 1;
        ExecuteInTransactionAsyncReturnValue = true;

        QuerySingleAsyncCalls.Clear();
        QueryAsyncCalls.Clear();
        ExecuteAsyncCalls.Clear();
        TableExistsAsyncCalls.Clear();
        DatabaseExistsAsyncCallCount = 0;
        ExecuteInTransactionAsyncCallCount = 0;
    }
}