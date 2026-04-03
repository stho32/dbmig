---
id: R00009
titel: "Entwickler-Tooling und Projekt-Standards"
typ: Infrastruktur
status: Umgesetzt
erstellt: 2026-04-03
---

# Entwickler-Tooling und Projekt-Standards

## Kurzzusammenfassung

Standardisierte Entwicklungsumgebung durch .editorconfig, vollstaendige .gitignore und MIT-Lizenz.

## Was passiert aktuell

- `.gitignore` enthaelt nur eine Zeile (JetBrains IDE)
- Keine `.editorconfig` vorhanden
- README erwaehnt MIT License, aber keine LICENSE-Datei im Repository

## Was soll zukuenftig passieren

- **`.gitignore`**: Vollstaendige .NET-gitignore (via `dotnet new gitignore`) plus JetBrains-, macOS- und .env-Eintraege
- **`.editorconfig`**: C#-Conventions (file-scoped namespaces, Naming-Conventions, Formatting), Einrueckung und Encoding fuer alle Dateitypen
- **`LICENSE`**: MIT-Lizenz-Datei mit Copyright Stefan Hoffmann 2025
