# Code Quality Prompt

## Prompt für Code-Qualitätsprüfung

```
Analysiere die Code-Qualität und prüfe folgende Aspekte:

1. **Code Readability**:
   - Selbsterklärende Variablen- und Methodennamen
   - Angemessene Kommentierung
   - Konsistente Formatierung
   - Vermeidung von "Magic Numbers" und "Magic Strings"

2. **Code Complexity**:
   - Zyklomatische Komplexität
   - Methodenlänge und Parameteranzahl
   - Verschachtelungstiefe
   - DRY (Don't Repeat Yourself) Prinzip

3. **Error Handling**:
   - Vollständige Exception-Behandlung
   - Angemessene Exception-Typen
   - Logging von Fehlern
   - Graceful Degradation

4. **Resource Management**:
   - Ordnungsgemäße Disposal von Ressourcen
   - Using-Statements für IDisposable
   - Memory Leaks vermeiden

5. **Performance**:
   - Ineffiziente Algorithmen
   - Unnötige Objekterstellung
   - String-Konkatenation in Schleifen
   - Database Query Optimierung

6. **Maintainability**:
   - Lose Kopplung zwischen Klassen
   - Hohe Kohäsion innerhalb von Klassen
   - Einfache Erweiterbarkeit

Gib konkrete Verbesserungsvorschläge mit Code-Beispielen.
```

## Verwendung

Nutze diesen Prompt für regelmäßige Code-Reviews und zur Verbesserung der allgemeinen Code-Qualität.