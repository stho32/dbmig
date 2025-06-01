# Konsolenanwendung mit ConnectionString und Aktionsparametern

## Kurzzusammenfassung

Die zu schreibende Anwendung ist eine Hauptanwendung, die als Einstiegspunkt für verschiedene Datenbankmigrationsaufgaben dient. Sie unterstützt verschiedene Aktionen über Kommandozeilenparameter und soll leicht erweiterbar sein.

## Was passiert aktuell

Aktuell existiert noch keine Konsolenanwendung zur Durchführung von Datenbankmigrationen. Es gibt keine standardisierte Methode, um Datenbankstrukturen zu löschen, zu initialisieren oder Migrationen durchzuführen. Diese Aufgaben müssen manuell oder mit separaten Skripten durchgeführt werden, was fehleranfällig und zeitaufwändig ist.

Projekt ist vorbereitet bei Source\Code\dbmig.Console

## Was soll zukünftig passieren

Zukünftig soll eine flexible Konsolenanwendung existieren, die als zentraler Einstiegspunkt für alle Datenbankmigrationsaufgaben dient. Die Anwendung soll folgende Funktionalitäten bieten:

1. Einen gemeinsamen Parameter für den Datenbankverbindungsstring (`--connection-string` oder `-c`)

2. Unterstützung verschiedener Aktionen (Subcommands) wie:
   - `cleardb`: Alle Datentabellen und Strukturen aus der Datenbank entfernen, inklusive aller Migrationstabellen
   - `init [<Migrationstabellenname>]`: Grundlegende Tabellen für Migration erstellen und dann beenden
   - `migrate <Verzeichnis> [<Migrationstabellenname>]`: Migrationen aus dem angegebenen Verzeichnis über die angegebene Migrationstabelle ausführen
   - Weitere Aktionen in der Zukunft

3. Eine erweiterbare Architektur, die das einfache Hinzufügen neuer Subcommands ermöglicht, ohne dass bestehende Commands angepasst werden müssen

4. Einheitliche Fehlerbehandlung und Logging

5. Hilfe- und Informationsausgabe bei fehlenden oder falschen Parametern

## Implementationshinweise

1. Die Anwendung soll gemäß der Interaktor-Architektur implementiert werden. Jeder Subcommand soll über einen eigenen Interaktor verfügen, der die entsprechende Geschäftslogik kapselt.

2. Es soll ein CommandInterpreter-Interaktor erstellt werden, der in seinem Result eine Datenstruktur zurückgibt, über die die Konsolenanwendung entscheiden kann, welche anderen Interaktoren mit welchen Parametern sie ausführen soll.

3. Der CommandInterpreter sollte eine Art Command-Factory und Command-Registry implementieren, die es ermöglicht, neue Commands zu registrieren, ohne bestehenden Code zu ändern (Open-Closed-Prinzip).

4. Alle Geschäftslogik soll in der dbmig.BL-Bibliothek implementiert werden, während die Konsolenanwendung (dbmig.Console) nur für die Benutzerinteraktion verantwortlich ist.

5. Die Fehlerbehandlung soll einheitlich über die Result-Objekte der Interaktoren erfolgen, wie in der Architektur-Dokumentation beschrieben.

6. Unit- und Integrationstests sollten für alle Komponenten erstellt werden, mit besonderem Fokus auf die Interaktoren, für die eine 100%-Testabdeckung angestrebt wird.
