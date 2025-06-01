# UI-Tests mit Playwright

Diese Dokumentation beschreibt die Implementierung und Durchführung von UI-Tests mit Playwright in diesem Projekt.

## Übersicht

UI-Tests automatisieren die Benutzerinteraktion mit der Webanwendung und stellen sicher, dass:
- Die Benutzeroberfläche korrekt funktioniert
- User Workflows einwandfrei ablaufen
- Cross-Browser-Kompatibilität gewährleistet ist
- Performance-Standards eingehalten werden

## Warum Playwright?

- **Cross-Browser**: Chrome, Firefox, Safari, Edge
- **Multi-Platform**: Windows, macOS, Linux
- **Modern Web**: Single Page Applications, Progressive Web Apps
- **Robust**: Auto-wait, retry-Mechanismen
- **Developer Experience**: Screenshots, Videos, Debugging-Tools

## Projekt-Setup

### NuGet Packages
```xml
<PackageReference Include="Microsoft.Playwright" Version="1.40.0" />
<PackageReference Include="Microsoft.Playwright.NUnit" Version="1.40.0" />
<PackageReference Include="NUnit" Version="3.14.0" />
<PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
```

### Playwright Installation
```bash
# Browser-Downloads (einmalig nach Package-Installation)
pwsh bin/Debug/net8.0/playwright.ps1 install

# Oder mit dotnet tool
dotnet tool install --global Microsoft.Playwright.CLI
playwright install
```

## Projekt-Struktur

```
Tests/UI/
├── README.md                    # Diese Dokumentation
├── PageObjects/                 # Page Object Model
│   ├── BasePage.cs             # Basis-Klasse für alle Seiten
│   ├── LoginPage.cs            # Login-Seite
│   ├── DashboardPage.cs        # Dashboard-Seite
│   └── Components/             # Wiederverwendbare UI-Komponenten
│       ├── NavigationComponent.cs
│       └── ModalComponent.cs
├── Tests/                      # Test-Implementierungen
│   ├── LoginTests.cs           # Login-Funktionalität
│   ├── NavigationTests.cs      # Navigation-Tests
│   ├── UserManagementTests.cs  # Benutzerverwaltung
│   └── PerformanceTests.cs     # Performance-Tests
├── Fixtures/                   # Test-Fixtures und Setup
│   ├── UITestBase.cs           # Basis-Klasse für UI-Tests
│   ├── TestDataSetup.cs        # Test-Daten Management
│   └── BrowserFixture.cs       # Browser-Konfiguration
├── Utils/                      # Hilfsfunktionen
│   ├── ScreenshotHelper.cs     # Screenshot-Utilities
│   ├── WaitHelper.cs           # Wait-Strategien
│   └── TestDataBuilder.cs     # Test-Daten Builder
└── Config/                     # Konfiguration
    ├── playwright.config.json  # Playwright-Konfiguration
    └── appsettings.test.json   # Test-spezifische Einstellungen
```

## Basis-Setup

### Test Base Class
```csharp
[TestFixture]
public abstract class UITestBase
{
    protected IPlaywright Playwright { get; private set; }
    protected IBrowser Browser { get; private set; }
    protected IBrowserContext Context { get; private set; }
    protected IPage Page { get; private set; }
    
    protected virtual BrowserTypeLaunchOptions LaunchOptions => new()
    {
        Headless = Environment.GetEnvironmentVariable("HEADLESS") != "false",
        SlowMo = 50, // Verlangsamung für bessere Nachvollziehbarkeit
        Timeout = 30000
    };
    
    protected virtual BrowserNewContextOptions ContextOptions => new()
    {
        ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
        Locale = "de-DE",
        TimezoneId = "Europe/Berlin",
        RecordVideoDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "videos")
    };
    
    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        
        // Browser-Auswahl basierend auf Umgebungsvariable
        var browserType = Environment.GetEnvironmentVariable("BROWSER")?.ToLower() switch
        {
            "firefox" => Playwright.Firefox,
            "webkit" => Playwright.Webkit,
            "edge" => Playwright.Chromium, // Edge läuft über Chromium
            _ => Playwright.Chromium // Default: Chrome
        };
        
        Browser = await browserType.LaunchAsync(LaunchOptions);
    }
    
    [SetUp]
    public async Task Setup()
    {
        Context = await Browser.NewContextAsync(ContextOptions);
        Page = await Context.NewPageAsync();
        
        // Global error handling
        Page.PageError += (_, error) => 
        {
            TestContext.WriteLine($"Page Error: {error}");
        };
        
        Page.Console += (_, msg) => 
        {
            if (msg.Type == "error")
            {
                TestContext.WriteLine($"Console Error: {msg.Text}");
            }
        };
    }
    
    [TearDown]
    public async Task TearDown()
    {
        // Screenshot bei Test-Failure
        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
        {
            await TakeScreenshotAsync($"failure_{TestContext.CurrentContext.Test.Name}");
        }
        
        await Page?.CloseAsync();
        await Context?.CloseAsync();
    }
    
    [OneTimeTearDown]
    public async Task GlobalTearDown()
    {
        await Browser?.CloseAsync();
        Playwright?.Dispose();
    }
    
    protected async Task<string> TakeScreenshotAsync(string name)
    {
        var screenshotPath = Path.Combine(
            TestContext.CurrentContext.WorkDirectory, 
            "screenshots", 
            $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png"
        );
        
        Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath));
        await Page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
        
        TestContext.AddTestAttachment(screenshotPath, "Screenshot");
        return screenshotPath;
    }
}
```

## Page Object Model

### Base Page
```csharp
public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly string BaseUrl;
    
    protected BasePage(IPage page, string baseUrl)
    {
        Page = page;
        BaseUrl = baseUrl;
    }
    
    // Common elements present on all pages
    protected ILocator NavigationMenu => Page.Locator("[data-testid='navigation-menu']");
    protected ILocator UserProfileDropdown => Page.Locator("[data-testid='user-profile']");
    protected ILocator LogoutButton => Page.Locator("[data-testid='logout-button']");
    
    // Common actions
    public async Task<bool> IsPageLoadedAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return await Page.Locator("body").IsVisibleAsync();
    }
    
    public async Task WaitForElementAsync(ILocator element, int timeoutMs = 30000)
    {
        await element.WaitForAsync(new() { Timeout = timeoutMs });
    }
    
    public async Task LogoutAsync()
    {
        await UserProfileDropdown.ClickAsync();
        await LogoutButton.ClickAsync();
        await Page.WaitForURLAsync("**/login");
    }
}
```

### Login Page
```csharp
public class LoginPage : BasePage
{
    public LoginPage(IPage page, string baseUrl) : base(page, baseUrl) { }
    
    // Locators
    private ILocator EmailInput => Page.Locator("[data-testid='email-input']");
    private ILocator PasswordInput => Page.Locator("[data-testid='password-input']");
    private ILocator LoginButton => Page.Locator("[data-testid='login-button']");
    private ILocator ErrorMessage => Page.Locator("[data-testid='error-message']");
    private ILocator RememberMeCheckbox => Page.Locator("[data-testid='remember-me']");
    
    // Actions
    public async Task NavigateAsync()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
        await IsPageLoadedAsync();
    }
    
    public async Task LoginAsync(string email, string password, bool rememberMe = false)
    {
        await EmailInput.FillAsync(email);
        await PasswordInput.FillAsync(password);
        
        if (rememberMe)
        {
            await RememberMeCheckbox.CheckAsync();
        }
        
        await LoginButton.ClickAsync();
    }
    
    public async Task<string> GetErrorMessageAsync()
    {
        await ErrorMessage.WaitForAsync();
        return await ErrorMessage.TextContentAsync();
    }
    
    public async Task<bool> IsLoginFormVisibleAsync()
    {
        return await EmailInput.IsVisibleAsync() && 
               await PasswordInput.IsVisibleAsync() && 
               await LoginButton.IsVisibleAsync();
    }
}
```

### Dashboard Page
```csharp
public class DashboardPage : BasePage
{
    public DashboardPage(IPage page, string baseUrl) : base(page, baseUrl) { }
    
    // Locators
    private ILocator WelcomeMessage => Page.Locator("[data-testid='welcome-message']");
    private ILocator StatsWidget => Page.Locator("[data-testid='stats-widget']");
    private ILocator RecentActivityList => Page.Locator("[data-testid='recent-activity']");
    private ILocator QuickActionButtons => Page.Locator("[data-testid='quick-actions'] button");
    
    // Actions
    public async Task<bool> IsLoadedAsync()
    {
        await WelcomeMessage.WaitForAsync();
        return await WelcomeMessage.IsVisibleAsync();
    }
    
    public async Task<string> GetWelcomeMessageAsync()
    {
        return await WelcomeMessage.TextContentAsync();
    }
    
    public async Task<int> GetRecentActivityCountAsync()
    {
        return await RecentActivityList.Locator("li").CountAsync();
    }
    
    public async Task ClickQuickActionAsync(string actionName)
    {
        await QuickActionButtons
            .Filter(new() { HasText = actionName })
            .ClickAsync();
    }
}
```

## Test-Implementierungen

### Login Tests
```csharp
[TestFixture]
public class LoginTests : UITestBase
{
    private LoginPage _loginPage;
    private DashboardPage _dashboardPage;
    private readonly string _baseUrl = "https://localhost:5001";
    
    [SetUp]
    public async Task TestSetup()
    {
        await base.Setup();
        _loginPage = new LoginPage(Page, _baseUrl);
        _dashboardPage = new DashboardPage(Page, _baseUrl);
    }
    
    [Test]
    public async Task Login_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        await _loginPage.NavigateAsync();
        
        // Act
        await _loginPage.LoginAsync("test@example.com", "password123");
        
        // Assert
        await Page.WaitForURLAsync("**/dashboard");
        var isLoaded = await _dashboardPage.IsLoadedAsync();
        isLoaded.Should().BeTrue();
        
        var welcomeMessage = await _dashboardPage.GetWelcomeMessageAsync();
        welcomeMessage.Should().Contain("Willkommen");
    }
    
    [Test]
    public async Task Login_WithInvalidCredentials_ShouldShowError()
    {
        // Arrange
        await _loginPage.NavigateAsync();
        
        // Act
        await _loginPage.LoginAsync("invalid@example.com", "wrongpassword");
        
        // Assert
        var errorMessage = await _loginPage.GetErrorMessageAsync();
        errorMessage.Should().Contain("Ungültige Anmeldedaten");
        
        // User sollte auf Login-Seite bleiben
        Page.Url.Should().Contain("/login");
    }
    
    [Test]
    [TestCase("", "password123", "E-Mail ist erforderlich")]
    [TestCase("test@example.com", "", "Passwort ist erforderlich")]
    [TestCase("invalid-email", "password123", "Ungültige E-Mail")]
    public async Task Login_WithInvalidInput_ShouldShowValidationError(
        string email, string password, string expectedError)
    {
        // Arrange
        await _loginPage.NavigateAsync();
        
        // Act
        await _loginPage.LoginAsync(email, password);
        
        // Assert
        var errorMessage = await _loginPage.GetErrorMessageAsync();
        errorMessage.Should().Contain(expectedError);
    }
}
```

### Navigation Tests
```csharp
[TestFixture]
public class NavigationTests : UITestBase
{
    private readonly string _baseUrl = "https://localhost:5001";
    
    [SetUp]
    public async Task TestSetup()
    {
        await base.Setup();
        await LoginAsTestUserAsync();
    }
    
    [Test]
    public async Task Navigation_MenuItems_ShouldBeAccessible()
    {
        // Arrange
        var menuItems = new[] { "Dashboard", "Benutzer", "Einstellungen", "Berichte" };
        
        foreach (var menuItem in menuItems)
        {
            // Act
            await Page.Locator($"[data-testid='nav-{menuItem.ToLower()}']").ClickAsync();
            
            // Assert
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            Page.Url.Should().Contain(menuItem.ToLower());
            
            var pageTitle = await Page.Locator("h1").TextContentAsync();
            pageTitle.Should().Contain(menuItem);
        }
    }
    
    [Test]
    public async Task Navigation_BreadcrumbNavigation_ShouldWork()
    {
        // Arrange - Navigate to deep page
        await Page.GotoAsync($"{_baseUrl}/benutzer/123/bearbeiten");
        
        // Act - Click breadcrumb to go back
        await Page.Locator("[data-testid='breadcrumb-benutzer']").ClickAsync();
        
        // Assert
        Page.Url.Should().Contain("/benutzer");
        Page.Url.Should().NotContain("/123/bearbeiten");
    }
    
    private async Task LoginAsTestUserAsync()
    {
        var loginPage = new LoginPage(Page, _baseUrl);
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync("test@example.com", "password123");
        await Page.WaitForURLAsync("**/dashboard");
    }
}
```

### Performance Tests
```csharp
[TestFixture]
public class PerformanceTests : UITestBase
{
    private readonly string _baseUrl = "https://localhost:5001";
    
    [Test]
    public async Task PageLoad_Dashboard_ShouldBeUnder3Seconds()
    {
        // Arrange
        await LoginAsTestUserAsync();
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        await Page.GotoAsync($"{_baseUrl}/dashboard");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        stopwatch.Stop();
        
        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000);
    }
    
    [Test]
    public async Task LargeDataTable_ShouldRenderWithinTimeLimit()
    {
        // Arrange
        await LoginAsTestUserAsync();
        await Page.GotoAsync($"{_baseUrl}/benutzer");
        
        var stopwatch = Stopwatch.StartNew();
        
        // Act - Load table with 1000+ rows
        await Page.Locator("[data-testid='load-all-users']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        stopwatch.Stop();
        
        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
        
        var rowCount = await Page.Locator("table tbody tr").CountAsync();
        rowCount.Should().BeGreaterThan(1000);
    }
    
    private async Task LoginAsTestUserAsync()
    {
        var loginPage = new LoginPage(Page, _baseUrl);
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync("test@example.com", "password123");
        await Page.WaitForURLAsync("**/dashboard");
    }
}
```

## Konfiguration

### playwright.config.json
```json
{
  "timeout": 30000,
  "expect": {
    "timeout": 5000
  },
  "testDir": "./Tests",
  "retries": 2,
  "workers": 4,
  "reporter": [
    ["html", { "outputFolder": "playwright-report" }],
    ["junit", { "outputFile": "test-results.xml" }]
  ],
  "use": {
    "browserName": "chromium",
    "headless": true,
    "viewport": { "width": 1920, "height": 1080 },
    "screenshot": "only-on-failure",
    "video": "retain-on-failure",
    "trace": "retain-on-failure"
  },
  "projects": [
    {
      "name": "Desktop Chrome",
      "use": { "browserName": "chromium" }
    },
    {
      "name": "Desktop Firefox",
      "use": { "browserName": "firefox" }
    },
    {
      "name": "Desktop Safari",
      "use": { "browserName": "webkit" }
    },
    {
      "name": "Mobile Chrome",
      "use": {
        "browserName": "chromium",
        "viewport": { "width": 375, "height": 667 }
      }
    }
  ]
}
```

## Best Practices

### 1. Test-Stabilität
- Verwende `data-testid` Attribute für stabile Selektoren
- Nutze Playwright's Auto-Wait Mechanismen
- Implementiere explizite Waits für dynamische Inhalte

### 2. Test-Daten Management
```csharp
public class TestDataBuilder
{
    public static async Task<User> CreateTestUserAsync(IPage page)
    {
        var user = new User
        {
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = "TestPassword123!",
            FirstName = "Test",
            LastName = "User"
        };
        
        // Create user via API for faster setup
        await CreateUserViaApiAsync(user);
        return user;
    }
    
    private static async Task CreateUserViaApiAsync(User user)
    {
        // Implementation depends on your API
    }
}
```

### 3. Parallele Ausführung
- Verwende isolierte Test-Daten
- Vermeide geteilte Ressourcen
- Nutze eindeutige Identifier

### 4. Fehlerbehandlung
```csharp
[Test]
public async Task RobustTest_WithRetryLogic()
{
    await Retry.DoAsync(async () =>
    {
        await Page.GotoAsync(TestUrl);
        await Page.Locator("button").ClickAsync();
        
        var result = await Page.Locator("[data-testid='result']").TextContentAsync();
        result.Should().Be("Expected Value");
    }, TimeSpan.FromSeconds(1), 3);
}
```

## Ausführung

### Lokal
```bash
# Alle UI-Tests
dotnet test Tests/UI/

# Spezifischer Browser
BROWSER=firefox dotnet test Tests/UI/

# Headless deaktivieren (für Debugging)
HEADLESS=false dotnet test Tests/UI/

# Mit Playwright CLI
playwright test
```

### CI/CD
```bash
# Mit PowerShell Script
.\Scripts\Run-UITests.ps1 -Browser "Chrome" -Headless

# Parallel über mehrere Browser
.\Scripts\Run-UITests.ps1 -Parallel -Screenshots -VideoRecording
```

### Debugging
```bash
# Debug-Modus mit Playwright Inspector
PWDEBUG=1 dotnet test Tests/UI/ --filter "TestMethod"

# Trace Viewer
playwright show-trace trace.zip
```

## Reporting

### HTML Report
Nach Test-Ausführung wird automatisch ein HTML-Report generiert:
```
playwright-report/
├── index.html          # Haupt-Report
├── screenshots/        # Failure-Screenshots
├── videos/             # Test-Videos
└── traces/             # Debug-Traces
```

### Integration in CI/CD
```yaml
- name: Upload Playwright Report
  uses: actions/upload-artifact@v3
  if: always()
  with:
    name: playwright-report
    path: playwright-report/
```