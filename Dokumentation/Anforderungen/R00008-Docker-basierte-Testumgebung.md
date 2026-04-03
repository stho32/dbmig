---
id: R00008
titel: "Docker-basierte Testumgebung fuer SQL Server"
typ: Infrastruktur
status: Umgesetzt
erstellt: 2026-04-03
---

# Docker-basierte Testumgebung

## Kurzzusammenfassung

Eine `docker-compose.yml` mit SQL Server-Container und automatischer Datenbank-Initialisierung, um Entwickler-Onboarding und Integration-Tests zu vereinfachen.

## Was passiert aktuell

Integration-Tests benoetigen einen manuell installierten SQL Server und manuell angelegte Datenbanken (`dbmig_integration_test`, `dbmig_unit_test`, `dbmig_default`).

## Was soll zukuenftig passieren

- `docker-compose.yml` mit `mcr.microsoft.com/mssql/server:2019-latest`
- Init-Script `docker/init-databases.sql` erstellt alle drei Test-Datenbanken automatisch
- Health-Check fuer SQL Server-Bereitschaft
- Ein-Befehl-Setup: `docker compose up -d`

## Implementationshinweise

- SA-Passwort fuer Entwicklung: `DbMig_Test_2024!`
- Port 1433 mapped
- Volume fuer Daten-Persistenz
- Init-Script prueft ob Datenbanken bereits existieren
