# Datenbank-Migrationen

Diese Dokumentation beschreibt das Migrationssystem für Datenbankänderungen in diesem Projekt.

## Übersicht

Das Projekt verwendet ein dateibasiertes Migrationssystem mit SQL-Skripten, die in nummerierter Reihenfolge ausgeführt werden. Dies gewährleistet eine konsistente und nachvollziehbare Datenbankentwicklung.

## Namenskonvention

Alle Migrationsdateien folgen dem Format:
```
NNNNN-kurze-inhaltsangabe.sql
```

**Beispiele:**
- `00001-initial-database-schema.sql`
- `00002-add-users-table.sql`
- `00003-add-user-email-index.sql`
- `00004-alter-users-add-created-date.sql`

### Nummerierung

- **5-stellige Zahlen**: Führende Nullen für einheitliche Sortierung
- **Sequenziell**: Jede neue Migration erhält die nächste verfügbare Nummer
- **Keine Lücken**: Migrationen werden in exakter Reihenfolge ausgeführt

### Inhaltsbeschreibung

- **Kebab-Case**: Kleinbuchstaben mit Bindestrichen
- **Beschreibend**: Kurz aber aussagekräftig
- **Englisch**: Einheitliche Sprache für technische Bezeichnungen

## Dateistruktur

```
Source/DBMigrations/
├── README.md                           # Diese Dokumentation
├── 00001-initial-database-schema.sql   # Erste Migration
├── 00002-add-users-table.sql          # Zweite Migration
├── 00003-add-user-email-index.sql     # Dritte Migration
└── ...
```

## Migration erstellen

### 1. Nummer bestimmen
```bash
# Nächste verfügbare Nummer finden
ls Source/DBMigrations/*.sql | tail -1
# Beispielausgabe: 00003-add-user-email-index.sql
# Nächste Nummer: 00004
```

### 2. Datei erstellen
```bash
# Neue Migrationsdatei erstellen
touch Source/DBMigrations/00004-alter-users-add-created-date.sql
```

### 3. SQL-Inhalt schreiben

#### Beispiel: Tabelle erstellen
```sql
-- Migration: 00002-add-users-table.sql
-- Beschreibung: Erstellt die Users-Tabelle mit grundlegenden Feldern

CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Email NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Eindeutiger Index für E-Mail
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
```

#### Beispiel: Spalte hinzufügen
```sql
-- Migration: 00004-alter-users-add-created-date.sql
-- Beschreibung: Fügt CreatedDate Spalte zur Users-Tabelle hinzu

ALTER TABLE Users
ADD CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE();

-- Index für bessere Performance bei Datums-Abfragen
CREATE INDEX IX_Users_CreatedDate ON Users(CreatedDate);
```

#### Beispiel: Index erstellen
```sql
-- Migration: 00003-add-user-email-index.sql
-- Beschreibung: Erstellt Index für E-Mail-Suche

CREATE INDEX IX_Users_Email_Search ON Users(Email) 
WHERE IsActive = 1;
```

## Best Practices

### SQL-Entwicklung

1. **Idempotenz**: Migrationen sollen mehrfach ausführbar sein
   ```sql
   -- Prüfung vor Erstellung
   IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
   BEGIN
       CREATE TABLE Users (...);
   END
   ```

2. **Transaktionen**: Komplexe Migrationen in Transaktionen kapseln
   ```sql
   BEGIN TRANSACTION;
   
   -- Migration Code hier
   
   COMMIT TRANSACTION;
   ```

3. **Rollback-Fähigkeit**: Dokumentiere mögliche Rollback-Schritte
   ```sql
   -- ROLLBACK: DROP TABLE Users;
   ```

4. **Performance**: Große Datenmengen in Batches verarbeiten
   ```sql
   -- Für große Tabellen
   UPDATE Users 
   SET IsActive = 1 
   WHERE Id IN (SELECT TOP 1000 Id FROM Users WHERE IsActive IS NULL);
   ```

### Datensicherheit

1. **Backups**: Vor kritischen Migrationen Backup erstellen
2. **Testing**: Migrationen zuerst in Entwicklungsumgebung testen
3. **Dokumentation**: Zweck und Auswirkungen dokumentieren

### Versionskontrolle

1. **Atomare Commits**: Eine Migration pro Commit
2. **Aussagekräftige Commit-Messages**: 
   ```
   feat: add users table migration (00002)
   ```
3. **Nie ändern**: Bereits angewendete Migrationen niemals ändern

## Migration ausführen

### Manuell (Entwicklung)
```sql
-- In SQL Server Management Studio oder ähnlichem Tool
-- Migrationen in nummerischer Reihenfolge ausführen
```

### Automatisiert (PowerShell)
```powershell
# Verwende das bereitgestellte Migrations-Script
.\Scripts\Database-Migrate.ps1 -TargetMigration "00004"
```

### Anwendungsstart
```csharp
// In Program.cs oder Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Migration Runner konfigurieren
    services.AddDbMigrationRunner();
}

public void Configure(IApplicationBuilder app)
{
    // Migrationen beim Anwendungsstart ausführen
    app.RunDatabaseMigrations();
}
```

## Troubleshooting

### Häufige Probleme

1. **Migration bereits angewendet**
   - Prüfe Migrationsstatus in der Datenbank
   - Überspringe bereits angewendete Migrationen

2. **SQL-Syntaxfehler**
   - Teste Migration in Entwicklungsumgebung
   - Prüfe Datenbankkompatibilität

3. **Deadlocks**
   - Führe Migrationen außerhalb der Hauptgeschäftszeiten aus
   - Verwende kürzere Transaktionen

4. **Rollback erforderlich**
   - Dokumentiere Rollback-Schritte in Migration
   - Erstelle neue Migration für Rollback

### Logging

Alle Migrationen sollten geloggt werden:
```sql
-- Am Ende jeder Migration
PRINT 'Migration 00004-alter-users-add-created-date.sql completed successfully';
```

## Tools und Hilfsmittel

### PowerShell Scripts
- `Scripts/Database-Initialize.ps1` - Datenbank initialisieren
- `Scripts/Database-Migrate.ps1` - Migrationen ausführen

### SQL Tools
- SQL Server Management Studio (SSMS)
- Azure Data Studio
- Visual Studio SQL Server Data Tools

### Version Control
- Git für Versionskontrolle der Migrationsdateien
- Branching-Strategie für Datenbankänderungen

## Beispiel-Workflow

1. **Feature-Branch erstellen**
   ```bash
   git checkout -b feature/user-management
   ```

2. **Migration erstellen**
   ```bash
   touch Source/DBMigrations/00005-add-user-roles.sql
   ```

3. **SQL entwickeln und testen**
   ```sql
   -- Entwicklung und Test in lokaler DB
   ```

4. **Migration committen**
   ```bash
   git add Source/DBMigrations/00005-add-user-roles.sql
   git commit -m "feat: add user roles migration (00005)"
   ```

5. **Integration**
   ```bash
   git push origin feature/user-management
   # Pull Request erstellen
   ```

6. **Deployment**
   ```bash
   # Nach Merge: Migration in Zielumgebung ausführen
   .\Scripts\Database-Migrate.ps1 -TargetMigration "00005"
   ```

## Umgebungen

### Entwicklung
- Lokale Datenbank (LocalDB oder Docker)
- Schnelle Iteration und Testing
- Experimentelle Migrationen erlaubt

### Staging
- Produktionsähnliche Datenbank
- Vollständige Migrationstests
- Performance-Validierung

### Produktion
- Kritische Umgebung
- Nur getestete Migrationen
- Backup vor jeder Migration
- Überwachung und Rollback-Plan