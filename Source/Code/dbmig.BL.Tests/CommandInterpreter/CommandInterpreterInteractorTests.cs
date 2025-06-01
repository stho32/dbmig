using NUnit.Framework;
using dbmig.BL.CommandInterpreter;
using dbmig.BL.Common;

namespace dbmig.BL.Tests.CommandInterpreter;

[TestFixture]
public class CommandInterpreterInteractorTests
{
    private CommandInterpreterInteractor _interactor = null!;

    [SetUp]
    public void SetUp()
    {
        _interactor = new CommandInterpreterInteractor();
    }

    [Test]
    public void ParseArguments_WithNoArguments_ReturnsFailure()
    {
        var result = _interactor.ParseArguments(new string[0]);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("No arguments provided"));
        Assert.That(result.CommandInfo, Is.Null);
    }

    [Test]
    public void ParseArguments_WithHelpFlag_ReturnsSuccessWithHelpText()
    {
        var result = _interactor.ParseArguments(new[] { "--help" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Does.Contain("dbmig - Database Migration Tool"));
        Assert.That(result.CommandInfo, Is.Null);
    }

    [Test]
    public void ParseArguments_WithShortHelpFlag_ReturnsSuccessWithHelpText()
    {
        var result = _interactor.ParseArguments(new[] { "-h" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Message, Does.Contain("dbmig - Database Migration Tool"));
        Assert.That(result.CommandInfo, Is.Null);
    }

    [Test]
    public void ParseArguments_WithoutConnectionString_ReturnsFailure()
    {
        var result = _interactor.ParseArguments(new[] { "cleardb" });

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Connection string is required"));
        Assert.That(result.CommandInfo, Is.Null);
    }

    [Test]
    public void ParseArguments_WithoutAction_ReturnsFailure()
    {
        var result = _interactor.ParseArguments(new[] { "-c", "Server=localhost;" });

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Action is required"));
        Assert.That(result.CommandInfo, Is.Null);
    }

    [Test]
    public void ParseArguments_WithValidClearDbCommand_ReturnsSuccess()
    {
        var result = _interactor.ParseArguments(new[] { "-c", "Server=localhost;Database=Test;", "cleardb" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.CommandInfo, Is.Not.Null);
        Assert.That(result.CommandInfo!.Action, Is.EqualTo("cleardb"));
        Assert.That(result.CommandInfo!.ConnectionString, Is.EqualTo("Server=localhost;Database=Test;"));
        Assert.That(result.CommandInfo!.Parameters.Count, Is.EqualTo(0));
    }

    [Test]
    public void ParseArguments_WithLongConnectionStringFlag_ReturnsSuccess()
    {
        var result = _interactor.ParseArguments(new[] { "--connection-string", "Server=localhost;Database=Test;", "cleardb" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.CommandInfo, Is.Not.Null);
        Assert.That(result.CommandInfo!.ConnectionString, Is.EqualTo("Server=localhost;Database=Test;"));
    }

    [Test]
    public void ParseArguments_WithInitCommand_WithoutTableName_ReturnsSuccess()
    {
        var result = _interactor.ParseArguments(new[] { "-c", "Server=localhost;", "init" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.CommandInfo, Is.Not.Null);
        Assert.That(result.CommandInfo!.Action, Is.EqualTo("init"));
        Assert.That(result.CommandInfo!.Parameters.Count, Is.EqualTo(0));
    }

    [Test]
    public void ParseArguments_WithInitCommand_WithTableName_ReturnsSuccess()
    {
        var result = _interactor.ParseArguments(new[] { "-c", "Server=localhost;", "init", "CustomMigrations" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.CommandInfo, Is.Not.Null);
        Assert.That(result.CommandInfo!.Action, Is.EqualTo("init"));
        Assert.That(result.CommandInfo!.Parameters["MigrationTableName"], Is.EqualTo("CustomMigrations"));
    }

    [Test]
    public void ParseArguments_WithMigrateCommand_WithDirectory_ReturnsSuccess()
    {
        var result = _interactor.ParseArguments(new[] { "-c", "Server=localhost;", "migrate", "./migrations" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.CommandInfo, Is.Not.Null);
        Assert.That(result.CommandInfo!.Action, Is.EqualTo("migrate"));
        Assert.That(result.CommandInfo!.Parameters["Directory"], Is.EqualTo("./migrations"));
        Assert.That(result.CommandInfo!.Parameters.ContainsKey("MigrationTableName"), Is.False);
    }

    [Test]
    public void ParseArguments_WithMigrateCommand_WithDirectoryAndTableName_ReturnsSuccess()
    {
        var result = _interactor.ParseArguments(new[] { "-c", "Server=localhost;", "migrate", "./migrations", "CustomMigrations" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.CommandInfo, Is.Not.Null);
        Assert.That(result.CommandInfo!.Action, Is.EqualTo("migrate"));
        Assert.That(result.CommandInfo!.Parameters["Directory"], Is.EqualTo("./migrations"));
        Assert.That(result.CommandInfo!.Parameters["MigrationTableName"], Is.EqualTo("CustomMigrations"));
    }

    [Test]
    public void ParseArguments_WithCaseInsensitiveAction_ReturnsSuccess()
    {
        var result = _interactor.ParseArguments(new[] { "-c", "Server=localhost;", "CLEARDB" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.CommandInfo, Is.Not.Null);
        Assert.That(result.CommandInfo!.Action, Is.EqualTo("CLEARDB"));
    }

    [Test]
    public void ParseArguments_WithInvalidAction_ReturnsFailure()
    {
        var result = _interactor.ParseArguments(new[] { "-c", "Server=localhost;", "invalidaction" });

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Action is required"));
        Assert.That(result.CommandInfo, Is.Null);
    }
}
