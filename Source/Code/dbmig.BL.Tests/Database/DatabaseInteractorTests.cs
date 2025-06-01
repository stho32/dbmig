using NUnit.Framework;
using dbmig.BL.Database;

namespace dbmig.BL.Tests.Database;

[TestFixture]
public class DatabaseInteractorTests
{
    private DatabaseInteractor _interactor = null!;

    [SetUp]
    public void SetUp()
    {
        _interactor = new DatabaseInteractor();
    }

    [Test]
    public void ClearDatabase_WithValidConnectionString_ReturnsSuccess()
    {
        var result = _interactor.ClearDatabase("Server=localhost;Database=Test;Integrated Security=true;");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Database cleared successfully."));
    }

    [Test]
    public void ClearDatabase_WithEmptyConnectionString_ReturnsSuccess()
    {
        var result = _interactor.ClearDatabase("");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Database cleared successfully."));
    }

    [Test]
    public void InitializeMigration_WithoutTableName_ReturnsSuccessWithDefaultTable()
    {
        var result = _interactor.InitializeMigration("Server=localhost;Database=Test;");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Migration system initialized with table '_Migrations'."));
    }

    [Test]
    public void InitializeMigration_WithCustomTableName_ReturnsSuccessWithCustomTable()
    {
        var result = _interactor.InitializeMigration("Server=localhost;Database=Test;", "CustomMigrations");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Migration system initialized with table 'CustomMigrations'."));
    }

    [Test]
    public void InitializeMigration_WithNullTableName_ReturnsSuccessWithDefaultTable()
    {
        var result = _interactor.InitializeMigration("Server=localhost;Database=Test;", null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Is.EqualTo("Migration system initialized with table '_Migrations'."));
    }

    [Test]
    public void RunMigrations_WithValidDirectory_ReturnsSuccess()
    {
        var tempDir = Path.GetTempPath();
        var result = _interactor.RunMigrations("Server=localhost;Database=Test;", tempDir);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Does.Contain($"Migrations from '{tempDir}' executed successfully"));
        Assert.That(result.Message, Does.Contain("using table '_Migrations'"));
    }

    [Test]
    public void RunMigrations_WithValidDirectoryAndCustomTable_ReturnsSuccess()
    {
        var tempDir = Path.GetTempPath();
        var result = _interactor.RunMigrations("Server=localhost;Database=Test;", tempDir, "CustomMigrations");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Does.Contain($"Migrations from '{tempDir}' executed successfully"));
        Assert.That(result.Message, Does.Contain("using table 'CustomMigrations'"));
    }

    [Test]
    public void RunMigrations_WithNonExistentDirectory_ReturnsFailure()
    {
        var nonExistentDir = "./nonexistent-directory-12345";
        var result = _interactor.RunMigrations("Server=localhost;Database=Test;", nonExistentDir);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo($"Migration directory '{nonExistentDir}' does not exist."));
    }

    [Test]
    public void RunMigrations_WithEmptyDirectory_ReturnsFailure()
    {
        var result = _interactor.RunMigrations("Server=localhost;Database=Test;", "");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("does not exist"));
    }

    [Test]
    public void RunMigrations_WithNullTableName_ReturnsSuccessWithDefaultTable()
    {
        var tempDir = Path.GetTempPath();
        var result = _interactor.RunMigrations("Server=localhost;Database=Test;", tempDir, null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Does.Contain("using table '_Migrations'"));
    }
}