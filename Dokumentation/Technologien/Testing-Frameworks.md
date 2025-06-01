# Testing Frameworks

## Überblick

Dokumentation der verwendeten Test-Frameworks und Testing-Strategien.

## Unit Testing

### NUnit
```csharp
// NUnit Framework für Unit-Tests mit Attribut-basierter Konfiguration
[TestFixture]
public class ExampleTests
{
    [SetUp]
    public void Setup() { }
    
    [Test]
    public void SampleTest() { }
    
    [TestCase("value1")]
    [TestCase("value2")]
    public void ParameterizedTest(string input) { }
}
```

### Test Structure
```csharp
// TODO: AAA Pattern (Arrange, Act, Assert) Beispiele
```

## Mocking

### Moq Framework
```csharp
// TODO: Moq Setup und Verwendung
```

### Test Doubles
- TODO: Mock, Stub, Fake Unterschiede und Verwendung

## Integration Testing

### TestServer Setup
```csharp
// TODO: ASP.NET Core Integration Tests
```

### Database Testing
```csharp
// TODO: In-Memory Database für Integration Tests
```

## End-to-End Testing

### Selenium/Playwright
- TODO: Browser-Testing Setup

### API Testing
```csharp
// TODO: HTTP Client Testing Strategien
```

## Test Data Management

### Test Fixtures
```csharp
// TODO: Test Data Setup und Cleanup
```

### Factory Pattern für Tests
```csharp
// TODO: Test Object Creation Patterns
```

## Code Coverage

### Tooling
- TODO: Code Coverage Tools und Konfiguration

### Coverage Targets
- TODO: Coverage-Ziele und Metriken

## Continuous Testing

### Test Pipeline
- TODO: CI/CD Test Integration