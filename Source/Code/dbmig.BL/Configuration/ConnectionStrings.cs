namespace dbmig.BL.Configuration;

/// <summary>
/// Zentrale Konfiguration für Datenbankverbindungsstrings
/// </summary>
public static class ConnectionStrings
{
    /// <summary>
    /// Standard SQL Server Connection String Template
    /// Verwendet localhost für Windows und Host-IP für WSL
    /// </summary>
    private static string StandardTemplate => IsRunningInWSL() 
        ? "Server=172.17.32.1;Database={0};Integrated Security=true;TrustServerCertificate=true;"
        : "Server=localhost;Database={0};Integrated Security=true;TrustServerCertificate=true;";

    /// <summary>
    /// Datenbank für Integration Tests
    /// Sie müssen diese Datenbank manuell anlegen: CREATE DATABASE dbmig_integration_test;
    /// </summary>
    public static string IntegrationTest => string.Format(StandardTemplate, "dbmig_integration_test");

    /// <summary>
    /// Datenbank für Unit Tests (wird für Verbindungstests verwendet)
    /// Sie müssen diese Datenbank manuell anlegen: CREATE DATABASE dbmig_unit_test;
    /// </summary>
    public static string UnitTest => string.Format(StandardTemplate, "dbmig_unit_test");

    /// <summary>
    /// Default-Datenbank für Produktionsumgebung
    /// Sie müssen diese Datenbank manuell anlegen: CREATE DATABASE dbmig_default;
    /// </summary>
    public static string Default => string.Format(StandardTemplate, "dbmig_default");

    /// <summary>
    /// Ungültiger Connection String für Fehlertests
    /// </summary>
    public static string Invalid => string.Format(StandardTemplate, "nonexistent_database_for_testing");

    /// <summary>
    /// Erstellt einen Connection String für eine spezifische Datenbank
    /// </summary>
    /// <param name="databaseName">Name der Datenbank</param>
    /// <returns>Vollständiger Connection String</returns>
    public static string ForDatabase(string databaseName)
    {
        return string.Format(StandardTemplate, databaseName);
    }

    /// <summary>
    /// Erstellt einen Connection String mit benutzerdefinierten Parametern
    /// </summary>
    /// <param name="server">Server-Name (Standard: localhost)</param>
    /// <param name="database">Datenbank-Name</param>
    /// <param name="integratedSecurity">Integrated Security verwenden (Standard: true)</param>
    /// <param name="trustServerCertificate">Server-Zertifikat vertrauen (Standard: true)</param>
    /// <returns>Vollständiger Connection String</returns>
    public static string Custom(string server = "localhost", string database = "dbmig_default", 
                               bool integratedSecurity = true, bool trustServerCertificate = true)
    {
        return $"Server={server};Database={database};Integrated Security={integratedSecurity};TrustServerCertificate={trustServerCertificate};";
    }

    /// <summary>
    /// Prüft, ob die Anwendung in einer WSL-Umgebung läuft
    /// </summary>
    /// <returns>True wenn WSL, False wenn Windows</returns>
    private static bool IsRunningInWSL()
    {
        try
        {
            // WSL hat typischerweise /proc/version mit "Microsoft" oder "WSL"
            if (File.Exists("/proc/version"))
            {
                var version = File.ReadAllText("/proc/version");
                return version.Contains("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                       version.Contains("WSL", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}