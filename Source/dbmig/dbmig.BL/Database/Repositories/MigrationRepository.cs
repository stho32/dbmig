using dbmig.BL.Common;
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

    private static void ValidateTableName(string tableName)
    {
        if (!ValidationHelper.IsValidTableName(tableName))
            throw new ArgumentException($"Invalid table name: '{tableName}'. Only alphanumeric characters and underscores are allowed.", nameof(tableName));
    }

    public async Task<bool> CreateMigrationTableAsync(string tableName = "_Migrations")
    {
        ValidateTableName(tableName);
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
        ValidateTableName(tableName);
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
        ValidateTableName(tableName);
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
        ValidateTableName(tableName);
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
        ValidateTableName(tableName);
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
        ValidateTableName(tableName);
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
            _logger.LogInfo("Starting to clear all user objects from database");

            // Drop all foreign key constraints first
            var getForeignKeysSQL = @"
                SELECT 
                    fk.name AS FK_Name,
                    OBJECT_SCHEMA_NAME(fk.parent_object_id) AS Schema_Name,
                    OBJECT_NAME(fk.parent_object_id) AS Table_Name
                FROM sys.foreign_keys fk
                WHERE OBJECT_SCHEMA_NAME(fk.parent_object_id) NOT IN ('sys', 'INFORMATION_SCHEMA')";

            var foreignKeys = await _databaseAccessor.QueryAsync<dynamic>(getForeignKeysSQL);
            foreach (var fk in foreignKeys)
            {
                var dropFKSQL = $"ALTER TABLE [{fk.Schema_Name}].[{fk.Table_Name}] DROP CONSTRAINT [{fk.FK_Name}]";
                await _databaseAccessor.ExecuteAsync(dropFKSQL);
                _logger.LogDebug($"Dropped foreign key: {fk.FK_Name}");
            }

            // Drop all user-defined functions
            var getFunctionsSQL = @"
                SELECT ROUTINE_SCHEMA, ROUTINE_NAME 
                FROM INFORMATION_SCHEMA.ROUTINES 
                WHERE ROUTINE_TYPE = 'FUNCTION' 
                AND ROUTINE_SCHEMA NOT IN ('sys', 'INFORMATION_SCHEMA')";

            var functions = await _databaseAccessor.QueryAsync<dynamic>(getFunctionsSQL);
            foreach (var func in functions)
            {
                var dropFuncSQL = $"DROP FUNCTION IF EXISTS [{func.ROUTINE_SCHEMA}].[{func.ROUTINE_NAME}]";
                await _databaseAccessor.ExecuteAsync(dropFuncSQL);
                _logger.LogDebug($"Dropped function: {func.ROUTINE_SCHEMA}.{func.ROUTINE_NAME}");
            }

            // Drop all stored procedures
            var getProcsSQL = @"
                SELECT ROUTINE_SCHEMA, ROUTINE_NAME 
                FROM INFORMATION_SCHEMA.ROUTINES 
                WHERE ROUTINE_TYPE = 'PROCEDURE' 
                AND ROUTINE_SCHEMA NOT IN ('sys', 'INFORMATION_SCHEMA')";

            var procedures = await _databaseAccessor.QueryAsync<dynamic>(getProcsSQL);
            foreach (var proc in procedures)
            {
                var dropProcSQL = $"DROP PROCEDURE IF EXISTS [{proc.ROUTINE_SCHEMA}].[{proc.ROUTINE_NAME}]";
                await _databaseAccessor.ExecuteAsync(dropProcSQL);
                _logger.LogDebug($"Dropped procedure: {proc.ROUTINE_SCHEMA}.{proc.ROUTINE_NAME}");
            }

            // Drop all views
            var getViewsSQL = @"
                SELECT TABLE_SCHEMA, TABLE_NAME 
                FROM INFORMATION_SCHEMA.VIEWS 
                WHERE TABLE_SCHEMA NOT IN ('sys', 'INFORMATION_SCHEMA')";

            var views = await _databaseAccessor.QueryAsync<dynamic>(getViewsSQL);
            foreach (var view in views)
            {
                var dropViewSQL = $"DROP VIEW IF EXISTS [{view.TABLE_SCHEMA}].[{view.TABLE_NAME}]";
                await _databaseAccessor.ExecuteAsync(dropViewSQL);
                _logger.LogDebug($"Dropped view: {view.TABLE_SCHEMA}.{view.TABLE_NAME}");
            }

            // Drop all user tables
            var getTablesSQL = @"
                SELECT TABLE_SCHEMA, TABLE_NAME 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_TYPE = 'BASE TABLE' 
                AND TABLE_SCHEMA NOT IN ('sys', 'INFORMATION_SCHEMA')";

            var tables = await _databaseAccessor.QueryAsync<dynamic>(getTablesSQL);
            foreach (var table in tables)
            {
                var dropTableSQL = $"DROP TABLE IF EXISTS [{table.TABLE_SCHEMA}].[{table.TABLE_NAME}]";
                await _databaseAccessor.ExecuteAsync(dropTableSQL);
                _logger.LogDebug($"Dropped table: {table.TABLE_SCHEMA}.{table.TABLE_NAME}");
            }

            _logger.LogInfo("All user objects cleared successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error clearing all user objects", ex);
            return false;
        }
    }

    public async Task<bool> ExecuteMigrationSqlAsync(string sql)
    {
        try
        {
            return await _databaseAccessor.ExecuteInTransactionAsync(async db =>
            {
                // Split on GO batch separator
                var batches = System.Text.RegularExpressions.Regex.Split(sql, @"^\s*GO\s*$",
                    System.Text.RegularExpressions.RegexOptions.Multiline |
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                foreach (var batch in batches)
                {
                    if (!string.IsNullOrWhiteSpace(batch))
                    {
                        await db.ExecuteAsync(batch.Trim());
                    }
                }
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error executing migration SQL", ex);
            throw;
        }
    }
}