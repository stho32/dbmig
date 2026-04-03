---
id: R00005
titel: "Connection-String korrekt an Repository weiterleiten"
typ: Bugfix
status: Umgesetzt
erstellt: 2026-04-03
---

# Connection-String korrekt an Repository weiterleiten

## Kurzzusammenfassung

Der ueber die Kommandozeile uebergebene Connection-String muss tatsaechlich fuer die Datenbankverbindung verwendet werden, statt eines hartkodierten Default-Werts.

## Was passiert aktuell

Der `DatabaseInteractor` erstellt im Konstruktor ein Default-Repository mit `ConnectionStrings.Default` (fest kodiert auf `dbmig_default`). Die Methoden `ClearDatabase`, `InitializeMigration` und `RunMigrations` empfangen einen Connection-String als Parameter, ignorieren ihn aber — alle Operationen laufen immer gegen `dbmig_default`.

## Was soll zukuenftig passieren

Jede Methode des `DatabaseInteractor` erstellt ein Repository mit dem uebergebenen Connection-String:
- Produktions-Konstruktor ohne Parameter: erstellt bei jedem Methodenaufruf ein neues Repository mit dem uebergebenen Connection-String
- Test-Konstruktor mit `IMigrationRepository`: nutzt immer das injizierte Mock-Repository
- `GetRepository(connectionString)` als zentrale Factory-Methode

## Implementationshinweise

- `DatabaseInteractor` speichert optional ein injiziertes Repository (`_injectedRepository`)
- `GetRepository(string connectionString)` gibt entweder das injizierte Repository oder ein neues `MigrationRepository(new SqlServerDatabaseAccessor(connectionString))` zurueck
- `InteractorFactory.GetDatabaseInteractor()` ruft den parameterlosen Konstruktor auf
