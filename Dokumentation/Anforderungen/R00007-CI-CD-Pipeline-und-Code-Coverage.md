---
id: R00007
titel: "CI/CD-Pipeline und Code-Coverage einrichten"
typ: Infrastruktur
status: Umgesetzt
erstellt: 2026-04-03
---

# CI/CD-Pipeline und Code-Coverage

## Kurzzusammenfassung

Automatisierter Build und Test bei jedem Push/PR via GitHub Actions, inklusive Code-Coverage-Messung.

## Was passiert aktuell

Keine CI/CD-Pipeline vorhanden. Build und Tests werden nur lokal ausgefuehrt. Keine Coverage-Messung.

## Was soll zukuenftig passieren

- GitHub Actions Workflow `.github/workflows/build-and-test.yml`
- Automatisch bei Push auf `main` und bei Pull Requests
- Steps: Checkout, .NET Setup, Restore, Build, Unit-Tests mit Coverage
- `coverlet.collector` in beiden Test-Projekten fuer Coverage-Daten
- Coverage-Report als Artefakt hochladen

## Implementationshinweise

- Integration-Tests werden im CI nicht ausgefuehrt (benoetigen SQL Server)
- Coverage-Artefakt im Cobertura-Format
- Spaeter: SQL Server Container im CI fuer Integration-Tests
