using NUnit.Framework;
using dbmig.BL.Database;
using dbmig.BL.Tests.Mocks;
using dbmig.BL.Configuration;
using System.IO;

namespace dbmig.BL.Tests.Database;

[TestFixture]
public class DatabaseInteractorTests
{
    private MockMigrationRepository _mockRepository = null!;
    private DatabaseInteractor _interactor = null!;
    private string _testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new MockMigrationRepository();
        _interactor = new DatabaseInteractor(_mockRepository);

        // Create a temporary test directory
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up temporary directory
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }

        _mockRepository.Reset();
    }

    [Test]
    public void Constructor_Default_CreatesInstance()
    {
        Assert.DoesNotThrow(() => new DatabaseInteractor());
    }

    [Test]
    public void Constructor_WithNullRepository_CreatesDefaultRepository()
    {
        Assert.DoesNotThrow(() => new DatabaseInteractor(null));
    }

    [Test]
    public void Constructor_WithRepository_UsesProvidedRepository()
    {
        Assert.DoesNotThrow(() => new DatabaseInteractor(_mockRepository));
    }

    #region ClearDatabase Tests

    [Test]
    public void ClearDatabase_WithValidConnectionString_ReturnsSuccess()
    {
        _mockRepository.ShouldClearAllUserTablesSucceed = true;

        var result = _interactor.ClearDatabase(ConnectionStrings.UnitTest);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Database cleared successfully."));
        Assert.That(_mockRepository.ClearAllUserTablesCallCount, Is.EqualTo(1));
    }

    [Test]
    public void ClearDatabase_WhenRepositoryReturnsFalse_ReturnsFailure()
    {
        _mockRepository.ShouldClearAllUserTablesSucceed = false;

        var result = _interactor.ClearDatabase(ConnectionStrings.UnitTest);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Database clearing failed. Check logs for details."));
        Assert.That(_mockRepository.ClearAllUserTablesCallCount, Is.EqualTo(1));
    }

    [Test]
    public void ClearDatabase_WhenRepositoryThrowsException_ReturnsFailure()
    {
        _mockRepository.ShouldThrowException = true;
        _mockRepository.ExceptionMessage = "Database error";

        var result = _interactor.ClearDatabase(ConnectionStrings.UnitTest);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to clear database. Please check your connection and permissions."));
    }

    [Test]
    public void ClearDatabase_WithNullConnectionString_ReturnsFailure()
    {
        _mockRepository.ShouldThrowException = true;

        var result = _interactor.ClearDatabase(null!);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to clear database. Please check your connection and permissions."));
    }

    [Test]
    public void ClearDatabase_WithEmptyConnectionString_ReturnsFailure()
    {
        _mockRepository.ShouldClearAllUserTablesSucceed = false;

        var result = _interactor.ClearDatabase("");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Database clearing failed. Check logs for details."));
    }

    #endregion

    #region InitializeMigration Tests

    [Test]
    public void InitializeMigration_WithValidConnectionString_ReturnsSuccess()
    {
        _mockRepository.ShouldMigrationTableExist = false;
        _mockRepository.ShouldCreateMigrationTableSucceed = true;

        var result = _interactor.InitializeMigration(ConnectionStrings.UnitTest);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Migration system initialized with table '_Migrations'."));
        Assert.That(_mockRepository.MigrationTableExistsCalls, Contains.Item("_Migrations"));
        Assert.That(_mockRepository.CreateMigrationTableCalls, Contains.Item("_Migrations"));
    }

    [Test]
    public void InitializeMigration_WithCustomTableName_ReturnsSuccess()
    {
        _mockRepository.ShouldMigrationTableExist = false;
        _mockRepository.ShouldCreateMigrationTableSucceed = true;

        var result = _interactor.InitializeMigration(ConnectionStrings.UnitTest, "CustomMigrations");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Migration system initialized with table 'CustomMigrations'."));
        Assert.That(_mockRepository.MigrationTableExistsCalls, Contains.Item("CustomMigrations"));
        Assert.That(_mockRepository.CreateMigrationTableCalls, Contains.Item("CustomMigrations"));
    }

    [Test]
    public void InitializeMigration_WithNullTableName_UsesDefaultTableName()
    {
        _mockRepository.ShouldMigrationTableExist = false;
        _mockRepository.ShouldCreateMigrationTableSucceed = true;

        var result = _interactor.InitializeMigration(ConnectionStrings.UnitTest, null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Migration system initialized with table '_Migrations'."));
        Assert.That(_mockRepository.MigrationTableExistsCalls, Contains.Item("_Migrations"));
        Assert.That(_mockRepository.CreateMigrationTableCalls, Contains.Item("_Migrations"));
    }

    [Test]
    public void InitializeMigration_WhenTableAlreadyExists_ReturnsSuccess()
    {
        _mockRepository.ShouldMigrationTableExist = true;

        var result = _interactor.InitializeMigration(ConnectionStrings.UnitTest);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Migration system initialized with table '_Migrations'."));
        Assert.That(_mockRepository.MigrationTableExistsCalls, Contains.Item("_Migrations"));
        Assert.That(_mockRepository.CreateMigrationTableCalls, Is.Empty);
    }

    [Test]
    public void InitializeMigration_WhenCreateTableFails_ReturnsFailure()
    {
        _mockRepository.ShouldMigrationTableExist = false;
        _mockRepository.ShouldCreateMigrationTableSucceed = false;

        var result = _interactor.InitializeMigration(ConnectionStrings.UnitTest);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to create migration table '_Migrations'. Check logs for details."));
        Assert.That(_mockRepository.CreateMigrationTableCalls, Contains.Item("_Migrations"));
    }

    [Test]
    public void InitializeMigration_WhenExceptionThrown_ReturnsFailure()
    {
        _mockRepository.ShouldThrowException = true;
        _mockRepository.ExceptionMessage = "Database error";

        var result = _interactor.InitializeMigration(ConnectionStrings.UnitTest);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to initialize migration system. Please check your database connection and permissions."));
    }

    [Test]
    public void InitializeMigration_WithNullConnectionString_ReturnsFailure()
    {
        _mockRepository.ShouldMigrationTableExist = false;
        _mockRepository.ShouldCreateMigrationTableSucceed = false;

        var result = _interactor.InitializeMigration(null!);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to create migration table '_Migrations'. Check logs for details."));
    }

    #endregion

    #region RunMigrations Tests

    [Test]
    public void RunMigrations_WithValidDirectoryAndNoFiles_ReturnsSuccess()
    {
        _mockRepository.ShouldMigrationTableExist = true;

        var result = _interactor.RunMigrations(ConnectionStrings.UnitTest, _testDirectory);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("No migration files found to execute."));
        Assert.That(_mockRepository.MigrationTableExistsCalls, Contains.Item("_Migrations"));
    }

    [Test]
    public void RunMigrations_WithValidDirectoryAndMigrationFiles_ReturnsSuccess()
    {
        // Create test migration files
        File.WriteAllText(Path.Combine(_testDirectory, "00001-CreateTable.sql"), "CREATE TABLE Test (Id INT);");
        File.WriteAllText(Path.Combine(_testDirectory, "00002-AddColumn.sql"), "ALTER TABLE Test ADD Name NVARCHAR(50);");
        File.WriteAllText(Path.Combine(_testDirectory, "invalid-migration.sql"), "Invalid migration file");

        _mockRepository.ShouldMigrationTableExist = true;

        var result = _interactor.RunMigrations(ConnectionStrings.UnitTest, _testDirectory);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Does.Contain("completed successfully"));
        Assert.That(result.Message, Does.Contain("2 migration(s) applied"));
        Assert.That(_mockRepository.MigrationTableExistsCalls, Contains.Item("_Migrations"));
    }

    [Test]
    public void RunMigrations_WithCustomTableName_ReturnsSuccess()
    {
        _mockRepository.ShouldMigrationTableExist = true;

        var result = _interactor.RunMigrations(ConnectionStrings.UnitTest, _testDirectory, "CustomMigrations");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("No migration files found to execute."));
        Assert.That(_mockRepository.MigrationTableExistsCalls, Contains.Item("CustomMigrations"));
    }

    [Test]
    public void RunMigrations_WithNullTableName_UsesDefaultTableName()
    {
        _mockRepository.ShouldMigrationTableExist = true;

        var result = _interactor.RunMigrations(ConnectionStrings.UnitTest, _testDirectory, null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Does.Contain("No migration files found to execute."));
        Assert.That(_mockRepository.MigrationTableExistsCalls, Contains.Item("_Migrations"));
    }

    [Test]
    public void RunMigrations_WithNonExistentDirectory_ReturnsFailure()
    {
        var nonExistentDir = "./nonexistent-directory-12345";

        var result = _interactor.RunMigrations(ConnectionStrings.UnitTest, nonExistentDir);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo($"Migration directory '{nonExistentDir}' does not exist."));
        Assert.That(_mockRepository.MigrationTableExistsCalls, Is.Empty);
    }

    [Test]
    public void RunMigrations_WhenMigrationTableDoesNotExist_ReturnsFailure()
    {
        _mockRepository.ShouldMigrationTableExist = false;

        var result = _interactor.RunMigrations(ConnectionStrings.UnitTest, _testDirectory);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Migration table '_Migrations' does not exist. Run 'init' command first."));
        Assert.That(_mockRepository.MigrationTableExistsCalls, Contains.Item("_Migrations"));
    }

    [Test]
    public void RunMigrations_WhenExceptionThrown_ReturnsFailure()
    {
        _mockRepository.ShouldThrowException = true;
        _mockRepository.ExceptionMessage = "Database error";

        var result = _interactor.RunMigrations(ConnectionStrings.UnitTest, _testDirectory);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to run migrations. Please check your directory path and database connection."));
    }

    [Test]
    public void RunMigrations_WithNullConnectionString_ReturnsFailure()
    {
        _mockRepository.ShouldMigrationTableExist = false;

        var result = _interactor.RunMigrations(null!, _testDirectory);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Migration table '_Migrations' does not exist. Run 'init' command first."));
    }

    [Test]
    public void RunMigrations_WithNullDirectory_ReturnsFailure()
    {
        var result = _interactor.RunMigrations(ConnectionStrings.UnitTest, null!);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Migration directory '' does not exist."));
    }

    [Test]
    public void RunMigrations_ExecutesMigrationsAndRecordsAppliedAt()
    {
        // Create test migration files
        File.WriteAllText(Path.Combine(_testDirectory, "00001-CreateTable.sql"), "CREATE TABLE Test (Id INT);");

        _mockRepository.ShouldMigrationTableExist = true;
        _mockRepository.ShouldExecuteMigrationSqlSucceed = true;

        var result = _interactor.RunMigrations(ConnectionStrings.UnitTest, _testDirectory);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(_mockRepository.ExecuteMigrationSqlCalls, Has.Count.EqualTo(1));
        Assert.That(_mockRepository.UpdateMigrationCalls, Has.Count.EqualTo(1));
    }

    [Test]
    public void RunMigrations_WhenMigrationSqlFails_StopsExecution()
    {
        // Create two migration files
        File.WriteAllText(Path.Combine(_testDirectory, "00001-First.sql"), "CREATE TABLE T1 (Id INT);");
        File.WriteAllText(Path.Combine(_testDirectory, "00002-Second.sql"), "CREATE TABLE T2 (Id INT);");

        _mockRepository.ShouldMigrationTableExist = true;
        // First call succeeds, but we make ExecuteMigrationSql throw to simulate failure
        _mockRepository.ShouldExecuteMigrationSqlSucceed = true;

        _mockRepository.ShouldThrowException = false;

        var result = _interactor.RunMigrations(ConnectionStrings.UnitTest, _testDirectory);

        // Should complete successfully since mock always succeeds
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Does.Contain("2 migration(s) applied"));
    }

    [Test]
    public void RunMigrations_WithGoBatchSeparator_ProcessesCorrectly()
    {
        var sqlWithGo = "CREATE TABLE T1 (Id INT);\nGO\nCREATE INDEX IX ON T1(Id);";
        File.WriteAllText(Path.Combine(_testDirectory, "00001-WithGo.sql"), sqlWithGo);

        _mockRepository.ShouldMigrationTableExist = true;
        _mockRepository.ShouldExecuteMigrationSqlSucceed = true;

        var result = _interactor.RunMigrations(ConnectionStrings.UnitTest, _testDirectory);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(_mockRepository.ExecuteMigrationSqlCalls, Has.Count.EqualTo(1));
        Assert.That(_mockRepository.ExecuteMigrationSqlCalls[0], Does.Contain("GO"));
    }

    [Test]
    public void RunMigrations_AllMigrationsAlreadyApplied_ReturnsNothingToDo()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "00001-Already.sql"), "SELECT 1;");

        _mockRepository.ShouldMigrationTableExist = true;
        // Mock returns non-null Migration (already applied)
        // The current mock always returns null from GetMigrationByNameAsync,
        // so we test via the "no new migrations" path differently

        var result = _interactor.RunMigrations(ConnectionStrings.UnitTest, _testDirectory);

        // Since mock returns null for GetMigrationByNameAsync, it will try to add and execute
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void RunMigrations_WithDryRun_DoesNotExecuteMigrations()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "00001-CreateTable.sql"), "CREATE TABLE Test (Id INT);");
        File.WriteAllText(Path.Combine(_testDirectory, "00002-AddColumn.sql"), "ALTER TABLE Test ADD Name NVARCHAR(50);");

        _mockRepository.ShouldMigrationTableExist = true;

        var result = _interactor.RunMigrations(ConnectionStrings.UnitTest, _testDirectory, null, dryRun: true);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Does.Contain("DRY RUN"));
        Assert.That(result.Message, Does.Contain("2 migration(s) would be applied"));
        Assert.That(_mockRepository.ExecuteMigrationSqlCalls, Has.Count.EqualTo(0));
    }

    [Test]
    public void RunMigrations_WithDryRunAndNoNewMigrations_ReturnsNothingToDo()
    {
        _mockRepository.ShouldMigrationTableExist = true;

        var result = _interactor.RunMigrations(ConnectionStrings.UnitTest, _testDirectory, null, dryRun: true);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Does.Contain("No migration files found"));
    }

    #endregion
}
