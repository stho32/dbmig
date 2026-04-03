namespace dbmig.BL.Logging;

public static class LoggerFactory
{
    public static ILogger Get()
    {
        return new ConsoleLogger();
    }
}