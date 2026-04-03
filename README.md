# dbmig - Database Migration Tool for SQL Server

Ein Kommandozeilen-Tool fuer SQL Server Datenbank-Migrationen mit Fokus auf Sicherheit, Nachvollziehbarkeit und Testbarkeit.

## Ueberblick

dbmig verwaltet SQL Server Datenbank-Migrationen ueber versionierte SQL-Skripte. Es bietet eine robuste, skriptbasierte Loesung fuer kontrollierte Schema-Aenderungen mit transaktionaler Sicherheit und detailliertem Logging.

## Hauptfunktionen

- **SQL-basierte Migrationen** mit 5-stelliger Nummerierung und automatischer Reihenfolge
- **Zweistufiger Prozess**: Erst Registrierung (Discovery), dann Ausfuehrung
- **Transaktionale Sicherheit**: Jede Migration in eigener Transaktion, Rollback bei Fehlern
- **GO-Batch-Separator**: Unterstuetzung fuer SQL Server `GO`-Statements
- **Fehler-Stopp**: Migrationsablauf stoppt beim ersten Fehler (Exit-Code 1)
- **Datenbank bereinigen**: Alle Benutzerobjekte sicher entfernen (FK-Constraints, SPs, Functions, Views, Tabellen)

## Voraussetzungen

- .NET 8.0 SDK
- SQL Server 2019+ oder Azure SQL Database
- PowerShell 7.0+ (optional, fuer Scripts)
- Docker (optional, fuer Testcontainer)

## Installation & Build

```powershell
git clone https://github.com/stho32/dbmig.git
cd dbmig
dotnet build Source/Code/dbmig.sln --configuration Release
```

## Verwendung

### Kommandozeile

```bash
# Hilfe anzeigen
dbmig --help

# Migrationssystem initialisieren (erstellt _Migrations-Tabelle)
dbmig -c "Server=localhost;Database=MyDb;Integrated Security=true;TrustServerCertificate=true;" init

# Mit benutzerdefiniertem Tabellennamen
dbmig -c "Server=localhost;Database=MyDb;Integrated Security=true;TrustServerCertificate=true;" init CustomMigrations

# Migrationen ausfuehren
dbmig -c "Server=localhost;Database=MyDb;Integrated Security=true;TrustServerCertificate=true;" migrate ./Source/DBMigrations

# Datenbank komplett leeren (alle Benutzerobjekte)
dbmig -c "Server=localhost;Database=MyDb;Integrated Security=true;TrustServerCertificate=true;" cleardb
```

### PowerShell-Scripts

```powershell
# Build
.\Scripts\Build.ps1 -Configuration Release

# Migrationssystem initialisieren
.\Scripts\Database-Initialize.ps1 -ConnectionString "Server=localhost;..." -Initialize

# Datenbank leeren
.\Scripts\Database-Initialize.ps1 -ConnectionString "Server=localhost;..." -Clear -Force

# Migrationen ausfuehren
.\Scripts\Database-Migrate.ps1 -ConnectionString "Server=localhost;..." -MigrationDirectory "./Source/DBMigrations"

# Tests ausfuehren
.\Scripts\Run-Tests.ps1 -All
.\Scripts\Run-Tests.ps1 -UnitTests
.\Scripts\Run-Tests.ps1 -IntegrationTests
```

## Migrations-Format

Migrationen folgen dem Format `XXXXX-beschreibung.sql` (5-stellige Nummer):

```sql
-- Migration: 00001-CreateUserTable.sql
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

GO

-- Weitere Batches nach GO
CREATE INDEX IX_Users_Username ON Users (Username);
```

Migrationen werden im Verzeichnis `Source/DBMigrations/` abgelegt.

## Entwicklung

### Lokales Setup mit Docker

```bash
# SQL Server Container starten
docker compose up -d

# Warten bis SQL Server bereit ist, dann Tests ausfuehren
.\Scripts\Run-Tests.ps1 -All
```

### Lokales Setup ohne Docker

1. SQL Server lokal installieren
2. Datenbanken anlegen:
   ```sql
   CREATE DATABASE dbmig_default;
   CREATE DATABASE dbmig_unit_test;
   CREATE DATABASE dbmig_integration_test;
   ```
3. Tests ausfuehren: `.\Scripts\Run-Tests.ps1 -All`

### Projektstruktur

```
Source/Code/
  dbmig.Console/          -- CLI-Einstiegspunkt
  dbmig.BL/               -- Business-Logik (Interactoren, Repository, DB-Zugriff)
  dbmig.BL.Tests/         -- Unit-Tests (NUnit)
  dbmig.BL.IntegrationTests/ -- Integration-Tests mit echtem SQL Server
```

### Architektur

- **Interactor-Pattern**: Business-Logik in `DatabaseInteractor` und `CommandInterpreterInteractor`
- **Repository-Pattern**: `MigrationRepository` kapselt alle DB-Operationen
- **Result-Types**: Alle Operationen geben `Result(IsSuccess, Message)` zurueck
- **Layered**: Console -> BL (keine Rueckwaerts-Abhaengigkeiten)

## Geplante Features

- Dry-Run-Modus fuer Migrationen
- Hash-Validierung fuer Migrationsskripte (Integritaetspruefung)
- Rollback-Funktionalitaet
- Status-/History-Abfrage
- Multi-Umgebungs-Konfiguration

## Lizenz

MIT License. Siehe [LICENSE](LICENSE).

---

**Entwicklungskosten (KI):**
- 01.06.2025: $12.94, 3h:40min
