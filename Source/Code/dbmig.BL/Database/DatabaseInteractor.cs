using dbmig.BL.Common;
using dbmig.BL.Logging;
using dbmig.BL.Database.Repositories;
using dbmig.BL.Configuration;

namespace dbmig.BL.Database;

public class DatabaseInteractor
{
    private readonly ILogger _logger;
    private readonly IMigrationRepository _migrationRepository;

    public DatabaseInteractor(IMigrationRepository? migrationRepository = null)
    {
        _logger = LoggerFactory.Get();
        _migrationRepository = migrationRepository ?? CreateDefaultRepository();
    }

    private static IMigrationRepository CreateDefaultRepository()
    {
        // Default: Create repository with default database accessor
        // Connection string comes from central configuration
        var databaseAccessor = new SqlServerDatabaseAccessor(ConnectionStrings.Default);
        return new MigrationRepository(databaseAccessor);
    }

    public Result ClearDatabase(string connectionString)
    {
        try
        {
            _logger.LogInfo($"Clearing database with connection: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
            
            var result = _migrationRepository.ClearAllUserTablesAsync().GetAwaiter().GetResult();
            
            if (result)
            {
                _logger.LogInfo("Database cleared successfully");
                return new Result(true, "Database cleared successfully.");
            }
            else
            {
                _logger.LogWarning("Database clearing completed with warnings");
                return new Result(false, "Database clearing failed. Check logs for details.");
            }
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
            
            // Check if table already exists
            var tableExists = _migrationRepository.MigrationTableExistsAsync(tableName).GetAwaiter().GetResult();
            if (tableExists)
            {
                _logger.LogWarning($"Migration table '{tableName}' already exists");
                return new Result(true, $"Migration system initialized with table '{tableName}'.");
            }
            
            var result = _migrationRepository.CreateMigrationTableAsync(tableName).GetAwaiter().GetResult();
            
            if (result)
            {
                _logger.LogInfo($"Migration system initialized successfully with table '{tableName}'");
                return new Result(true, $"Migration system initialized with table '{tableName}'.");
            }
            else
            {
                _logger.LogWarning($"Migration table creation failed for '{tableName}'");
                return new Result(false, $"Failed to create migration table '{tableName}'. Check logs for details.");
            }
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

            // Check if migration table exists
            var tableExists = _migrationRepository.MigrationTableExistsAsync(tableName).GetAwaiter().GetResult();
            if (!tableExists)
            {
                _logger.LogWarning($"Migration table '{tableName}' does not exist");
                return new Result(false, $"Migration table '{tableName}' does not exist. Run 'init' command first.");
            }
            
            // Get migration files
            var migrationFiles = Directory.GetFiles(directory, "*.sql")
                .Where(f => System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(f), @"^\d{5}-.*\.sql$"))
                .OrderBy(f => Path.GetFileName(f))
                .ToArray();
                
            if (migrationFiles.Length == 0)
            {
                _logger.LogInfo("No migration files found in directory");
                return new Result(true, "No migration files found to execute.");
            }
            
            _logger.LogInfo($"Found {migrationFiles.Length} migration files");
            
            // For now, just register the migrations (discovery phase)
            // TODO: Implement actual execution phase
            foreach (var file in migrationFiles)
            {
                var fileName = Path.GetFileName(file);
                _logger.LogInfo($"Discovered migration: {fileName}");
            }
            
            _logger.LogInfo($"Migration discovery completed for '{directory}' using table '{tableName}'");
            return new Result(true, $"Migrations from '{directory}' processed successfully using table '{tableName}'.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to run migrations", ex);
            return new Result(false, "Failed to run migrations. Please check your directory path and database connection.");
        }
    }
}