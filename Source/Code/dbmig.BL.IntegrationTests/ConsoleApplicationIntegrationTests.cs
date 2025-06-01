using NUnit.Framework;
using dbmig.BL;
using dbmig.BL.Common;
using dbmig.BL.CommandInterpreter;
using dbmig.BL.Database;

namespace dbmig.BL.IntegrationTests;

[TestFixture]
public class ConsoleApplicationIntegrationTests
{
    private CommandInterpreterInteractor _commandInterpreter = null!;
    private DatabaseInteractor _databaseInteractor = null!;

    [SetUp]
    public void SetUp()
    {
        _commandInterpreter = InteractorFactory.GetCommandInterpreterInteractor();
        _databaseInteractor = InteractorFactory.GetDatabaseInteractor();
    }

    [Test]
    public void EndToEnd_ClearDbCommand_WorksCorrectly()
    {
        var args = new[] { "-c", "Server=localhost;Database=TestDb;Integrated Security=true;", "cleardb" };
        
        var parseResult = _commandInterpreter.ParseArguments(args);
        
        Assert.That(parseResult.IsSuccess, Is.True);
        Assert.That(parseResult.CommandInfo, Is.Not.Null);
        
        var executeResult = _databaseInteractor.ClearDatabase(parseResult.CommandInfo!.ConnectionString);
        
        Assert.That(executeResult.IsSuccess, Is.True);
        Assert.That(executeResult.Message, Is.EqualTo("Database cleared successfully."));
    }

    [Test]
    public void EndToEnd_InitCommand_WithDefaultTable_WorksCorrectly()
    {
        var args = new[] { "-c", "Server=localhost;Database=TestDb;Integrated Security=true;", "init" };
        
        var parseResult = _commandInterpreter.ParseArguments(args);
        
        Assert.That(parseResult.IsSuccess, Is.True);
        Assert.That(parseResult.CommandInfo, Is.Not.Null);
        
        var tableName = parseResult.CommandInfo!.Parameters.GetValueOrDefault("MigrationTableName") as string;
        var executeResult = _databaseInteractor.InitializeMigration(parseResult.CommandInfo.ConnectionString, tableName);
        
        Assert.That(executeResult.IsSuccess, Is.True);
        Assert.That(executeResult.Message, Is.EqualTo("Migration system initialized with table '_Migrations'."));
    }

    [Test]
    public void EndToEnd_InitCommand_WithCustomTable_WorksCorrectly()
    {
        var args = new[] { "-c", "Server=localhost;Database=TestDb;Integrated Security=true;", "init", "CustomMigrations" };
        
        var parseResult = _commandInterpreter.ParseArguments(args);
        
        Assert.That(parseResult.IsSuccess, Is.True);
        Assert.That(parseResult.CommandInfo, Is.Not.Null);
        
        var tableName = parseResult.CommandInfo!.Parameters.GetValueOrDefault("MigrationTableName") as string;
        var executeResult = _databaseInteractor.InitializeMigration(parseResult.CommandInfo.ConnectionString, tableName);
        
        Assert.That(executeResult.IsSuccess, Is.True);
        Assert.That(executeResult.Message, Is.EqualTo("Migration system initialized with table 'CustomMigrations'."));
    }

    [Test]
    public void EndToEnd_MigrateCommand_WithValidDirectory_WorksCorrectly()
    {
        var tempDir = Path.GetTempPath();
        var args = new[] { "-c", "Server=localhost;Database=TestDb;Integrated Security=true;", "migrate", tempDir };
        
        var parseResult = _commandInterpreter.ParseArguments(args);
        
        Assert.That(parseResult.IsSuccess, Is.True);
        Assert.That(parseResult.CommandInfo, Is.Not.Null);
        
        var directory = parseResult.CommandInfo!.Parameters.GetValueOrDefault("Directory") as string;
        var tableName = parseResult.CommandInfo.Parameters.GetValueOrDefault("MigrationTableName") as string;
        
        var executeResult = _databaseInteractor.RunMigrations(parseResult.CommandInfo.ConnectionString, directory ?? string.Empty, tableName);
        
        Assert.That(executeResult.IsSuccess, Is.True);
        Assert.That(executeResult.Message, Does.Contain("executed successfully"));
    }

    [Test]
    public void EndToEnd_MigrateCommand_WithNonExistentDirectory_ReturnsFailure()
    {
        var nonExistentDir = "./nonexistent-integration-test-dir";
        var args = new[] { "-c", "Server=localhost;Database=TestDb;Integrated Security=true;", "migrate", nonExistentDir };
        
        var parseResult = _commandInterpreter.ParseArguments(args);
        
        Assert.That(parseResult.IsSuccess, Is.True);
        Assert.That(parseResult.CommandInfo, Is.Not.Null);
        
        var directory = parseResult.CommandInfo!.Parameters.GetValueOrDefault("Directory") as string;
        var tableName = parseResult.CommandInfo.Parameters.GetValueOrDefault("MigrationTableName") as string;
        
        var executeResult = _databaseInteractor.RunMigrations(parseResult.CommandInfo.ConnectionString, directory ?? string.Empty, tableName);
        
        Assert.That(executeResult.IsSuccess, Is.False);
        Assert.That(executeResult.Message, Does.Contain("does not exist"));
    }

    [Test]
    public void EndToEnd_InvalidCommand_ReturnsParseFailure()
    {
        var args = new[] { "-c", "Server=localhost;Database=TestDb;Integrated Security=true;", "invalidcommand" };
        
        var parseResult = _commandInterpreter.ParseArguments(args);
        
        Assert.That(parseResult.IsSuccess, Is.False);
        Assert.That(parseResult.CommandInfo, Is.Null);
        Assert.That(parseResult.Message, Does.Contain("Action is required"));
    }

    [Test]
    public void EndToEnd_MissingConnectionString_ReturnsParseFailure()
    {
        var args = new[] { "cleardb" };
        
        var parseResult = _commandInterpreter.ParseArguments(args);
        
        Assert.That(parseResult.IsSuccess, Is.False);
        Assert.That(parseResult.CommandInfo, Is.Null);
        Assert.That(parseResult.Message, Does.Contain("Connection string is required"));
    }

    [Test]
    public void EndToEnd_HelpCommand_ReturnsHelpText()
    {
        var args = new[] { "--help" };
        
        var parseResult = _commandInterpreter.ParseArguments(args);
        
        Assert.That(parseResult.IsSuccess, Is.True);
        Assert.That(parseResult.CommandInfo, Is.Null);
        Assert.That(parseResult.Message, Does.Contain("dbmig - Database Migration Tool"));
        Assert.That(parseResult.Message, Does.Contain("Usage:"));
        Assert.That(parseResult.Message, Does.Contain("Examples:"));
    }

    [Test]
    public void InteractorFactory_ReturnsWorkingInteractors()
    {
        var commandInteractor = InteractorFactory.GetCommandInterpreterInteractor();
        var databaseInteractor = InteractorFactory.GetDatabaseInteractor();
        
        Assert.That(commandInteractor, Is.Not.Null);
        Assert.That(databaseInteractor, Is.Not.Null);
        
        var testArgs = new[] { "-c", "test", "cleardb" };
        var parseResult = commandInteractor.ParseArguments(testArgs);
        
        Assert.That(parseResult.IsSuccess, Is.True);
        
        var dbResult = databaseInteractor.ClearDatabase("test");
        Assert.That(dbResult.IsSuccess, Is.True);
    }
}
