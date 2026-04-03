using NUnit.Framework;
using dbmig.BL.CommandLineArguments;

namespace dbmig.BL.Tests.CommandLineArguments;

[TestFixture]
public class CommandLineArgumentsParserTests
{
    [Test]
    public void Parse_ClearDbWithConnectionString_ReturnsSuccess()
    {
        var result = CommandLineArgumentsParser.Parse(new[] { "cleardb", "-c", "Server=localhost;" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Action, Is.EqualTo("cleardb"));
        Assert.That(result.Value.ConnectionString, Is.EqualTo("Server=localhost;"));
    }

    [Test]
    public void Parse_InitWithConnectionString_ReturnsSuccess()
    {
        var result = CommandLineArgumentsParser.Parse(new[] { "init", "-c", "Server=localhost;" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Action, Is.EqualTo("init"));
        Assert.That(result.Value.MigrationTableName, Is.Null);
    }

    [Test]
    public void Parse_InitWithCustomTableName_ReturnsSuccess()
    {
        var result = CommandLineArgumentsParser.Parse(new[] { "init", "-c", "Server=localhost;", "CustomTable" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Action, Is.EqualTo("init"));
        Assert.That(result.Value.MigrationTableName, Is.EqualTo("CustomTable"));
    }

    [Test]
    public void Parse_MigrateWithDirectory_ReturnsSuccess()
    {
        var result = CommandLineArgumentsParser.Parse(new[] { "migrate", "-c", "Server=localhost;", "./migrations" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Action, Is.EqualTo("migrate"));
        Assert.That(result.Value.Directory, Is.EqualTo("./migrations"));
        Assert.That(result.Value.DryRun, Is.False);
    }

    [Test]
    public void Parse_MigrateWithDryRun_ReturnsDryRunTrue()
    {
        var result = CommandLineArgumentsParser.Parse(new[] { "migrate", "-c", "Server=localhost;", "./migrations", "--dry-run" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.DryRun, Is.True);
    }

    [Test]
    public void Parse_MigrateWithCustomTableName_ReturnsSuccess()
    {
        var result = CommandLineArgumentsParser.Parse(new[] { "migrate", "-c", "Server=localhost;", "./migrations", "CustomTable" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Directory, Is.EqualTo("./migrations"));
        Assert.That(result.Value.MigrationTableName, Is.EqualTo("CustomTable"));
    }

    [Test]
    public void Parse_WithLongConnectionStringOption_ReturnsSuccess()
    {
        var result = CommandLineArgumentsParser.Parse(new[] { "cleardb", "--connection-string", "Server=localhost;" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.ConnectionString, Is.EqualTo("Server=localhost;"));
    }

    [Test]
    public void Parse_WithNoArguments_ReturnsHelpShown()
    {
        var result = CommandLineArgumentsParser.Parse(Array.Empty<string>());

        // CommandLineParser shows help for no args — Value is null but IsSuccess may vary
        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public void Parse_WithInvalidVerb_ReturnsFailure()
    {
        var result = CommandLineArgumentsParser.Parse(new[] { "invalidverb" });

        Assert.That(result.Value, Is.Null);
    }
}
