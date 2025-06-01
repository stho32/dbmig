using dbmig.BL.Database.Models;
using dbmig.BL.Database.Repositories;

namespace dbmig.BL.Tests.Mocks;

public class MockMigrationRepository : IMigrationRepository
{
    public bool ShouldCreateMigrationTableSucceed { get; set; } = true;
    public bool ShouldMigrationTableExist { get; set; } = false;
    public bool ShouldAddMigrationSucceed { get; set; } = true;
    public bool ShouldUpdateMigrationSucceed { get; set; } = true;
    public bool ShouldClearAllUserTablesSucceed { get; set; } = true;
    public bool ShouldThrowException { get; set; } = false;
    public string ExceptionMessage { get; set; } = "Mock exception";

    public List<string> CreateMigrationTableCalls { get; } = new();
    public List<string> MigrationTableExistsCalls { get; } = new();
    public List<string> AddMigrationCalls { get; } = new();
    public List<string> UpdateMigrationCalls { get; } = new();
    public int ClearAllUserTablesCallCount { get; private set; }

    public Task<bool> CreateMigrationTableAsync(string tableName = "_Migrations")
    {
        if (ShouldThrowException)
            throw new Exception(ExceptionMessage);

        CreateMigrationTableCalls.Add(tableName);
        return Task.FromResult(ShouldCreateMigrationTableSucceed);
    }

    public Task<bool> MigrationTableExistsAsync(string tableName = "_Migrations")
    {
        if (ShouldThrowException)
            throw new Exception(ExceptionMessage);

        MigrationTableExistsCalls.Add(tableName);
        return Task.FromResult(ShouldMigrationTableExist);
    }

    public Task<IEnumerable<Migration>> GetAllMigrationsAsync(string tableName = "_Migrations")
    {
        if (ShouldThrowException)
            throw new Exception(ExceptionMessage);

        return Task.FromResult(Enumerable.Empty<Migration>());
    }

    public Task<Migration?> GetMigrationByNameAsync(string migrationName, string tableName = "_Migrations")
    {
        if (ShouldThrowException)
            throw new Exception(ExceptionMessage);

        return Task.FromResult<Migration?>(null);
    }

    public Task<bool> AddMigrationAsync(NewMigration migration, string tableName = "_Migrations")
    {
        if (ShouldThrowException)
            throw new Exception(ExceptionMessage);

        AddMigrationCalls.Add($"{migration.MigrationName}:{tableName}");
        return Task.FromResult(ShouldAddMigrationSucceed);
    }

    public Task<bool> UpdateMigrationAsync(string migrationName, DateTime? appliedAt, string? errorMessage, string tableName = "_Migrations")
    {
        if (ShouldThrowException)
            throw new Exception(ExceptionMessage);

        UpdateMigrationCalls.Add($"{migrationName}:{tableName}");
        return Task.FromResult(ShouldUpdateMigrationSucceed);
    }

    public Task<bool> ClearAllUserTablesAsync()
    {
        if (ShouldThrowException)
            throw new Exception(ExceptionMessage);

        ClearAllUserTablesCallCount++;
        return Task.FromResult(ShouldClearAllUserTablesSucceed);
    }

    public void Reset()
    {
        ShouldCreateMigrationTableSucceed = true;
        ShouldMigrationTableExist = false;
        ShouldAddMigrationSucceed = true;
        ShouldUpdateMigrationSucceed = true;
        ShouldClearAllUserTablesSucceed = true;
        ShouldThrowException = false;
        ExceptionMessage = "Mock exception";

        CreateMigrationTableCalls.Clear();
        MigrationTableExistsCalls.Clear();
        AddMigrationCalls.Clear();
        UpdateMigrationCalls.Clear();
        ClearAllUserTablesCallCount = 0;
    }
}