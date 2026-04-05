using NUnit.Framework;
using dbmig.BL.Database.Repositories;
using dbmig.BL.Database.Models;
using dbmig.BL.Tests.Mocks;

namespace dbmig.BL.Tests.Database.Repositories;

[TestFixture]
public class MigrationRepositoryTests
{
    private MockDatabaseAccessor _mockDatabaseAccessor = null!;
    private MigrationRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDatabaseAccessor = new MockDatabaseAccessor();
        _repository = new MigrationRepository(_mockDatabaseAccessor);
    }

    [TearDown]
    public void TearDown()
    {
        _mockDatabaseAccessor.Reset();
    }

    [Test]
    public void Constructor_WithNullDatabaseAccessor_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MigrationRepository(null!));
    }

    [Test]
    public async Task CreateMigrationTableAsync_WithDefaultTableName_CallsExecuteAsync()
    {
        _mockDatabaseAccessor.ExecuteAsyncReturnValue = 1;

        var result = await _repository.CreateMigrationTableAsync();

        Assert.That(result, Is.True);
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls, Has.Count.EqualTo(2));
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls[0], Contains.Substring("CREATE TABLE [_Migrations]"));
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls[1], Contains.Substring("INSERT INTO [_Migrations]"));
    }

    [Test]
    public async Task CreateMigrationTableAsync_WithCustomTableName_CallsExecuteAsync()
    {
        _mockDatabaseAccessor.ExecuteAsyncReturnValue = 1;

        var result = await _repository.CreateMigrationTableAsync("CustomMigrations");

        Assert.That(result, Is.True);
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls, Has.Count.EqualTo(2));
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls[0], Contains.Substring("CREATE TABLE [CustomMigrations]"));
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls[1], Contains.Substring("INSERT INTO [CustomMigrations]"));
    }

    [Test]
    public async Task CreateMigrationTableAsync_WhenExecuteFails_ReturnsFalse()
    {
        _mockDatabaseAccessor.ShouldThrowException = true;
        _mockDatabaseAccessor.ExceptionMessage = "Database error";

        var result = await _repository.CreateMigrationTableAsync();

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task MigrationTableExistsAsync_CallsTableExistsAsync()
    {
        _mockDatabaseAccessor.ShouldTableExist = true;

        var result = await _repository.MigrationTableExistsAsync();

        Assert.That(result, Is.True);
        Assert.That(_mockDatabaseAccessor.TableExistsAsyncCalls, Contains.Item("_Migrations:null"));
    }

    [Test]
    public async Task MigrationTableExistsAsync_WithCustomTableName_CallsTableExistsAsync()
    {
        _mockDatabaseAccessor.ShouldTableExist = false;

        var result = await _repository.MigrationTableExistsAsync("CustomMigrations");

        Assert.That(result, Is.False);
        Assert.That(_mockDatabaseAccessor.TableExistsAsyncCalls, Contains.Item("CustomMigrations:null"));
    }

    [Test]
    public async Task MigrationTableExistsAsync_WhenExceptionThrown_ReturnsFalse()
    {
        _mockDatabaseAccessor.ShouldThrowException = true;
        _mockDatabaseAccessor.ExceptionMessage = "Database error";

        var result = await _repository.MigrationTableExistsAsync();

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetMigrationByNameAsync_CallsQuerySingleAsync()
    {
        _mockDatabaseAccessor.ExecuteAsyncReturnValue = 1;

        var result = await _repository.GetMigrationByNameAsync("00001-TestMigration");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.MigrationName, Is.EqualTo("00001-TestMigration"));
        Assert.That(_mockDatabaseAccessor.QuerySingleAsyncCalls, Has.Count.EqualTo(1));
        Assert.That(_mockDatabaseAccessor.QuerySingleAsyncCalls[0], Contains.Substring("SELECT 1 FROM [_Migrations]"));
    }

    [Test]
    public async Task GetMigrationByNameAsync_WhenMigrationNotFound_ReturnsNull()
    {
        _mockDatabaseAccessor.ExecuteAsyncReturnValue = 0; // This will make QuerySingleAsync return null

        var result = await _repository.GetMigrationByNameAsync("NonExistent");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetMigrationByNameAsync_WhenExceptionThrown_ReturnsNull()
    {
        _mockDatabaseAccessor.ShouldThrowException = true;
        _mockDatabaseAccessor.ExceptionMessage = "Database error";

        var result = await _repository.GetMigrationByNameAsync("00001-TestMigration");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task AddMigrationAsync_CallsExecuteAsync()
    {
        var migration = new NewMigration("00001-TestMigration", DateTime.Now);
        _mockDatabaseAccessor.ExecuteAsyncReturnValue = 1;

        var result = await _repository.AddMigrationAsync(migration);

        Assert.That(result, Is.True);
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls, Has.Count.EqualTo(1));
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls[0], Contains.Substring("INSERT INTO [_Migrations]"));
    }

    [Test]
    public async Task AddMigrationAsync_WithCustomTableName_CallsExecuteAsync()
    {
        var migration = new NewMigration("00001-TestMigration", DateTime.Now);
        _mockDatabaseAccessor.ExecuteAsyncReturnValue = 1;

        var result = await _repository.AddMigrationAsync(migration, "CustomMigrations");

        Assert.That(result, Is.True);
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls, Has.Count.EqualTo(1));
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls[0], Contains.Substring("INSERT INTO [CustomMigrations]"));
    }

    [Test]
    public async Task AddMigrationAsync_WhenNoRowsAffected_ReturnsFalse()
    {
        var migration = new NewMigration("00001-TestMigration", DateTime.Now);
        _mockDatabaseAccessor.ExecuteAsyncReturnValue = 0;

        var result = await _repository.AddMigrationAsync(migration);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task AddMigrationAsync_WhenExceptionThrown_ReturnsFalse()
    {
        var migration = new NewMigration("00001-TestMigration", DateTime.Now);
        _mockDatabaseAccessor.ShouldThrowException = true;
        _mockDatabaseAccessor.ExceptionMessage = "Database error";

        var result = await _repository.AddMigrationAsync(migration);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UpdateMigrationAsync_CallsExecuteAsync()
    {
        var appliedAt = DateTime.Now;
        _mockDatabaseAccessor.ExecuteAsyncReturnValue = 1;

        var result = await _repository.UpdateMigrationAsync("00001-TestMigration", appliedAt, null);

        Assert.That(result, Is.True);
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls, Has.Count.EqualTo(1));
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls[0], Contains.Substring("UPDATE [_Migrations]"));
    }

    [Test]
    public async Task UpdateMigrationAsync_WithErrorMessage_CallsExecuteAsync()
    {
        var appliedAt = DateTime.Now;
        var errorMessage = "Migration failed";
        _mockDatabaseAccessor.ExecuteAsyncReturnValue = 1;

        var result = await _repository.UpdateMigrationAsync("00001-TestMigration", appliedAt, errorMessage, "CustomMigrations");

        Assert.That(result, Is.True);
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls, Has.Count.EqualTo(1));
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls[0], Contains.Substring("UPDATE [CustomMigrations]"));
    }

    [Test]
    public async Task UpdateMigrationAsync_WhenNoRowsAffected_ReturnsFalse()
    {
        _mockDatabaseAccessor.ExecuteAsyncReturnValue = 0;

        var result = await _repository.UpdateMigrationAsync("NonExistent", DateTime.Now, null);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UpdateMigrationAsync_WhenExceptionThrown_ReturnsFalse()
    {
        _mockDatabaseAccessor.ShouldThrowException = true;
        _mockDatabaseAccessor.ExceptionMessage = "Database error";

        var result = await _repository.UpdateMigrationAsync("00001-TestMigration", DateTime.Now, null);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ClearAllUserTablesAsync_ClearsAllDatabaseObjectsInCorrectOrder()
    {
        _mockDatabaseAccessor.ExecuteAsyncReturnValue = 1;

        var result = await _repository.ClearAllUserTablesAsync();

        Assert.That(result, Is.True);

        // Should query for: foreign keys, functions, procedures, views, and tables
        Assert.That(_mockDatabaseAccessor.QueryAsyncCalls, Has.Count.EqualTo(5));

        // Verify each query type
        Assert.That(_mockDatabaseAccessor.QueryAsyncCalls[0], Contains.Substring("sys.foreign_keys"));
        Assert.That(_mockDatabaseAccessor.QueryAsyncCalls[1], Contains.Substring("ROUTINE_TYPE = 'FUNCTION'"));
        Assert.That(_mockDatabaseAccessor.QueryAsyncCalls[2], Contains.Substring("ROUTINE_TYPE = 'PROCEDURE'"));
        Assert.That(_mockDatabaseAccessor.QueryAsyncCalls[3], Contains.Substring("INFORMATION_SCHEMA.VIEWS"));
        Assert.That(_mockDatabaseAccessor.QueryAsyncCalls[4], Contains.Substring("INFORMATION_SCHEMA.TABLES"));

        // Should execute DROP commands for all object types:
        // 1 FK + 1 function + 1 procedure + 1 view + 3 tables = 7
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls, Has.Count.EqualTo(7));
    }

    [Test]
    public async Task ClearAllUserTablesAsync_DropStatements_ContainFullyQualifiedNames()
    {
        // Regression test for GitHub Issue #1:
        // Original bug: QueryAsync<string> only read first column (TABLE_SCHEMA),
        // producing "DROP TABLE IF EXISTS [dbo]" instead of "DROP TABLE IF EXISTS [dbo].[TableName]"
        _mockDatabaseAccessor.ExecuteAsyncReturnValue = 1;

        var result = await _repository.ClearAllUserTablesAsync();

        Assert.That(result, Is.True);

        var executeCalls = _mockDatabaseAccessor.ExecuteAsyncCalls;

        // FK constraint: must reference both schema.table and constraint name
        Assert.That(executeCalls[0], Contains.Substring("[dbo].[Orders]"));
        Assert.That(executeCalls[0], Contains.Substring("[FK_Orders_Customers]"));

        // Function: must include schema and function name
        Assert.That(executeCalls[1], Contains.Substring("DROP FUNCTION IF EXISTS [dbo].[GetTotal]"));

        // Procedure: must include schema and procedure name
        Assert.That(executeCalls[2], Contains.Substring("DROP PROCEDURE IF EXISTS [dbo].[UpdateStats]"));

        // View: must include schema and view name
        Assert.That(executeCalls[3], Contains.Substring("DROP VIEW IF EXISTS [dbo].[ActiveOrders]"));

        // Tables: must include schema and table name for each
        Assert.That(executeCalls[4], Contains.Substring("DROP TABLE IF EXISTS [dbo].[Table1]"));
        Assert.That(executeCalls[5], Contains.Substring("DROP TABLE IF EXISTS [dbo].[Table2]"));
        Assert.That(executeCalls[6], Contains.Substring("DROP TABLE IF EXISTS [dbo].[Table3]"));
    }

    [Test]
    public async Task ClearAllUserTablesAsync_WhenNoTables_ReturnsTrue()
    {
        _mockDatabaseAccessor.ShouldReturnDatabaseObjects = false;

        var result = await _repository.ClearAllUserTablesAsync();

        Assert.That(result, Is.True);
        // ClearAllUserTablesAsync always queries for all 5 object types:
        // foreign keys, functions, procedures, views, tables
        Assert.That(_mockDatabaseAccessor.QueryAsyncCalls, Has.Count.EqualTo(5));
        // No DROP statements should be executed when there are no objects
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task ClearAllUserTablesAsync_WhenExceptionThrown_ReturnsFalse()
    {
        _mockDatabaseAccessor.ShouldThrowException = true;
        _mockDatabaseAccessor.ExceptionMessage = "Database error";

        var result = await _repository.ClearAllUserTablesAsync();

        Assert.That(result, Is.False);
    }

    #region SQL Injection Protection Tests

    [Test]
    public void CreateMigrationTableAsync_WithSqlInjectionTableName_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _repository.CreateMigrationTableAsync("'; DROP TABLE Users; --"));
    }

    [Test]
    public void MigrationTableExistsAsync_WithSqlInjectionTableName_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _repository.MigrationTableExistsAsync("]; DROP TABLE Users; --"));
    }

    [Test]
    public void GetAllMigrationsAsync_WithSqlInjectionTableName_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _repository.GetAllMigrationsAsync("dbo.Users"));
    }

    [Test]
    public void AddMigrationAsync_WithSqlInjectionTableName_ThrowsArgumentException()
    {
        var migration = new NewMigration("00001-Test", DateTime.Now);
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _repository.AddMigrationAsync(migration, "table; DROP TABLE x"));
    }

    [Test]
    public void UpdateMigrationAsync_WithSqlInjectionTableName_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _repository.UpdateMigrationAsync("00001-Test", DateTime.Now, null, "[evil]"));
    }

    [Test]
    public async Task CreateMigrationTableAsync_WithValidTableName_Succeeds()
    {
        _mockDatabaseAccessor.ExecuteAsyncReturnValue = 1;
        var result = await _repository.CreateMigrationTableAsync("ValidTable_123");
        Assert.That(result, Is.True);
    }

    #endregion

    #region ExecuteMigrationSqlAsync Tests

    [Test]
    public async Task ExecuteMigrationSqlAsync_WithSimpleSql_ExecutesSuccessfully()
    {
        _mockDatabaseAccessor.ExecuteInTransactionAsyncReturnValue = true;

        var result = await _repository.ExecuteMigrationSqlAsync("CREATE TABLE Test (Id INT);");

        Assert.That(result, Is.True);
        Assert.That(_mockDatabaseAccessor.ExecuteInTransactionAsyncCallCount, Is.EqualTo(1));
    }

    [Test]
    public void ExecuteMigrationSqlAsync_WhenExceptionThrown_Throws()
    {
        _mockDatabaseAccessor.ShouldThrowException = true;
        _mockDatabaseAccessor.ExceptionMessage = "SQL error";

        Assert.ThrowsAsync<Exception>(async () =>
            await _repository.ExecuteMigrationSqlAsync("INVALID SQL"));
    }

    #endregion
}
