---
id: R00004
titel: "SQL-Injection-Schutz fuer Tabellennamen"
typ: Sicherheit
status: Umgesetzt
erstellt: 2026-04-03
---

# SQL-Injection-Schutz fuer Tabellennamen

## Kurzzusammenfassung

Alle Tabellennamen, die ueber CLI-Parameter an das Migrationssystem uebergeben werden, muessen vor der Verwendung in SQL-Queries validiert werden, um SQL-Injection-Angriffe zu verhindern.

## Was passiert aktuell

Tabellennamen werden per String-Interpolation in SQL-Queries eingefuegt (z.B. `$"[{tableName}]"`). Ein Angreifer koennte ueber die CLI-Parameter `init` oder `migrate` manipulierte Tabellennamen uebergeben, die SQL-Injection ermoeglichen.

## Was soll zukuenftig passieren

Eine zentrale Validierungsmethode (`ValidationHelper.IsValidTableName`) prueft alle Tabellennamen per Regex: nur alphanumerische Zeichen und Unterstriche, beginnend mit Buchstabe oder Unterstrich, max. 128 Zeichen (SQL Server Identifier-Limit). Die Validierung wird zu Beginn jeder Repository-Methode aufgerufen.

## Implementationshinweise

- `ValidationHelper.IsValidTableName()` in `dbmig.BL.Common` mit Regex `^[a-zA-Z_][a-zA-Z0-9_]{0,127}$`
- `MigrationRepository.ValidateTableName()` als private Methode, aufgerufen in allen Methoden die tableName nutzen
- Bei ungueltigem Namen: `ArgumentException` werfen
- Unit-Tests fuer Injection-Versuche
