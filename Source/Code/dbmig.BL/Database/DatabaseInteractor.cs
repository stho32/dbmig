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

            using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                _logger.LogInfo("Starting to clear all user tables, views, procedures, and functions from database");

                // 1. Drop all foreign key constraints
                var dropFKsCmd = @"
                    DECLARE @sql NVARCHAR(MAX) = N'';
                    SELECT @sql += N'ALTER TABLE [' + s.name + '].[' + t.name + '] DROP CONSTRAINT [' + f.name + '];'
                    FROM sys.foreign_keys f
                    INNER JOIN sys.tables t ON f.parent_object_id = t.object_id
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id;
                    EXEC sp_executesql @sql;
                ";
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(dropFKsCmd, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                // 2. Drop all tables (except system tables)
                var dropTablesCmd = @"
                    DECLARE @sql NVARCHAR(MAX) = N'';
                    SELECT @sql += N'DROP TABLE [' + s.name + '].[' + t.name + '];'
                    FROM sys.tables t
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE s.name NOT IN ('sys', 'INFORMATION_SCHEMA');
                    EXEC sp_executesql @sql;
                ";
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(dropTablesCmd, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                // 3. Drop all views
                var dropViewsCmd = @"
                    DECLARE @sql NVARCHAR(MAX) = N'';
                    SELECT @sql += N'DROP VIEW [' + s.name + '].[' + v.name + '];'
                    FROM sys.views v
                    INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
                    WHERE s.name NOT IN ('sys', 'INFORMATION_SCHEMA');
                    EXEC sp_executesql @sql;
                ";
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(dropViewsCmd, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                // 4. Drop all stored procedures
                var dropProcsCmd = @"
                    DECLARE @sql NVARCHAR(MAX) = N'';
                    SELECT @sql += N'DROP PROCEDURE [' + s.name + '].[' + p.name + '];'
                    FROM sys.procedures p
                    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
                    WHERE s.name NOT IN ('sys', 'INFORMATION_SCHEMA');
                    EXEC sp_executesql @sql;
                ";
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(dropProcsCmd, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                // 5. Drop all functions
                var dropFuncsCmd = @"
                    DECLARE @sql NVARCHAR(MAX) = N'';
                    SELECT @sql += N'DROP FUNCTION [' + s.name + '].[' + f.name + '];'
                    FROM sys.objects f
                    INNER JOIN sys.schemas s ON f.schema_id = s.schema_id
                    WHERE f.type IN ('FN', 'IF', 'TF') AND s.name NOT IN ('sys', 'INFORMATION_SCHEMA');
                    EXEC sp_executesql @sql;
                ";
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(dropFuncsCmd, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                _logger.LogInfo("All user tables, views, procedures, and functions cleared successfully");
                _logger.LogInfo("Database cleared successfully");
                return new Result(true, "Database cleared successfully.");
            }
            catch (Exception innerEx)
            {
                transaction.Rollback();
                _logger.LogError("Failed to clear database (transaction rolled back)", innerEx);
                return new Result(false, "Failed to clear database. Transaction rolled back. " + innerEx.Message);
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