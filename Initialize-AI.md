# KI-Initialisierung für Golden Cage .NET Template

Diese Datei enthält Befehle zur schnellen und effektiven Initialisierung von KI-Tools für die Arbeit mit diesem produktionsreifen .NET-Template.

## Vollständige Projekt-Initialisierung

```bash
# Grundlegende Projektstruktur und Features verstehen
cat README.md
cat CLAUDE.md

# Anforderungen und Spezifikationen lesen
find ./Dokumentation/Anforderungen -name "*.md" -exec echo "=== {} ===" \; -exec cat {} \;

# Architektur-Dokumentation laden
cat ./Dokumentation/Architektur/CodeStyle.md
cat ./Dokumentation/Architektur/ProgrammierPatterns.md

# Technologie-Stack und Framework-Dokumentation verstehen
find ./Dokumentation/Technologien -name "*.md" -exec echo "=== {} ===" \; -exec cat {} \;

# Datenbank-Migration System verstehen
cat ./Source/DBMigrations/README.md

# Test-Strategien analysieren
cat ./Tests/Datenbank/README.md
cat ./Tests/UI/README.md
```

## Standard-Prompts verfügbar machen

```bash
# Security Review Prompt
cat ./Dokumentation/Commands/Security-Review.md

# Code Quality Prompt  
cat ./Dokumentation/Commands/Code-Quality.md

# Architektur-Prüfung Prompt
cat ./Dokumentation/Commands/Architektur-Pruefung.md

# Rechtschreibprüfung Prompt
cat ./Dokumentation/Commands/Rechtschreibpruefung.md
```

## Automatisierungs-Scripts verstehen

```bash
# Alle verfügbaren PowerShell-Scripts anzeigen
ls -la ./Scripts/*.ps1

# Build-System Dokumentation
Get-Help ./Scripts/Build.ps1 -Examples

# Datenbank-Scripts Dokumentation  
Get-Help ./Scripts/Database-Initialize.ps1 -Examples
Get-Help ./Scripts/Database-Migrate.ps1 -Examples

# Test-Ausführungs-Scripts
Get-Help ./Scripts/Run-Tests.ps1 -Examples
Get-Help ./Scripts/Run-UITests.ps1 -Examples

# Solution-Generator verstehen
Get-Help ./Source/Create-Solution.ps1 -Examples
```

## Schnell-Initialisierung (Basis)

```bash
# Nur die wichtigsten Informationen für schnellen Start
cat README.md
cat ./Dokumentation/Anforderungen/README.md
ls -la ./Source/
ls -la ./Tests/
```

## Entwicklungsumgebung verstehen

```bash
# Aktueller Code-Stand
find ./Source -name "*.cs" -o -name "*.csproj" -o -name "*.sln" | head -20
find ./Tests -name "*.cs" | head -10

# Verfügbare Scripts
ls -la ./Scripts/
```

## KI-Prompt für Vollinitialisierung

```
Bitte führe eine systematische Repository-Analyse des Golden Cage .NET Templates durch:

1. **Template-Überblick verstehen:**
   - Lies README.md für Template-Features und Produktionsmerkmale
   - Studiere CLAUDE.md für KI-optimierte Entwicklungsrichtlinien
   - Verstehe die vollautomatisierten PowerShell-Scripts

2. **Automatisierungs-Infrastruktur analysieren:**
   - Analysiere alle Scripts in ./Scripts/ (Build, Database, Testing)
   - Verstehe Create-Solution.ps1 für automatische Projektgenerierung
   - Prüfe die Migrations-Dokumentation in ./Source/DBMigrations/README.md

3. **Test-Architekturen verstehen:**
   - Studiere ./Tests/Datenbank/README.md für Datenbanktest-Strategien
   - Analysiere ./Tests/UI/README.md für Playwright UI-Testing
   - Verstehe Testcontainers, Page Object Model und Performance-Testing

4. **Dokumentations-Standards erfassen:**
   - Lies ./Dokumentation/Anforderungen/README.md für R00001-*.md Format
   - Analysiere Architektur-Templates (CodeStyle, ProgrammierPatterns)
   - Verstehe Standard-Prompts in ./Dokumentation/Commands/

5. **Code-Struktur und Template-System:**
   - Untersuche Source-Struktur mit Create-Solution.ps1 Generator
   - Prüfe DBMigrations mit 00001-beschreibung.sql Format
   - Analysiere Test-Strukturen für verschiedene Test-Typen

6. **Produktions-Features bewerten:**
   - Multi-Browser UI-Testing mit Playwright
   - Rollback-fähige Datenbank-Migrationen
   - Cross-Platform PowerShell Automatisierung
   - KI-optimierte Entwicklungsworkflows

7. **Template-Verwendung verstehen:**
   - Wie neue Solutions generiert werden
   - Wie Tests strukturiert und ausgeführt werden
   - Wie Datenbank-Migrationen verwaltet werden
   - Wie KI-Assistenten optimal genutzt werden

Verwende die Standard-Prompts aus ./Dokumentation/Commands/ für spezifische Analysen.
Fokussiere auf die produktionsreifen Automatisierungs-Features und KI-Integration.
```

## Einzelne Bereiche initialisieren

### Nur Anforderungen
```bash
cat ./Dokumentation/Anforderungen/README.md
find ./Dokumentation/Anforderungen -name "R*.md" -exec cat {} \;
```

### Nur Architektur
```bash
find ./Dokumentation/Architektur -name "*.md" -exec cat {} \;
```

### Nur Technologien
```bash
find ./Dokumentation/Technologien -name "*.md" -exec cat {} \;
```

### Nur Scripts und Automatisierung
```bash
ls -la ./Scripts/*.ps1
cat ./Source/Create-Solution.ps1
Get-Help ./Scripts/Build.ps1 -Examples
```

### Nur Test-Architekturen
```bash
cat ./Tests/Datenbank/README.md
cat ./Tests/UI/README.md
find ./Tests -name "*.cs" | head -10
```

### Nur Datenbank-System
```bash
cat ./Source/DBMigrations/README.md
Get-Help ./Scripts/Database-Initialize.ps1 -Examples
Get-Help ./Scripts/Database-Migrate.ps1 -Examples
```

## Hinweise

- **Vollständige Initialisierung**: Für optimalen KI-Kontext alle Befehle ausführen
- **Template-Features**: Fokus auf produktionsreife Automatisierung und KI-Integration
- **Spezifische Bereiche**: Für gezielte Aufgaben entsprechende Einzelbereich-Initialisierung
- **Standard-Prompts**: Nutze vorgefertigte Prompts für Code-Reviews und Qualitätssicherung
- **Script-Dokumentation**: Alle PowerShell-Scripts haben integrierte Hilfe und Beispiele