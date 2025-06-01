# dbmig - Database Migration and Testing Tool

Ein leistungsstarkes Kommandozeilen-Tool für SQL Server Datenbank-Migrationen und -Tests mit Fokus auf Sicherheit, Nachvollziehbarkeit und Testbarkeit.

## Überblick

dbmig ist ein spezialisiertes Tool für die Verwaltung von SQL Server Datenbank-Migrationen und automatisierten Datenbank-Tests. Es bietet eine robuste, skriptbasierte Lösung für kontrollierte Schema-Änderungen, Datenmigrationen und umfassende Testszenarien in Enterprise-Umgebungen.

## Hauptfunktionen

### 🔄 **Datenbank-Migration**
- SQL-basierte Migrationsskripte mit Versionskontrolle
- Automatische Rollback-Funktionalität bei Fehlern
- Transaktionale Sicherheit für kritische Änderungen
- Detailliertes Migrations-Logging und Audit-Trail

### 🧪 **Datenbank-Testing**
- Automatisierte Schema-Validierung
- Performance-Tests für Migrationen
- Datenintegritäts-Prüfungen
- Testcontainer-basierte isolierte Testumgebungen

### 🔐 **Enterprise-Features**
- Multi-Umgebungs-Support (Dev, Test, Staging, Prod)
- Verschlüsselte Verbindungsstrings
- Rollback-Strategien und Recovery-Optionen
- Umfassende Fehlerbehandlung und Logging

## Verzeichnisstruktur

```
dbmig/
├── CLAUDE.md                   # KI-Entwicklungsrichtlinien
├── Initialize-AI.md            # Schnelle KI-Initialisierung
├── Dokumentation/              # Projektdokumentation
│   ├── Anforderungen/          # Feature-Anforderungen (R00001-*.md)
│   ├── Architektur/            # Architektur & Design
│   ├── Commands/               # Standard KI-Prompts
│   ├── Promptlog/              # Entwicklungs-Prompts (P00001-*.md)
│   └── Technologien/           # Technologie-Dokumentation
├── Scripts/                    # PowerShell Automatisierung
│   ├── Build.ps1              # Build-System
│   ├── Database-Initialize.ps1 # DB Initialisierung
│   ├── Database-Migrate.ps1   # Migration Runner
│   ├── Run-Tests.ps1          # Test Runner
│   └── Run-UITests.ps1        # UI-Tests (falls benötigt)
├── Source/                     # Quellcode
│   ├── Create-Solution.ps1    # Solution Generator
│   ├── Code/                  # dbmig Anwendungscode
│   └── DBMigrations/          # SQL Migrationsskripte
│       └── README.md          # Migrations-Dokumentation
├── Tests/                     # Test-Suite
│   ├── Datenbank/             # DB-spezifische Tests
│   │   └── README.md          # DB-Test Dokumentation
│   └── UI/                    # UI-Tests (falls benötigt)
│       └── README.md          # UI-Test Dokumentation
└── README.md                  # Diese Datei
```

## Installation

### Voraussetzungen
- .NET 8.0 SDK
- SQL Server 2019+ oder Azure SQL Database
- PowerShell 7.0+
- Optional: Docker für Testcontainer

### Setup
```powershell
# Repository klonen
git clone https://github.com/yourusername/dbmig.git
cd dbmig

# Solution erstellen und bauen
.\Source\Create-Solution.ps1 -SolutionName "dbmig" -ProjectType "Console"
.\Scripts\Build.ps1 -Configuration "Release"
```

## Verwendung

### Basis-Kommandos

```powershell
# Neue Migration erstellen
dbmig create-migration "AddUserTable"

# Migrationen ausführen
dbmig migrate --target latest
dbmig migrate --target 00005-AddIndexes

# Rollback durchführen
dbmig rollback --steps 1
dbmig rollback --target 00003-InitialSchema

# Status anzeigen
dbmig status
dbmig history --last 10
```

### Erweiterte Features

```powershell
# Dry-Run Modus
dbmig migrate --dry-run --verbose

# Multi-Umgebung
dbmig migrate --env production --config prod.json

# Batch-Migrationen
dbmig batch-migrate --from 00001 --to 00010 --pause 5

# Validierung
dbmig validate --schema --data --performance
```

## Konfiguration

### Basis-Konfiguration (dbmig.json)
```json
{
  "connectionStrings": {
    "default": "Server=localhost;Database=MyApp;Integrated Security=true;",
    "test": "Server=localhost;Database=MyApp_Test;Integrated Security=true;"
  },
  "migration": {
    "tableName": "_MigrationHistory",
    "schema": "dbo",
    "transactionMode": "PerMigration",
    "timeout": 300
  },
  "logging": {
    "level": "Information",
    "file": "logs/dbmig.log"
  }
}
```

## Migrations-Format

Migrationen folgen dem Format `00001-beschreibung.sql`:

```sql
-- Migration: 00001-CreateUserTable.sql
-- Author: dbmig
-- Date: 2025-01-06
-- Description: Creates the initial user table

-- UP Migration
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

GO

-- DOWN Migration
DROP TABLE IF EXISTS Users;
```

## Testing

```powershell
# Alle Tests ausführen
.\Scripts\Run-Tests.ps1 -All

# Nur Migrations-Tests
.\Scripts\Run-Tests.ps1 -Category "Migration"

# Performance-Tests
.\Scripts\Run-Tests.ps1 -Category "Performance" -Verbose

# Mit Testcontainern
.\Scripts\Run-Tests.ps1 -UseContainers -Coverage
```

## Sicherheit

- Verschlüsselte Verbindungsstrings in Konfiguration
- Audit-Logging für alle Migrationsaktivitäten
- Rollback-Funktionalität für kritische Fehler
- Berechtigungsprüfung vor Ausführung

## Entwicklung

### KI-gestützte Entwicklung
```bash
# Projekt für KI-Assistenten initialisieren
cat Initialize-AI.md

# Spezifische Dokumentation laden
cat ./Dokumentation/Anforderungen/README.md
cat ./Dokumentation/Architektur/ProgrammierPatterns.md
```

### Beitragen
1. Fork des Repositories
2. Feature-Branch erstellen (`git checkout -b feature/AmazingFeature`)
3. Änderungen committen (`git commit -m 'Add AmazingFeature'`)
4. Branch pushen (`git push origin feature/AmazingFeature`)
5. Pull Request erstellen

## Lizenz

Distributed under the MIT License. See `LICENSE` for more information.

---

**dbmig** - Enterprise-grade Database Migration and Testing Tool for SQL Server

## Entwicklungskosten (KI):
- 01.06.2025: $12.94, 3h:40min