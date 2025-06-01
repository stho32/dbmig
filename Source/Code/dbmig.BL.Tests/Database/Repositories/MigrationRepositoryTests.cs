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
    public async Task ClearAllUserTablesAsync_CallsQueryAsync()
    {
        _mockDatabaseAccessor.ExecuteAsyncReturnValue = 1;

        var result = await _repository.ClearAllUserTablesAsync();

        Assert.That(result, Is.True);
        Assert.That(_mockDatabaseAccessor.QueryAsyncCalls, Has.Count.EqualTo(1));
        Assert.That(_mockDatabaseAccessor.QueryAsyncCalls[0], Contains.Substring("INFORMATION_SCHEMA.TABLES"));
        Assert.That(_mockDatabaseAccessor.ExecuteAsyncCalls, Has.Count.EqualTo(3)); // 3 tables
    }

    [Test]
    public async Task ClearAllUserTablesAsync_WhenNoTables_ReturnsTrue()
    {
        // Configure mock to return empty table list
        _mockDatabaseAccessor.QueryAsyncCalls.Clear();

        var result = await _repository.ClearAllUserTablesAsync();

        Assert.That(result, Is.True);
        Assert.That(_mockDatabaseAccessor.QueryAsyncCalls, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ClearAllUserTablesAsync_WhenExceptionThrown_ReturnsFalse()
    {
        _mockDatabaseAccessor.ShouldThrowException = true;
        _mockDatabaseAccessor.ExceptionMessage = "Database error";

        var result = await _repository.ClearAllUserTablesAsync();

        Assert.That(result, Is.False);
    }
}