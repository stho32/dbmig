# Generelle Architektur Dotnet

## Projektstruktur und Abhängigkeiten

Die Anwendung besteht aus 4 Teilprojekten mit klarer Abhängigkeitsrichtung:
- **dbmig.Console** (Presentation Layer)
- **dbmig.BL** (Business Logic Layer)
- **dbmig.BL.Tests** (Unit Tests)
- **dbmig.BL.IntegrationTests** (Integration Tests)

**Abhängigkeitsstruktur:** dbmig.Console → dbmig.BL (keine Rückabhängigkeit)

## Verantwortlichkeiten der Projekte

### dbmig.Console
Enthält ausschließlich Konsolen-Frontend-Komponenten:
- Interpretation von Kommandozeilenparametern
- Log-Ausgabe auf die Konsole
- Verknüpfung von Inhalten der BL-Bibliothek für weitere Funktionen

**Warum getrennt?** Diese Trennung ermöglicht Testbarkeit der Geschäftslogik ohne Konsolen-Dependencies und erlaubt zukünftige UI-Erweiterungen (Web, Desktop) mit derselben BL.

### dbmig.BL
Die Business-Logik für die Anwendung - vollkommen unabhängig von Konsolen-Code:
- Migrations-Engine und Datenbankoperationen
- Geschäftsregeln und Validierungslogik
- Feature-basierte Ordnerstruktur nach Themenkomplexen

**Bevorzugte Struktur:**
- Themenkomplex-Ordner
  - ggf. Unterthema-Ordner
    - Alle Dateien, die dazu nötig sind ggf. weiterstrukturiert

**Gemeinsame Komponenten:** "Common" Ordner für geteilte Utilities, ansonsten sind die Themenkomplexe relativ unabhängig voneinander.

## Teststrategie

### dbmig.BL.Tests
Unit-Tests mit NUnit für isolierte BL-Komponenten:
- Isolierte Tests pro Klasse/Feature
- Keine externen Dependencies (Mocks/Stubs)
- Schnelle Ausführung

### dbmig.BL.IntegrationTests
End-to-End-Szenarien mit echter Datenbank:
- Datenbankoperationen und komplette Migration-Workflows
- Realistische Testumgebungen
- Performance-Tests für Migrationen

## Logging
- Der Logging-Aspekt ist über das Logging-Thema abgedeckt, in dem ein ILogger Interface definiert wird und über eine LoggerFactory.Get-Methode wird eine Implementation für die Console zurückgegeben. Das ist das einzige Thema, für den es keinen Interactor gibt, weil es sich um einen Aspekt handelt.

## Was ist ein Interaktor?
- Jede Funktionalität für die UI ist ein einer Klasse XyzInteractor definiert, in der sich 1..n public methods für Methoden befinden.
- Es gibt in jedem Thema einen Interactor, was insgesamt bedeutet, dass es in jedem Themen-Ordner oder in jedem der Unterthemen-Ordner einen Interactor gibt.
- Interaktoren geben über public-Methoden immer einen Result zurück, das ist entweder ein Result-Record (IsSuccess:bool, Message:string) oder ein Result<T>-Record (Value:T, IsSuccess:bool, Message:string).
- Jede public-Method in einem Interaktor ist in ein try-Catch gewrappt und Fehlermeldungen werden über den Result zurückgegeben.
- Dabei ist die Message, die zurückgegeben wird, eine gute Beschreibung des Fehlers (oder allgemeine Beschreibung) für die UI und den Benutzer. Die eigentlichen Exceptions und ggf. weitere Informationen über die Situation des Fehlers werden an den Logger gegeben.
- Interaktoren bekommen über ihren Constructor ggf. Repositories oder andere Subklassen eingegeben, die man für Integrations-Tests austauschen muss.
- Es gibt eine zentrale InteractorFactory in der BL-Bibliothek, über die Interaktoren für die Oberfläche durch Get...Interactor abgerufen werden können.
- In der IntegrationTests und Tests Bibliothek gibt es nach Wunsch eigene Interactor-Factories die dann die Tests auf höchster Ebene ausführen.
- Versuche für Interaktoren immer eine 100%-Testabdeckung zu erreichen.

## Datenbankzugriff
- Wir nutzen den Repository-Pattern für die Zugriffe auf die Datenbank.
- Für den konkreten Zugriff nutzen wir eine IDatabaseAccessor-Schnittstelle, die mit einem SqlServerDatabaseAccessor implementiert ist.
- In dem DatabaseAccessor gibt es eine Reihe von einfachen Methoden, die entweder abfragend sind und einzelne Records oder Arrays von Records zurückgeben.
- Es gibt möglicherweise eine Implementation des DatabaseAccessors mit und ohne umspannende Transaktion.
- Repositories nutzen einen IDatabaseAccessor zum Zugriff auf die tatsächliche Datenbank
