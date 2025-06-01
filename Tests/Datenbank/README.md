# Datenbank-Tests

Diese Dokumentation beschreibt die Strategie und Implementierung von Datenbanktests in diesem Projekt.

## Übersicht

Datenbanktests stellen sicher, dass:
- Datenbankschema korrekt funktioniert
- Datenintegrität gewährleistet ist
- Performance-Anforderungen erfüllt werden
- Migrationen ordnungsgemäß funktionieren

## Test-Kategorien

### 1. Schema-Tests
Validierung der Datenbankstruktur und -konfiguration.

### 2. Migrations-Tests  
Testen der Datenbank-Migrationen und deren Rollback-Fähigkeit.

### 3. Data Access Tests
Integration zwischen Anwendung und Datenbank.

### 4. Performance-Tests
Messung der Datenbankleistung und Optimierung.

## Projekt-Struktur

```
Tests/Datenbank/
├── README.md                    # Diese Dokumentation
├── Schema/                      # Schema-Validierungstests
│   ├── TableTests.cs           # Tabellen-Struktur Tests
│   ├── IndexTests.cs           # Index-Validierung
│   ├── ConstraintTests.cs      # Constraint-Tests
│   └── ViewTests.cs            # View-Tests
├── Migrations/                  # Migrations-Tests
│   ├── MigrationTests.cs       # Migration-Ausführung
│   ├── RollbackTests.cs        # Rollback-Funktionalität
│   └── DataConsistencyTests.cs # Datenkonsistenz
├── DataAccess/                  # Data Access Layer Tests
│   ├── RepositoryTests.cs      # Repository-Pattern Tests
│   ├── EntityTests.cs          # Entity-Framework Tests
│   └── ConnectionTests.cs      # Verbindungs-Tests
├── Performance/                 # Performance-Tests
│   ├── QueryPerformanceTests.cs
│   ├── IndexEfficiencyTests.cs
│   └── LoadTests.cs
└── TestData/                   # Test-Daten und Fixtures
    ├── SampleData.sql
    ├── TestDataBuilder.cs
    └── DatabaseFixtures.cs
```

## Test-Setup

### NuGet Packages
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.1.0" />
<PackageReference Include="Testcontainers.SqlServer" Version="3.6.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="NUnit" Version="3.14.0" />
<PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
<PackageReference Include="Bogus" Version="34.0.2" />
```

### Database Test Base Class
```csharp
public abstract class DatabaseTestBase : IDisposable
{
    protected readonly string ConnectionString;
    protected readonly IServiceProvider ServiceProvider;
    
    protected DatabaseTestBase()
    {
        // Setup Test Database (Docker Container oder LocalDB)
        ConnectionString = SetupTestDatabase();
        ServiceProvider = ConfigureServices();
    }
    
    private string SetupTestDatabase()
    {
        // Option 1: Testcontainers (Docker)
        var container = new SqlServerBuilder()
            .WithPassword("YourStrong!Passw0rd")
            .Build();
        
        container.StartAsync().Wait();
        return container.GetConnectionString();
        
        // Option 2: LocalDB
        // return "Server=(localdb)\\mssqllocaldb;Database=TestDb_" + 
        //        Guid.NewGuid().ToString("N")[..8] + ";Trusted_Connection=true;";
    }
    
    private IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(ConnectionString));
            
        services.AddScoped<IRepository<User>, Repository<User>>();
        
        return services.BuildServiceProvider();
    }
    
    protected async Task<AppDbContext> GetDbContextAsync()
    {
        var context = ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        return context;
    }
    
    public void Dispose()
    {
        ServiceProvider?.Dispose();
        // Container cleanup wird automatisch durch Testcontainers gemacht
    }
}
```

## Test-Implementierungen

### Schema-Tests

#### Tabellen-Struktur Tests
```csharp
[TestFixture]
public class TableTests : DatabaseTestBase
{
    [Test]
    public async Task Users_Table_Should_Have_Correct_Schema()
    {
        // Arrange
        using var context = await GetDbContextAsync();
        
        // Act
        var entityType = context.Model.FindEntityType(typeof(User));
        
        // Assert
        entityType.Should().NotBeNull();
        entityType.GetTableName().Should().Be("Users");
        
        // Überprüfe Spalten
        var properties = entityType.GetProperties().ToList();
        properties.Should().Contain(p => p.Name == "Id" && p.ClrType == typeof(Guid));
        properties.Should().Contain(p => p.Name == "Email" && p.ClrType == typeof(string));
        properties.Should().Contain(p => p.Name == "CreatedAt" && p.ClrType == typeof(DateTime));
    }
    
    [Test]
    public async Task Users_Table_Should_Have_Primary_Key()
    {
        // Arrange
        using var context = await GetDbContextAsync();
        
        // Act
        var entityType = context.Model.FindEntityType(typeof(User));
        var primaryKey = entityType.FindPrimaryKey();
        
        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey.Properties.Should().HaveCount(1);
        primaryKey.Properties.First().Name.Should().Be("Id");
    }
}
```

#### Index-Tests
```csharp
[TestFixture]
public class IndexTests : DatabaseTestBase
{
    [Test]
    public async Task Users_Should_Have_Email_Index()
    {
        // Arrange
        using var context = await GetDbContextAsync();
        
        // Act
        var entityType = context.Model.FindEntityType(typeof(User));
        var indexes = entityType.GetIndexes();
        
        // Assert
        indexes.Should().Contain(i => 
            i.Properties.Any(p => p.Name == "Email"));
    }
    
    [Test]
    public async Task Email_Index_Should_Be_Unique()
    {
        // Arrange
        using var context = await GetDbContextAsync();
        
        // Act
        var entityType = context.Model.FindEntityType(typeof(User));
        var emailIndex = entityType.GetIndexes()
            .First(i => i.Properties.Any(p => p.Name == "Email"));
        
        // Assert
        emailIndex.IsUnique.Should().BeTrue();
    }
}
```

### Migrations-Tests

```csharp
[TestFixture]
public class MigrationTests : DatabaseTestBase
{
    [Test]
    public async Task Migration_Should_Create_All_Tables()
    {
        // Arrange
        using var context = await GetDbContextAsync();
        
        // Act
        await context.Database.MigrateAsync();
        
        // Assert
        var tableExists = await context.Database
            .ExecuteSqlRawAsync("SELECT 1 FROM sys.tables WHERE name = 'Users'");
        tableExists.Should().Be(-1); // -1 bedeutet erfolgreich ausgeführt
    }
    
    [Test]
    public async Task Migration_Should_Be_Idempotent()
    {
        // Arrange
        using var context = await GetDbContextAsync();
        
        // Act - Führe Migration zweimal aus
        await context.Database.MigrateAsync();
        await context.Database.MigrateAsync();
        
        // Assert - Sollte ohne Fehler funktionieren
        var user = new User { Email = "test@example.com" };
        context.Users.Add(user);
        var result = await context.SaveChangesAsync();
        
        result.Should().Be(1);
    }
}
```

### Data Access Tests

```csharp
[TestFixture]
public class RepositoryTests : DatabaseTestBase
{
    [Test]
    public async Task Repository_Should_Save_User()
    {
        // Arrange
        using var context = await GetDbContextAsync();
        var repository = new Repository<User>(context);
        var user = new User 
        { 
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        
        // Act
        await repository.AddAsync(user);
        await repository.SaveChangesAsync();
        
        // Assert
        var savedUser = await repository.GetByIdAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser.Email.Should().Be("test@example.com");
    }
    
    [Test]
    public async Task Repository_Should_Enforce_Unique_Email()
    {
        // Arrange
        using var context = await GetDbContextAsync();
        var repository = new Repository<User>(context);
        
        var user1 = new User { Email = "test@example.com" };
        var user2 = new User { Email = "test@example.com" };
        
        // Act
        await repository.AddAsync(user1);
        await repository.SaveChangesAsync();
        
        await repository.AddAsync(user2);
        
        // Assert
        var act = async () => await repository.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
```

### Performance-Tests

```csharp
[TestFixture]
public class QueryPerformanceTests : DatabaseTestBase
{
    [Test]
    public async Task User_Query_By_Email_Should_Be_Fast()
    {
        // Arrange
        using var context = await GetDbContextAsync();
        await SeedTestData(context, userCount: 10000);
        
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == "test5000@example.com");
        
        stopwatch.Stop();
        
        // Assert
        user.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Unter 100ms
    }
    
    private async Task SeedTestData(AppDbContext context, int userCount)
    {
        var faker = new Faker<User>()
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName());
        
        var users = faker.Generate(userCount);
        
        // Spezielle Test-User
        users[5000].Email = "test5000@example.com";
        
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }
}
```

## Test-Daten Management

### Test Data Builder
```csharp
public class TestDataBuilder
{
    private readonly AppDbContext _context;
    
    public TestDataBuilder(AppDbContext context)
    {
        _context = context;
    }
    
    public UserBuilder Users => new(_context);
}

public class UserBuilder
{
    private readonly AppDbContext _context;
    private readonly List<User> _users = new();
    
    public UserBuilder(AppDbContext context)
    {
        _context = context;
    }
    
    public UserBuilder WithUser(string email, string firstName = "John", string lastName = "Doe")
    {
        _users.Add(new User 
        { 
            Email = email, 
            FirstName = firstName, 
            LastName = lastName 
        });
        return this;
    }
    
    public UserBuilder WithUsers(int count)
    {
        var faker = new Faker<User>()
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName());
        
        _users.AddRange(faker.Generate(count));
        return this;
    }
    
    public async Task<List<User>> BuildAsync()
    {
        _context.Users.AddRange(_users);
        await _context.SaveChangesAsync();
        return _users;
    }
}
```

### Verwendung in Tests
```csharp
[Test]
public async Task Example_Test_With_TestData()
{
    // Arrange
    using var context = await GetDbContextAsync();
    var builder = new TestDataBuilder(context);
    
    var users = await builder.Users
        .WithUser("admin@example.com", "Admin", "User")
        .WithUsers(100)
        .BuildAsync();
    
    // Act & Assert
    var adminUser = await context.Users
        .FirstAsync(u => u.Email == "admin@example.com");
    
    adminUser.FirstName.Should().Be("Admin");
}
```

## Continuous Integration

### GitHub Actions
```yaml
name: Database Tests

on: [push, pull_request]

jobs:
  database-tests:
    runs-on: ubuntu-latest
    
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          SA_PASSWORD: YourStrong!Passw0rd
          ACCEPT_EULA: Y
        ports:
          - 1433:1433
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Run Database Tests
      run: |
        dotnet test Tests/Datenbank/ --logger trx --results-directory TestResults
      env:
        ConnectionStrings__DefaultConnection: "Server=localhost,1433;Database=TestDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true"
```

## Best Practices

### 1. Isolation
- Jeder Test verwendet eine eigene Datenbankinstanz
- Tests können parallel ausgeführt werden
- Keine Abhängigkeiten zwischen Tests

### 2. Performance
- Verwende In-Memory-Datenbank für einfache Tests
- Docker Container für komplexe Integrationstests
- Lazy Loading für Test-Daten

### 3. Wartbarkeit
- Gemeinsame Base-Klassen für Setup
- Builder-Pattern für Test-Daten
- Aussagekräftige Test-Namen

### 4. Realitätsnähe
- Teste gegen echte Datenbank-Engine
- Verwende realistische Datenmengen
- Simuliere Produktionsszenarien

## Ausführung

### Lokal
```bash
# Alle Datenbank-Tests
dotnet test Tests/Datenbank/

# Spezifische Test-Kategorie
dotnet test Tests/Datenbank/Schema/

# Mit Coverage
dotnet test Tests/Datenbank/ --collect:"XPlat Code Coverage"
```

### CI/CD
```bash
# Mit PowerShell Script
.\Scripts\Run-Tests.ps1 -IntegrationTests -Category "Database"
```