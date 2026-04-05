---
id: R00010
titel: "Bug-Behebung: ClearDB generiert DROP TABLE SQL ohne Tabellennamen"
typ: Bug-Behebung
status: Umgesetzt
erstellt: 2026-04-05
ursprung: GitHub Issue #1
---

# Bug-Behebung: ClearDB generiert DROP TABLE SQL ohne Tabellennamen

## Kurzzusammenfassung

Bei Ausfuehrung von `cleardb` bleiben alle Tabellen in der Datenbank zurueck, weil das generierte DROP-SQL nur den Schema-Namen enthaelt (`DROP TABLE IF EXISTS [dbo]`), nicht aber den Tabellennamen (`DROP TABLE IF EXISTS [dbo].[TableName]`).

## Fehlerbeschreibung

GitHub Issue: stho32/dbmig#1

Beim Aufruf von `dbmig -c "..." cleardb` werden die Tabellen nicht geloescht. Das Debug-Log zeigt:
```
Executing command: DROP TABLE IF EXISTS [dbo]
Dropped table: dbo
```

Statt:
```
Executing command: DROP TABLE IF EXISTS [dbo].[MeineTabelle]
Dropped table: dbo.MeineTabelle
```

## Ursachenanalyse

### Root Cause

Der originale Code in `ClearAllUserTablesAsync()` verwendete `QueryAsync<string>` fuer eine Query die zwei Spalten zurueckliefert (`TABLE_SCHEMA, TABLE_NAME`):

```csharp
// BUGGY (Commit 8baa8fe):
var tables = await _databaseAccessor.QueryAsync<string>(getTablesSQL);
foreach (var table in tables)
{
    var dropSQL = $"DROP TABLE IF EXISTS [{table}]";
```

`QueryAsync<string>` liest nur die erste Spalte (`TABLE_SCHEMA = "dbo"`). Der Tabellenname aus der zweiten Spalte wird komplett ignoriert.

### Betroffene Komponenten

- `MigrationRepository.ClearAllUserTablesAsync()` — Primaerer Bug-Ort
- `SqlServerDatabaseAccessor.QueryAsync<T>()` — Verhalten bei `QueryAsync<string>` (liest nur erste Spalte)
- `MigrationRepositoryTests` — Tests pruefen nicht ob Tabellennamen im SQL enthalten sind

## Aktueller Stand

Der Produktionscode wurde bereits korrigiert (Commit `502e1d9`). Die aktuelle Version verwendet `QueryAsync<dynamic>` und greift korrekt auf beide Spalten zu:

```csharp
// AKTUELL (gefixt):
var tables = await _databaseAccessor.QueryAsync<dynamic>(getTablesSQL);
foreach (var table in tables)
{
    var dropTableSQL = $"DROP TABLE IF EXISTS [{table.TABLE_SCHEMA}].[{table.TABLE_NAME}]";
```

**Problem:** Die Unit-Tests pruefen NICHT ob die Tabellennamen im generierten SQL enthalten sind. Der Test prueft nur:
```csharp
Assert.That(executeCalls.All(x => x.Contains("DROP TABLE")), Is.True);
```

Damit koennte der Bug unbemerkt zurueckkehren (Regression).

## Akzeptanzkriterien

- [ ] Unit-Test existiert der explizit prueft dass Tabellennamen im DROP-SQL enthalten sind
- [ ] Unit-Test existiert der explizit prueft dass Schema UND Tabellenname im Format `[schema].[name]` erscheinen
- [ ] Alle bestehenden Tests bleiben gruen
- [ ] GitHub Issue #1 ist geschlossen

## Implementierungshinweise

Der Fix-Code existiert bereits. Die Arbeit besteht darin, Regressionstests zu schreiben die:

1. Pruefen dass `ExecuteAsyncCalls` SQL-Statements der Form `DROP TABLE IF EXISTS [dbo].[Table1]` enthalten
2. Sicherstellen dass fuer JEDE gefundene Tabelle ein korrektes DROP-Statement generiert wird
3. Optional: Aehnliches Muster bei Views, Procedures, Functions pruefen

## Notizen

- Prioritaet: Hoch (Regressions-Absicherung fuer bereits behobenen Bug)
- Workaround: Keiner noetig, Code ist bereits gefixt
