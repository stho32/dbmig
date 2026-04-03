using dbmig.BL.Database.Models;

namespace dbmig.BL.Database.Repositories;

public interface IMigrationRepository
{
    Task<bool> CreateMigrationTableAsync(string tableName = "_Migrations");
    Task<bool> MigrationTableExistsAsync(string tableName = "_Migrations");
    Task<IEnumerable<Migration>> GetAllMigrationsAsync(string tableName = "_Migrations");
    Task<Migration?> GetMigrationByNameAsync(string migrationName, string tableName = "_Migrations");
    Task<bool> AddMigrationAsync(NewMigration migration, string tableName = "_Migrations");
    Task<bool> UpdateMigrationAsync(string migrationName, DateTime? appliedAt, string? errorMessage, string tableName = "_Migrations");
    Task<bool> ClearAllUserTablesAsync();
    Task<bool> ExecuteMigrationSqlAsync(string sql);
}