using dbmig.BL.Database.Models;
using dbmig.BL.Logging;

namespace dbmig.BL.Database.Repositories;

public class MigrationRepository : IMigrationRepository
{
    private readonly IDatabaseAccessor _databaseAccessor;
    private readonly ILogger _logger;

    public MigrationRepository(IDatabaseAccessor databaseAccessor)
    {
        _databaseAccessor = databaseAccessor ?? throw new ArgumentNullException(nameof(databaseAccessor));
        _logger = LoggerFactory.Get();
    }

    public async Task<bool> CreateMigrationTableAsync(string tableName = "_Migrations")
    {
        try
        {
            _logger.LogInfo($"Creating migration table: {tableName}");

            var sql = $@"
                CREATE TABLE [{tableName}] (
                    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
                    [MigrationName] NVARCHAR(255) NOT NULL UNIQUE,
                    [DiscoveredAt] DATETIME NOT NULL DEFAULT GETDATE(),
                    [ErrorMessage] NVARCHAR(MAX) NULL,
                    [AppliedAt] DATETIME NULL,
                    [Hash] NVARCHAR(64) NULL
                )";

            await _databaseAccessor.ExecuteAsync(sql);
            
            // Add baseline migration
            var baselineMigration = new NewMigration(
                "00000-Baseline",
                DateTime.Now,
                null,
                DateTime.Now,
                null
            );
            
            await AddMigrationAsync(baselineMigration, tableName);
            
            _logger.LogInfo($"Migration table '{tableName}' created successfully with baseline migration");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating migration table: {tableName}", ex);
            return false;
        }
    }

    public async Task<bool> MigrationTableExistsAsync(string tableName = "_Migrations")
    {
        try
        {
            return await _databaseAccessor.TableExistsAsync(tableName);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking if migration table exists: {tableName}", ex);
            return false;
        }
    }

    public async Task<IEnumerable<Migration>> GetAllMigrationsAsync(string tableName = "_Migrations")
    {
        try
        {
            var sql = $@"
                SELECT [Id], [MigrationName], [DiscoveredAt], [ErrorMessage], [AppliedAt], [Hash]
                FROM [{tableName}]
                ORDER BY [MigrationName]";

            var results = await _databaseAccessor.QueryAsync<dynamic>(sql);
            
            // Convert dynamic results to Migration objects
            var migrations = new List<Migration>();
            // Note: This is a simplified version. In a real implementation,
            // you would use a proper ORM or mapping mechanism
            
            return migrations;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting all migrations from table: {tableName}", ex);
            return Enumerable.Empty<Migration>();
        }
    }

    public async Task<Migration?> GetMigrationByNameAsync(string migrationName, string tableName = "_Migrations")
    {
        try
        {
            var sql = $@"
                SELECT [Id], [MigrationName], [DiscoveredAt], [ErrorMessage], [AppliedAt], [Hash]
                FROM [{tableName}]
                WHERE [MigrationName] = @migrationName";

            var parameters = new { migrationName };
            
            // Simplified implementation - in practice you'd use proper mapping
            var exists = await _databaseAccessor.QuerySingleAsync<int?>(
                $"SELECT 1 FROM [{tableName}] WHERE [MigrationName] = @migrationName", 
                parameters);
            
            return exists.HasValue ? new Migration(0, migrationName, DateTime.Now, null, null, null) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting migration by name: {migrationName}", ex);
            return null;
        }
    }

    public async Task<bool> AddMigrationAsync(NewMigration migration, string tableName = "_Migrations")
    {
        try
        {
            var sql = $@"
                INSERT INTO [{tableName}] ([MigrationName], [DiscoveredAt], [ErrorMessage], [AppliedAt], [Hash])
                VALUES (@migrationName, @discoveredAt, @errorMessage, @appliedAt, @hash)";

            var parameters = new
            {
                migrationName = migration.MigrationName,
                discoveredAt = migration.DiscoveredAt,
                errorMessage = migration.ErrorMessage,
                appliedAt = migration.AppliedAt,
                hash = migration.Hash
            };

            var rowsAffected = await _databaseAccessor.ExecuteAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding migration: {migration.MigrationName}", ex);
            return false;
        }
    }

    public async Task<bool> UpdateMigrationAsync(string migrationName, DateTime? appliedAt, string? errorMessage, string tableName = "_Migrations")
    {
        try
        {
            var sql = $@"
                UPDATE [{tableName}]
                SET [AppliedAt] = @appliedAt, [ErrorMessage] = @errorMessage
                WHERE [MigrationName] = @migrationName";

            var parameters = new
            {
                migrationName,
                appliedAt,
                errorMessage
            };

            var rowsAffected = await _databaseAccessor.ExecuteAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating migration: {migrationName}", ex);
            return false;
        }
    }

    public async Task<bool> ClearAllUserTablesAsync()
    {
        try
        {
            _logger.LogInfo("Starting to clear all user tables from database");

            // Get all user tables (excluding system tables)
            var getTablesSQL = @"
                SELECT TABLE_SCHEMA, TABLE_NAME 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_TYPE = 'BASE TABLE' 
                AND TABLE_SCHEMA NOT IN ('sys', 'INFORMATION_SCHEMA')";

            var tables = await _databaseAccessor.QueryAsync<string>(getTablesSQL);
            
            // Note: This is simplified. In reality, you'd need to handle:
            // - Foreign key constraints
            // - Views, procedures, functions
            // - Proper schema handling
            
            foreach (var table in tables)
            {
                var dropSQL = $"DROP TABLE IF EXISTS [{table}]";
                await _databaseAccessor.ExecuteAsync(dropSQL);
                _logger.LogDebug($"Dropped table: {table}");
            }

            _logger.LogInfo("All user tables cleared successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error clearing all user tables", ex);
            return false;
        }
    }
}