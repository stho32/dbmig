namespace dbmig.BL.Logging;

public class ConsoleLogger : ILogger
{
    public void LogInfo(string message)
    {
        System.Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    public void LogWarning(string message)
    {
        System.Console.WriteLine($"[WARN] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    public void LogError(string message, Exception? exception = null)
    {
        System.Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        if (exception != null)
        {
            System.Console.WriteLine($"[ERROR] Exception: {exception.GetType().Name}: {exception.Message}");
            System.Console.WriteLine($"[ERROR] StackTrace: {exception.StackTrace}");
        }
    }

    public void LogDebug(string message)
    {
        System.Console.WriteLine($"[DEBUG] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
    }
}