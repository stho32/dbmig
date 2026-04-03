using dbmig.BL.Common;
using dbmig.BL.Logging;
using dbmig.BL.Database.Repositories;

namespace dbmig.BL.Database;

public class DatabaseInteractor
{
    private readonly ILogger _logger;
    private readonly IMigrationRepository? _injectedRepository;

    /// <summary>
    /// Production constructor — creates a real repository per call using the provided connection string.
    /// </summary>
    public DatabaseInteractor()
    {
        _logger = LoggerFactory.Get();
        _injectedRepository = null;
    }

    /// <summary>
    /// Test constructor — uses the injected repository for all operations.
    /// </summary>
    public DatabaseInteractor(IMigrationRepository? migrationRepository)
    {
        _logger = LoggerFactory.Get();
        _injectedRepository = migrationRepository;
    }

    private IMigrationRepository GetRepository(string connectionString)
    {
        if (_injectedRepository != null)
            return _injectedRepository;

        var databaseAccessor = new SqlServerDatabaseAccessor(connectionString);
        return new MigrationRepository(databaseAccessor);
    }

    public Result ClearDatabase(string connectionString)
    {
        try
        {
            _logger.LogInfo($"Clearing database with connection: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");

            var repository = GetRepository(connectionString);
            var result = repository.ClearAllUserTablesAsync().GetAwaiter().GetResult();

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

            var repository = GetRepository(connectionString);

            // Check if table already exists
            var tableExists = repository.MigrationTableExistsAsync(tableName).GetAwaiter().GetResult();
            if (tableExists)
            {
                _logger.LogWarning($"Migration table '{tableName}' already exists");
                return new Result(true, $"Migration system initialized with table '{tableName}'.");
            }

            var result = repository.CreateMigrationTableAsync(tableName).GetAwaiter().GetResult();

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

            var repository = GetRepository(connectionString);

            // Check if migration table exists
            var tableExists = repository.MigrationTableExistsAsync(tableName).GetAwaiter().GetResult();
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

            // Phase 1: Discovery — register all migration files
            var newMigrations = new List<string>();
            foreach (var file in migrationFiles)
            {
                var fileName = Path.GetFileName(file);
                var existingMigration = repository.GetMigrationByNameAsync(fileName, tableName).GetAwaiter().GetResult();

                if (existingMigration == null)
                {
                    var newMigration = new Models.NewMigration(fileName, DateTime.Now);
                    repository.AddMigrationAsync(newMigration, tableName).GetAwaiter().GetResult();
                    newMigrations.Add(file);
                    _logger.LogInfo($"Registered new migration: {fileName}");
                }
                else if (existingMigration.AppliedAt != null && existingMigration.ErrorMessage == null)
                {
                    _logger.LogDebug($"Migration already applied: {fileName}");
                }
                else
                {
                    // Previously failed or not yet applied
                    newMigrations.Add(file);
                    _logger.LogInfo($"Migration pending: {fileName}");
                }
            }

            if (newMigrations.Count == 0)
            {
                _logger.LogInfo("All migrations already applied");
                return new Result(true, "All migrations are already applied. Nothing to do.");
            }

            // Phase 2: Execution — run each new migration
            var executedCount = 0;
            foreach (var file in newMigrations)
            {
                var fileName = Path.GetFileName(file);
                _logger.LogInfo($"Executing migration: {fileName}");

                try
                {
                    var sqlContent = File.ReadAllText(file);
                    repository.ExecuteMigrationSqlAsync(sqlContent).GetAwaiter().GetResult();

                    repository.UpdateMigrationAsync(fileName, DateTime.Now, null, tableName).GetAwaiter().GetResult();
                    executedCount++;
                    _logger.LogInfo($"Migration applied successfully: {fileName}");
                }
                catch (Exception ex)
                {
                    repository.UpdateMigrationAsync(fileName, null, ex.Message, tableName).GetAwaiter().GetResult();
                    _logger.LogError($"Migration failed: {fileName}", ex);
                    return new Result(false, $"Migration '{fileName}' failed: {ex.Message}. Execution stopped.");
                }
            }

            _logger.LogInfo($"Migration completed: {executedCount} migrations applied successfully");
            return new Result(true, $"Migrations from '{directory}' completed successfully. {executedCount} migration(s) applied using table '{tableName}'.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to run migrations", ex);
            return new Result(false, "Failed to run migrations. Please check your directory path and database connection.");
        }
    }

}
