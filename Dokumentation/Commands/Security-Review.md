# Security Review Prompt

## Prompt für Sicherheitsüberprüfung

```
Führe eine umfassende Sicherheitsanalyse des Codes durch und prüfe auf:

1. **Input Validation**: 
   - SQL Injection Schwachstellen
   - XSS (Cross-Site Scripting) Anfälligkeiten
   - Command Injection Risiken

2. **Authentication & Authorization**:
   - Schwache Passwort-Richtlinien
   - Unsichere Session-Verwaltung
   - Fehlende Autorisierungsprüfungen

3. **Data Protection**:
   - Unverschlüsselte sensible Daten
   - Unsichere Datenübertragung
   - Logging von sensiblen Informationen

4. **Error Handling**:
   - Information Disclosure durch Fehlermeldungen
   - Unbehandelte Exceptions

5. **Dependencies**:
   - Veraltete oder unsichere NuGet-Pakete
   - Bekannte Sicherheitslücken

Gib konkrete Verbesserungsvorschläge mit Code-Beispielen.
```

## Verwendung

Füge diesen Prompt in deine KI-Anfrage ein, wenn du eine Sicherheitsüberprüfung benötigst.