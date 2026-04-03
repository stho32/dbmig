using NUnit.Framework;
using dbmig.BL.Logging;
using System.IO;

namespace dbmig.BL.Tests.Logging;

[TestFixture]
public class LoggerTests
{
    private StringWriter _consoleOutput = null!;
    private ConsoleLogger _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _consoleOutput = new StringWriter();
        System.Console.SetOut(_consoleOutput);
        _logger = new ConsoleLogger();
    }

    [TearDown]
    public void TearDown()
    {
        _consoleOutput.Dispose();
        System.Console.SetOut(new StreamWriter(System.Console.OpenStandardOutput()));
    }

    [Test]
    public void LogInfo_WritesToConsole()
    {
        _logger.LogInfo("Test info message");

        var output = _consoleOutput.ToString();
        Assert.That(output, Does.Contain("[INFO]"));
        Assert.That(output, Does.Contain("Test info message"));
    }

    [Test]
    public void LogWarning_WritesToConsole()
    {
        _logger.LogWarning("Test warning message");

        var output = _consoleOutput.ToString();
        Assert.That(output, Does.Contain("[WARN]"));
        Assert.That(output, Does.Contain("Test warning message"));
    }

    [Test]
    public void LogError_WithoutException_WritesToConsole()
    {
        _logger.LogError("Test error message");

        var output = _consoleOutput.ToString();
        Assert.That(output, Does.Contain("[ERROR]"));
        Assert.That(output, Does.Contain("Test error message"));
    }

    [Test]
    public void LogError_WithException_WritesExceptionDetails()
    {
        var exception = new InvalidOperationException("Test exception");
        _logger.LogError("Test error message", exception);

        var output = _consoleOutput.ToString();
        Assert.That(output, Does.Contain("[ERROR]"));
        Assert.That(output, Does.Contain("Test error message"));
        Assert.That(output, Does.Contain("InvalidOperationException"));
        Assert.That(output, Does.Contain("Test exception"));
    }

    [Test]
    public void LogDebug_WritesToConsole()
    {
        _logger.LogDebug("Test debug message");

        var output = _consoleOutput.ToString();
        Assert.That(output, Does.Contain("[DEBUG]"));
        Assert.That(output, Does.Contain("Test debug message"));
    }
}

[TestFixture]
public class LoggerFactoryTests
{
    [Test]
    public void Get_ReturnsConsoleLoggerInstance()
    {
        var logger = LoggerFactory.Get();

        Assert.That(logger, Is.Not.Null);
        Assert.That(logger, Is.InstanceOf<ConsoleLogger>());
    }

    [Test]
    public void Get_ReturnsNewInstanceEachTime()
    {
        var logger1 = LoggerFactory.Get();
        var logger2 = LoggerFactory.Get();

        Assert.That(logger1, Is.Not.SameAs(logger2));
    }
}