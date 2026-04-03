---
id: R00006
titel: "Migrationen tatsaechlich ausfuehren (nicht nur Discovery)"
typ: Feature
status: Umgesetzt
erstellt: 2026-04-03
---

# Migrationen tatsaechlich ausfuehren

## Kurzzusammenfassung

Die `migrate`-Aktion soll Migrationsdateien nicht nur erkennen (Discovery), sondern auch tatsaechlich gegen die Datenbank ausfuehren.

## Was passiert aktuell

`DatabaseInteractor.RunMigrations()` liest Migrationsdateien aus dem Verzeichnis und loggt sie, fuehrt aber kein SQL aus. Der Code enthaelt einen `TODO`-Kommentar: "Implement actual execution phase".

## Was soll zukuenftig passieren

Zweistufiger Migrationsprozess:

### Phase 1: Discovery
- Migrationsdateien aus Verzeichnis laden (Pattern: `XXXXX-beschreibung.sql`)
- Alphanumerisch sortieren
- Gegen Migrationstabelle abgleichen: neue, fehlgeschlagene und bereits angewandte Migrationen identifizieren
- Neue Migrationen in Tabelle registrieren (DiscoveredAt-Zeitstempel)

### Phase 2: Execution
- Nur neue/ausstehende Migrationen ausfuehren
- Jede Migration in eigener Transaktion (via `ExecuteInTransactionAsync`)
- GO-Batch-Separator unterstuetzen
- Bei Erfolg: `AppliedAt` setzen
- Bei Fehler: `ErrorMessage` setzen, Migrationsablauf sofort stoppen
- Exit-Code 1 bei Fehler (wird ueber `Result.IsSuccess = false` propagiert)

## Implementationshinweise

- `IMigrationRepository.ExecuteMigrationSqlAsync(string sql)` fuer transaktionale SQL-Ausfuehrung
- GO-Splitting per Regex in `MigrationRepository`
- `DatabaseInteractor` orchestriert Discovery und Execution
- Ausfuehrung stoppt beim ersten Fehler
