# Vervollständigung der Datenbankaktionen

## Kurzzusammenfassung

Die in R00001 implementierte Konsolenanwendung unterstützt bereits die grundlegenden Datenbankaktionen `cleardb`, `init` und `migrate` als Prototypen. Diese Implementierung soll nun vervollständigt werden, sodass die Aktionen tatsächlich mit SQL Server-Datenbanken interagieren und echte Datenbankoperationen durchführen.

## Was passiert aktuell

Aktuell sind die drei Hauptaktionen (`cleardb`, `init`, `migrate`) nur als Prototypen implementiert:

- **`cleardb`**: Gibt nur eine Erfolgsmeldung zurück, löscht aber keine Daten aus der Datenbank
- **`init [tablename]`**: Gibt nur eine Erfolgsmeldung zurück, erstellt aber keine Migrationstabelle
- **`migrate <directory> [tablename]`**: Prüft nur, ob das Verzeichnis existiert, führt aber keine SQL-Migrationen aus

Die Konsolenanwendung kann bereits Kommandozeilenparameter korrekt parsen, Verbindungsstrings verarbeiten und die entsprechenden Interactoren aufrufen. Das Logging-System funktioniert und alle Tests sind vorhanden.

## Was soll zukünftig passieren

Die drei Datenbankaktionen sollen mit echter SQL Server-Funktionalität implementiert werden:

### 1. `cleardb` - Datenbank vollständig leeren
- Alle benutzerdefinierten Tabellen aus der Datenbank entfernen
- Alle benutzerdefinierten Views, Stored Procedures, Functions löschen
- Migrationstabellen (alle Varianten) vollständig entfernen
- Systemtabellen und -objekte unberührt lassen
- Transaktionale Sicherheit: Bei Fehlern sollte die Datenbank im ursprünglichen Zustand bleiben

### 2. `init [tablename]` - Migrationssystem initialisieren
- Migrationstabelle erstellen (Standard: `_Migrations`, oder übergebener Name)
- Tabellenschema definieren mit folgenden Spalten:
  - `Id` (BIGINT IDENTITY, PRIMARY KEY)
  - `MigrationName` (NVARCHAR(255), NOT NULL, UNIQUE) - im Normalfall der Dateiname
  - `DiscoveredAt` (DATETIME, NOT NULL, DEFAULT GETDATE())
  - `ErrorMessage` VARCHAR(MAX) - Fehlermeldung, falls die Migration nicht gelaufen ist.
  - `AppliedAt` (DATETIME, NULL, DEFAULT GETDATE())
  - `Hash` (NVARCHAR(64), NULL) - für zukünftige Integritätsprüfung
- Prüfung, ob Migrationstabelle bereits existiert (Fehler vermeiden)
- Initiale "Baseline"-Migration eintragen, um den Start der Migrationshistorie zu markieren


### 3. `migrate <directory> [tablename]` - Migrationen ausführen
- Migrationsskripte aus dem angegebenen Verzeichnis laden
- Namenskonvention prüfen: `XXXXX-beschreibung.sql` (5-stellige Nummer + Beschreibung)
- Bereits ausgeführte Migrationen aus der Migrationstabelle ermitteln
- Neue Migrationen in der richtigen Reihenfolge ausführen
- Jede Migration in einer eigenen Transaktion ausführen
- Bei Fehlern: Rollback der aktuellen Migration, Stopp der Ausführung
- Erfolgreiche Migrationen in der Migrationstabelle vermerken
- Detailliertes Logging für jede ausgeführte Migration
- Bitte zweistufig, d.h. erst werden die Dateien erkannt und registriert und dann erst ausgeführt, so dass man einfach überprüfen kann, ob noch was offen ist, anhand der Zeitstempel.
- Die Ausführung der Migrationen soll anhand der alphanumerischen Sortierung der Dateien sichergestellt sein, d.h. so wie die Dateien im Normalfall in der Verzeichnisstruktur aufgelistet werden, genau so werden sie in der Reihenfolge ausgeführt.
- Der Migrationslauf findet nur bis zum ersten Fehler statt. Dann stoppt der Gesamtprozess.

## Implementationshinweise

### Datenbankzugriff
- Verwendung von `System.Data.SqlClient` oder `Microsoft.Data.SqlClient` für SQL Server-Zugriff
- Connection-String-Validierung vor Datenbankoperationen
- Robust Error-Handling für verschiedene Datenbankfehler (Verbindung, Berechtigung, Syntax)

### Migrationsskript-Format
- Unterstützung für SQL-Batch-Separator `GO`
- UTF-8-Encoding für Migrationsdateien
- Kommentare in Migrationsskripten zulassen
- Validierung der Migrationsskript-Syntax vor Ausführung

### Transaktionsstrategie
- Jede Migration in separater Transaktion für granulares Rollback
- `cleardb` als eine große Transaktion für Konsistenz
- Timeout-Konfiguration für langläufige Operationen

### Sicherheit
- SQL-Injection-Schutz durch parametrisierte Queries
- Validierung von Tabellen- und Verzeichnisnamen
- Berechtigung des Datenbankbenutzers prüfen (DDL-Rechte erforderlich)

### Erweiterte Funktionalität (optional für spätere Versionen)
- Dry-Run-Modus für `migrate` (nur anzeigen, was ausgeführt würde)
- Hash-Validierung für Migrationsskripte (Integritätsprüfung)
- Rollback-Funktionalität für Migrationen
- Parallele Ausführung von unabhängigen Migrationen

## Testanforderungen

### Unit-Tests
- Datenbankinteraktor-Tests mit Mock-Verbindungen
- SQL-Generierung und -Parsing testen
- Fehlerbehandlung für verschiedene Datenbankfehler-Szenarien

### Integration-Tests
- Tests mit echter SQL Server-Testdatenbank
- End-to-End-Tests für alle drei Aktionen
- Tests für Migrationsskript-Ausführung
- Rollback-Tests bei Migrationsfehler

### Test-Datenbank-Setup
- Automatisierte Erstellung von Testdatenbanken
- Cleanup nach Tests
- Isolierung von Tests (separate Datenbanken pro Test)

## Abhängigkeiten

- NuGet-Package für SQL Server-Zugriff (`Microsoft.Data.SqlClient`)
- Erweiterte Logging-Funktionalität für SQL-Operationen
- Konfigurationsmöglichkeiten für Timeouts und Retry-Strategien