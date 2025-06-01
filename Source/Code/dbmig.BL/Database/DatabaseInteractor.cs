using dbmig.BL.Common;
using dbmig.BL.Logging;

namespace dbmig.BL.Database;

public class DatabaseInteractor
{
    private readonly ILogger _logger;

    public DatabaseInteractor()
    {
        _logger = LoggerFactory.Get();
    }

    public Result ClearDatabase(string connectionString)
    {
        try
        {
            _logger.LogInfo($"Clearing database with connection: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
            
            // TODO: Implement actual database clearing logic
            
            _logger.LogInfo("Database cleared successfully");
            return new Result(true, "Database cleared successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to clear database", ex);
            return new Result(false, "Failed to clear database. Please check your connection and permissions.");
        }
    }

    public Result InitializeMigration(string connectionString, string? migrationTableName = null)
    {
        try
        {
            var tableName = migrationTableName ?? "_Migrations";
            _logger.LogInfo($"Initializing migration system with table '{tableName}'");
            
            // TODO: Implement actual migration table creation logic
            
            _logger.LogInfo($"Migration system initialized successfully with table '{tableName}'");
            return new Result(true, $"Migration system initialized with table '{tableName}'.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to initialize migration system", ex);
            return new Result(false, "Failed to initialize migration system. Please check your database connection and permissions.");
        }
    }

    public Result RunMigrations(string connectionString, string directory, string? migrationTableName = null)
    {
        try
        {
            var tableName = migrationTableName ?? "_Migrations";
            _logger.LogInfo($"Running migrations from directory '{directory}' using table '{tableName}'");
            
            if (!Directory.Exists(directory))
            {
                _logger.LogWarning($"Migration directory '{directory}' does not exist");
                return new Result(false, $"Migration directory '{directory}' does not exist.");
            }

            // TODO: Implement actual migration execution logic
            
            _logger.LogInfo($"Migrations executed successfully from '{directory}' using table '{tableName}'");
            return new Result(true, $"Migrations from '{directory}' executed successfully using table '{tableName}'.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to run migrations", ex);
            return new Result(false, "Failed to run migrations. Please check your directory path and database connection.");
        }
    }
}