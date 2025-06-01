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
        Assert.That(result.Message, Is.EqualTo($"Migrations from '{_testDirectory}' processed successfully using table '_Migrations'."));
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

    #endregion
}