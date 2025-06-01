# Datenbank-Setup für dbmig

## Erforderliche Datenbanken

Für das dbmig-Projekt müssen Sie die folgenden SQL Server-Datenbanken manuell anlegen:

### 1. Integration Tests
```sql
CREATE DATABASE dbmig_integration_test;
```

### 2. Unit Tests (für Verbindungstests)
```sql
CREATE DATABASE dbmig_unit_test;
```

### 3. Produktive Standarddatenbank
```sql
CREATE DATABASE dbmig_default;
```

## SQL Server Setup

1. **SQL Server installieren:**
   - SQL Server Express (kostenlos) oder eine andere SQL Server Edition
   - SQL Server Management Studio (SSMS) für die Verwaltung

2. **Datenbanken anlegen:**
   ```sql
   -- Alle drei Datenbanken auf einmal anlegen
   CREATE DATABASE dbmig_integration_test;
   CREATE DATABASE dbmig_unit_test;
   CREATE DATABASE dbmig_default;
   ```

3. **Berechtigungen prüfen:**
   - Stellen Sie sicher, dass Ihr Windows-Benutzer `db_owner`-Berechtigung für alle drei Datenbanken hat
   - Bei Integrated Security (Windows Authentication) sollte dies automatisch funktionieren

## Connection String-Konfiguration

Die Connection Strings sind zentral in `dbmig.BL/Configuration/ConnectionStrings.cs` konfiguriert:

- **Server:** `localhost` (Standard)
- **Authentication:** `Integrated Security=true` (Windows Authentication)
- **SSL:** `TrustServerCertificate=true` (für lokale Entwicklung)

## Verwendung in Tests

- **Integration Tests:** Verwenden `ConnectionStrings.IntegrationTest`
- **Unit Tests:** Verwenden `ConnectionStrings.UnitTest`  
- **Production Code:** Verwendet `ConnectionStrings.Default`

## Anpassung der Konfiguration

Falls Sie andere Server-Namen oder Authentifizierungsmethoden verwenden möchten, bearbeiten Sie die `ConnectionStrings.cs` Datei:

```csharp
// Beispiel für SQL Server Authentication
public static string Custom(string server = "localhost", string database = "dbmig_default", 
                           string userId = "", string password = "",
                           bool integratedSecurity = false, bool trustServerCertificate = true)
{
    if (integratedSecurity)
        return $"Server={server};Database={database};Integrated Security=true;TrustServerCertificate={trustServerCertificate};";
    else
        return $"Server={server};Database={database};User Id={userId};Password={password};TrustServerCertificate={trustServerCertificate};";
}
```

## Tests ausführen

Nach dem Anlegen der Datenbanken können Sie die Tests ausführen:

```bash
# Nur Unit Tests (funktionieren mit Mock-Objekten)
dotnet test dbmig.BL.Tests

# Integration Tests (benötigen echte Datenbank)
dotnet test dbmig.BL.IntegrationTests

# Alle Tests
dotnet test
```

## Troubleshooting

**Problem:** "Server was not found or was not accessible"
- **Lösung:** Prüfen Sie, ob SQL Server läuft und die Datenbanken existieren

**Problem:** "Login failed"
- **Lösung:** Prüfen Sie die Berechtigungen Ihres Windows-Benutzers

**Problem:** "Certificate chain validation failed"
- **Lösung:** `TrustServerCertificate=true` ist bereits in den Connection Strings konfiguriert