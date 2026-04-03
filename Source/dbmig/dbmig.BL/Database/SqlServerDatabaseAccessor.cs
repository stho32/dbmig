using Microsoft.Data.SqlClient;
using System.Data;
using dbmig.BL.Logging;

namespace dbmig.BL.Database;

public class SqlServerDatabaseAccessor : IDatabaseAccessor
{
    private readonly string _connectionString;
    private readonly ILogger _logger;
    private SqlConnection? _connection;
    private SqlTransaction? _transaction;
    private bool _disposed = false;

    public SqlServerDatabaseAccessor(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = LoggerFactory.Get();
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null)
    {
        try
        {
            _logger.LogDebug($"Executing single query: {sql}");

            var connection = await GetConnectionAsync();
            using var command = new SqlCommand(sql, connection, _transaction);

            AddParameters(command, parameters);

            var result = await command.ExecuteScalarAsync();
            return result != null && result != DBNull.Value ? (T)result : default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error executing single query: {sql}", ex);
            throw;
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
    {
        try
        {
            _logger.LogDebug($"Executing query: {sql}");

            var connection = await GetConnectionAsync();
            using var command = new SqlCommand(sql, connection, _transaction);

            AddParameters(command, parameters);

            var results = new List<T>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (typeof(T) == typeof(string))
                {
                    results.Add((T)(object)reader.GetString(0));
                }
                else if (typeof(T).IsPrimitive || typeof(T) == typeof(DateTime))
                {
                    results.Add((T)reader.GetValue(0));
                }
                else if (typeof(T) == typeof(object) || typeof(T).Name == "Object" ||
                         typeof(T).FullName?.Contains("Dynamic") == true ||
                         typeof(T).FullName?.Contains("<>") == true)
                {
                    // Handle dynamic/anonymous types
                    dynamic obj = new System.Dynamic.ExpandoObject();
                    var dict = (IDictionary<string, object>)obj;

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        dict[reader.GetName(i)] = reader.GetValue(i);
                    }

                    results.Add((T)(object)obj);
                }
                else
                {
                    // For complex types, you might want to implement a mapping mechanism
                    throw new NotSupportedException($"Type {typeof(T)} is not supported for simple queries");
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error executing query: {sql}", ex);
            throw;
        }
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null)
    {
        try
        {
            _logger.LogDebug($"Executing command: {sql}");

            var connection = await GetConnectionAsync();
            using var command = new SqlCommand(sql, connection, _transaction);

            AddParameters(command, parameters);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            _logger.LogDebug($"Command executed, rows affected: {rowsAffected}");

            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error executing command: {sql}", ex);
            throw;
        }
    }

    public async Task<bool> ExecuteInTransactionAsync(Func<IDatabaseAccessor, Task<bool>> operation)
    {
        try
        {
            _logger.LogDebug("Starting database transaction");

            var connection = await GetConnectionAsync();

            if (_transaction != null)
            {
                // Already in transaction, just execute the operation
                return await operation(this);
            }

            _transaction = connection.BeginTransaction();

            try
            {
                var result = await operation(this);

                if (result)
                {
                    await _transaction.CommitAsync();
                    _logger.LogDebug("Transaction committed successfully");
                }
                else
                {
                    await _transaction.RollbackAsync();
                    _logger.LogDebug("Transaction rolled back due to operation failure");
                }

                return result;
            }
            catch
            {
                await _transaction.RollbackAsync();
                _logger.LogDebug("Transaction rolled back due to exception");
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error executing transaction", ex);
            throw;
        }
    }

    public async Task<bool> TableExistsAsync(string tableName, string? schemaName = null)
    {
        try
        {
            var schema = schemaName ?? "dbo";
            var sql = @"
                SELECT COUNT(1) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @tableName";

            var parameters = new { schema, tableName };
            var count = await QuerySingleAsync<int>(sql, parameters);

            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking if table exists: {tableName}", ex);
            throw;
        }
    }

    public async Task<bool> DatabaseExistsAsync()
    {
        try
        {
            var connection = await GetConnectionAsync();
            return connection.State == ConnectionState.Open;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error checking database existence", ex);
            return false;
        }
    }

    private async Task<SqlConnection> GetConnectionAsync()
    {
        if (_connection == null)
        {
            _connection = new SqlConnection(_connectionString);
            await _connection.OpenAsync();
            _logger.LogDebug("Database connection opened");
        }

        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync();
            _logger.LogDebug("Database connection reopened");
        }

        return _connection;
    }

    private static void AddParameters(SqlCommand command, object? parameters)
    {
        if (parameters == null) return;

        var properties = parameters.GetType().GetProperties();
        foreach (var property in properties)
        {
            var value = property.GetValue(parameters) ?? DBNull.Value;
            command.Parameters.AddWithValue($"@{property.Name}", value);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _connection?.Dispose();
            _disposed = true;
            _logger.LogDebug("Database accessor disposed");
        }
    }
}
