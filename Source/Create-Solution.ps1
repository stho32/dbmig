# .NET Solution Generator Script
# Creates a complete .NET solution with main project, business logic library, and test projects

param(
    [Parameter(Mandatory=$true)]
    [string]$SolutionName,
    
    [Parameter(Mandatory=$true)]
    [ValidateSet("Console", "WebApi", "BlazorServer")]
    [string]$ProjectType,
    
    [string]$Framework = "net8.0",
    [string]$OutputPath = ".",
    [switch]$UseTopLevelStatements = $true,
    [switch]$UseNullableContext = $true,
    [switch]$AddDockerSupport = $false,
    [switch]$AddSwaggerSupport = $true,
    [switch]$Verbose = $false
)

# Configuration
$ErrorActionPreference = "Stop"

# Helper Functions
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] [$Level] $Message" -ForegroundColor $(
        switch ($Level) {
            "ERROR" { "Red" }
            "WARNING" { "Yellow" }
            "SUCCESS" { "Green" }
            "INFO" { "Cyan" }
            "STEP" { "Magenta" }
            default { "White" }
        }
    )
}

function Test-Prerequisites {
    Write-Log "Checking prerequisites..." "INFO"
    
    # Check if dotnet is available
    try {
        $dotnetVersion = dotnet --version
        Write-Log "Using .NET version: $dotnetVersion" "INFO"
    }
    catch {
        Write-Log ".NET CLI not found. Please install .NET SDK." "ERROR"
        throw
    }
    
    # Check if target framework is available
    $availableFrameworks = dotnet --list-sdks
    Write-Log "Available .NET SDKs:" "INFO"
    $availableFrameworks | ForEach-Object { Write-Log "  $_" "INFO" }
}

function Create-SolutionStructure {
    param([string]$SolutionName, [string]$OutputPath)
    
    Write-Log "Creating solution structure..." "STEP"
    
    $solutionPath = Join-Path $OutputPath $SolutionName
    
    if (Test-Path $solutionPath) {
        $confirmation = Read-Host "Directory '$solutionPath' already exists. Continue? (y/N)"
        if ($confirmation -ne "y" -and $confirmation -ne "Y") {
            throw "Operation cancelled by user"
        }
    } else {
        New-Item -ItemType Directory -Path $solutionPath -Force | Out-Null
    }
    
    # Create subdirectories
    $directories = @(
        "src",
        "tests",
        "docs"
    )
    
    foreach ($dir in $directories) {
        $dirPath = Join-Path $solutionPath $dir
        if (-not (Test-Path $dirPath)) {
            New-Item -ItemType Directory -Path $dirPath -Force | Out-Null
            Write-Log "Created directory: $dir" "INFO"
        }
    }
    
    return $solutionPath
}

function Create-Solution {
    param([string]$SolutionName, [string]$SolutionPath)
    
    Write-Log "Creating solution file..." "STEP"
    
    Push-Location $SolutionPath
    try {
        dotnet new sln --name $SolutionName --force
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create solution"
        }
        
        Write-Log "Solution '$SolutionName.sln' created successfully" "SUCCESS"
    }
    finally {
        Pop-Location
    }
}

function Create-MainProject {
    param([string]$SolutionName, [string]$ProjectType, [string]$Framework, [string]$SolutionPath)
    
    Write-Log "Creating main project..." "STEP"
    
    $projectPath = Join-Path $SolutionPath "src\$SolutionName"
    
    Push-Location $SolutionPath
    try {
        $template = switch ($ProjectType) {
            "Console" { "console" }
            "WebApi" { "webapi" }
            "BlazorServer" { "blazorserver" }
        }
        
        $createArgs = @(
            "new", $template,
            "--name", $SolutionName,
            "--output", $projectPath,
            "--framework", $Framework,
            "--force"
        )
        
        if ($UseTopLevelStatements) {
            $createArgs += "--use-program-main", "false"
        }
        
        if ($ProjectType -eq "WebApi" -and -not $AddSwaggerSupport) {
            $createArgs += "--use-openapi", "false"
        }
        
        & dotnet $createArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create main project"
        }
        
        # Add project to solution
        dotnet sln add "src\$SolutionName\$SolutionName.csproj"
        
        Write-Log "Main project '$SolutionName' created successfully" "SUCCESS"
        
        # Configure project properties
        Configure-ProjectProperties -ProjectPath "$projectPath\$SolutionName.csproj" -SolutionName $SolutionName
        
        return $projectPath
    }
    finally {
        Pop-Location
    }
}

function Create-BusinessLogicLibrary {
    param([string]$SolutionName, [string]$Framework, [string]$SolutionPath)
    
    Write-Log "Creating business logic library..." "STEP"
    
    $libraryName = "$SolutionName.BL"
    $libraryPath = Join-Path $SolutionPath "src\$libraryName"
    
    Push-Location $SolutionPath
    try {
        dotnet new classlib --name $libraryName --output $libraryPath --framework $Framework --force
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create business logic library"
        }
        
        # Add project to solution
        dotnet sln add "src\$libraryName\$libraryName.csproj"
        
        Write-Log "Business logic library '$libraryName' created successfully" "SUCCESS"
        
        # Configure project properties
        Configure-ProjectProperties -ProjectPath "$libraryPath\$libraryName.csproj" -SolutionName $SolutionName
        
        # Create initial folder structure
        Create-BusinessLogicStructure -LibraryPath $libraryPath
        
        return $libraryPath
    }
    finally {
        Pop-Location
    }
}

function Create-TestProjects {
    param([string]$SolutionName, [string]$Framework, [string]$SolutionPath)
    
    Write-Log "Creating test projects..." "STEP"
    
    # Create Unit Tests project
    $unitTestName = "$SolutionName.BL.Tests"
    $unitTestPath = Join-Path $SolutionPath "tests\$unitTestName"
    
    # Create Integration Tests project
    $integrationTestName = "$SolutionName.BL.IntegrationTests"
    $integrationTestPath = Join-Path $SolutionPath "tests\$integrationTestName"
    
    Push-Location $SolutionPath
    try {
        # Create unit test project
        dotnet new nunit --name $unitTestName --output $unitTestPath --framework $Framework --force
        dotnet sln add "tests\$unitTestName\$unitTestName.csproj"
        
        # Create integration test project
        dotnet new nunit --name $integrationTestName --output $integrationTestPath --framework $Framework --force
        dotnet sln add "tests\$integrationTestName\$integrationTestName.csproj"
        
        Write-Log "Test projects created successfully" "SUCCESS"
        
        # Configure test projects
        Configure-TestProject -TestProjectPath "$unitTestPath\$unitTestName.csproj" -SolutionName $SolutionName -TestType "Unit"
        Configure-TestProject -TestProjectPath "$integrationTestPath\$integrationTestName.csproj" -SolutionName $SolutionName -TestType "Integration"
        
        # Add project references
        Add-ProjectReferences -SolutionName $SolutionName -SolutionPath $SolutionPath
        
        return @($unitTestPath, $integrationTestPath)
    }
    finally {
        Pop-Location
    }
}

function Configure-ProjectProperties {
    param([string]$ProjectPath, [string]$SolutionName)
    
    Write-Log "Configuring project properties for $(Split-Path $ProjectPath -Leaf)..." "INFO"
    
    # Read current project file
    $projectContent = Get-Content $ProjectPath -Raw
    
    # Add common properties
    $propertyGroup = @"
  <PropertyGroup>
    <Company>$SolutionName</Company>
    <Product>$SolutionName</Product>
    <Authors>Development Team</Authors>
    <Copyright>Copyright © $(Get-Date -Format yyyy)</Copyright>
    <Version>1.0.0</Version>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
"@

    if ($UseNullableContext) {
        $propertyGroup += "`n    <Nullable>enable</Nullable>"
    }
    
    $propertyGroup += "`n    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>"
    $propertyGroup += "`n    <WarningsAsErrors />"
    $propertyGroup += "`n  </PropertyGroup>"
    
    # Insert property group after the first PropertyGroup
    $projectContent = $projectContent -replace '(<PropertyGroup>.*?</PropertyGroup>)', "`$1`n$propertyGroup"
    
    Set-Content -Path $ProjectPath -Value $projectContent -Encoding UTF8
}

function Configure-TestProject {
    param([string]$TestProjectPath, [string]$SolutionName, [string]$TestType)
    
    Write-Log "Configuring $TestType test project..." "INFO"
    
    # Read current project file
    $projectContent = Get-Content $TestProjectPath -Raw
    
    # Add test-specific packages based on type
    $testPackages = @"
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="NUnit" Version="3.14.0" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Moq" Version="4.20.69" />
"@

    if ($TestType -eq "Integration") {
        $testPackages += "`n    <PackageReference Include=`"Microsoft.AspNetCore.Mvc.Testing`" Version=`"8.0.0`" />"
        $testPackages += "`n    <PackageReference Include=`"Microsoft.EntityFrameworkCore.InMemory`" Version=`"8.0.0`" />"
    }
    
    $testPackages += "`n  </ItemGroup>"
    
    # Add test packages before closing Project tag
    $projectContent = $projectContent -replace '</Project>', "$testPackages`n</Project>"
    
    Set-Content -Path $TestProjectPath -Value $projectContent -Encoding UTF8
}

function Add-ProjectReferences {
    param([string]$SolutionName, [string]$SolutionPath)
    
    Write-Log "Adding project references..." "INFO"
    
    Push-Location $SolutionPath
    try {
        # Add BL reference to main project
        dotnet add "src\$SolutionName\$SolutionName.csproj" reference "src\$SolutionName.BL\$SolutionName.BL.csproj"
        
        # Add BL reference to test projects
        dotnet add "tests\$SolutionName.BL.Tests\$SolutionName.BL.Tests.csproj" reference "src\$SolutionName.BL\$SolutionName.BL.csproj"
        dotnet add "tests\$SolutionName.BL.IntegrationTests\$SolutionName.BL.IntegrationTests.csproj" reference "src\$SolutionName.BL\$SolutionName.BL.csproj"
        
        Write-Log "Project references added successfully" "SUCCESS"
    }
    finally {
        Pop-Location
    }
}

function Create-BusinessLogicStructure {
    param([string]$LibraryPath)
    
    Write-Log "Creating business logic folder structure..." "INFO"
    
    $folders = @(
        "Models",
        "Services", 
        "Interfaces",
        "Exceptions",
        "Extensions",
        "Validators"
    )
    
    foreach ($folder in $folders) {
        $folderPath = Join-Path $LibraryPath $folder
        if (-not (Test-Path $folderPath)) {
            New-Item -ItemType Directory -Path $folderPath -Force | Out-Null
        }
    }
    
    # Remove default Class1.cs
    $defaultFile = Join-Path $LibraryPath "Class1.cs"
    if (Test-Path $defaultFile) {
        Remove-Item $defaultFile -Force
    }
    
    # Create example interface and service
    Create-ExampleBusinessLogic -LibraryPath $LibraryPath -SolutionName (Split-Path $LibraryPath -Leaf).Replace('.BL', '')
}

function Create-ExampleBusinessLogic {
    param([string]$LibraryPath, [string]$SolutionName)
    
    # Create example interface
    $interfaceContent = @"
namespace $SolutionName.BL.Interfaces;

/// <summary>
/// Example service interface
/// </summary>
public interface IExampleService
{
    /// <summary>
    /// Gets a greeting message
    /// </summary>
    /// <param name="name">Name to greet</param>
    /// <returns>Greeting message</returns>
    string GetGreeting(string name);
    
    /// <summary>
    /// Validates if a name is valid
    /// </summary>
    /// <param name="name">Name to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool IsValidName(string name);
}
"@

    Set-Content -Path (Join-Path $LibraryPath "Interfaces\IExampleService.cs") -Value $interfaceContent -Encoding UTF8
    
    # Create example service implementation
    $serviceContent = @"
using $SolutionName.BL.Interfaces;
using $SolutionName.BL.Exceptions;

namespace $SolutionName.BL.Services;

/// <summary>
/// Example service implementation
/// </summary>
public class ExampleService : IExampleService
{
    /// <inheritdoc />
    public string GetGreeting(string name)
    {
        if (!IsValidName(name))
        {
            throw new ValidationException(`"Name cannot be null or empty`");
        }
        
        return `$`"Hello, {name}!`";
    }
    
    /// <inheritdoc />
    public bool IsValidName(string name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }
}
"@

    Set-Content -Path (Join-Path $LibraryPath "Services\ExampleService.cs") -Value $serviceContent -Encoding UTF8
    
    # Create example exception
    $exceptionContent = @"
namespace $SolutionName.BL.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : Exception
{
    public ValidationException() : base() { }
    
    public ValidationException(string message) : base(message) { }
    
    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
}
"@

    Set-Content -Path (Join-Path $LibraryPath "Exceptions\ValidationException.cs") -Value $exceptionContent -Encoding UTF8
}

function Create-ExampleTests {
    param([string]$UnitTestPath, [string]$IntegrationTestPath, [string]$SolutionName)
    
    Write-Log "Creating example tests..." "INFO"
    
    # Remove default test files
    $defaultUnitTest = Join-Path $UnitTestPath "UnitTest1.cs"
    $defaultIntegrationTest = Join-Path $IntegrationTestPath "UnitTest1.cs"
    
    if (Test-Path $defaultUnitTest) { Remove-Item $defaultUnitTest -Force }
    if (Test-Path $defaultIntegrationTest) { Remove-Item $defaultIntegrationTest -Force }
    
    # Create unit test example
    $unitTestContent = @"
using NUnit.Framework;
using FluentAssertions;
using $SolutionName.BL.Services;
using $SolutionName.BL.Exceptions;

namespace $SolutionName.BL.Tests;

[TestFixture]
public class ExampleServiceTests
{
    private ExampleService _service;
    
    [SetUp]
    public void Setup()
    {
        _service = new ExampleService();
    }
    
    [Test]
    public void GetGreeting_WithValidName_ReturnsCorrectGreeting()
    {
        // Arrange
        var name = "World";
        
        // Act
        var result = _service.GetGreeting(name);
        
        // Assert
        result.Should().Be("Hello, World!");
    }
    
    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void GetGreeting_WithInvalidName_ThrowsValidationException(string invalidName)
    {
        // Act & Assert
        _service.Invoking(s => s.GetGreeting(invalidName))
            .Should().Throw<ValidationException>()
            .WithMessage("Name cannot be null or empty");
    }
    
    [TestCase("John", true)]
    [TestCase("", false)]
    [TestCase(" ", false)]
    [TestCase(null, false)]
    public void IsValidName_ReturnsExpectedResult(string name, bool expected)
    {
        // Act
        var result = _service.IsValidName(name);
        
        // Assert
        result.Should().Be(expected);
    }
}
"@

    Set-Content -Path (Join-Path $UnitTestPath "ExampleServiceTests.cs") -Value $unitTestContent -Encoding UTF8
    
    # Create integration test example
    $integrationTestContent = @"
using NUnit.Framework;
using FluentAssertions;
using $SolutionName.BL.Services;
using $SolutionName.BL.Interfaces;

namespace $SolutionName.BL.IntegrationTests;

[TestFixture]
public class ExampleServiceIntegrationTests
{
    [Test]
    public void ExampleService_ImplementsInterface_Correctly()
    {
        // Arrange & Act
        IExampleService service = new ExampleService();
        
        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IExampleService>();
    }
    
    [Test]
    public void ExampleService_EndToEndTest_WorksCorrectly()
    {
        // Arrange
        var service = new ExampleService();
        var testName = "Integration Test";
        
        // Act
        var isValid = service.IsValidName(testName);
        var greeting = service.GetGreeting(testName);
        
        // Assert
        isValid.Should().BeTrue();
        greeting.Should().Be("Hello, Integration Test!");
    }
}
"@

    Set-Content -Path (Join-Path $IntegrationTestPath "ExampleServiceIntegrationTests.cs") -Value $integrationTestContent -Encoding UTF8
}

function Add-DockerSupport {
    param([string]$SolutionName, [string]$SolutionPath, [string]$ProjectType)
    
    if (-not $AddDockerSupport) {
        return
    }
    
    Write-Log "Adding Docker support..." "INFO"
    
    $dockerfileContent = switch ($ProjectType) {
        "Console" { Get-ConsoleDockerfile -SolutionName $SolutionName }
        "WebApi" { Get-WebApiDockerfile -SolutionName $SolutionName }
        "BlazorServer" { Get-BlazorDockerfile -SolutionName $SolutionName }
    }
    
    Set-Content -Path (Join-Path $SolutionPath "Dockerfile") -Value $dockerfileContent -Encoding UTF8
    
    # Create .dockerignore
    $dockerignoreContent = @"
**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.project
**/.settings
**/.toolstarget
**/.vs
**/.vscode
**/*.*proj.user
**/*.dbmdl
**/*.jfm
**/azds.yaml
**/bin
**/charts
**/docker-compose*
**/Dockerfile*
**/node_modules
**/npm-debug.log
**/obj
**/secrets.dev.yaml
**/values.dev.yaml
LICENSE
README.md
"@

    Set-Content -Path (Join-Path $SolutionPath ".dockerignore") -Value $dockerignoreContent -Encoding UTF8
    
    Write-Log "Docker support added successfully" "SUCCESS"
}

function Get-WebApiDockerfile {
    param([string]$SolutionName)
    
    return @"
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/$SolutionName/$SolutionName.csproj", "src/$SolutionName/"]
COPY ["src/$SolutionName.BL/$SolutionName.BL.csproj", "src/$SolutionName.BL/"]
RUN dotnet restore "src/$SolutionName/$SolutionName.csproj"
COPY . .
WORKDIR "/src/src/$SolutionName"
RUN dotnet build "$SolutionName.csproj" -c `$BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "$SolutionName.csproj" -c `$BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "$SolutionName.dll"]
"@
}

function Get-ConsoleDockerfile {
    param([string]$SolutionName)
    
    return @"
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/$SolutionName/$SolutionName.csproj", "src/$SolutionName/"]
COPY ["src/$SolutionName.BL/$SolutionName.BL.csproj", "src/$SolutionName.BL/"]
RUN dotnet restore "src/$SolutionName/$SolutionName.csproj"
COPY . .
WORKDIR "/src/src/$SolutionName"
RUN dotnet build "$SolutionName.csproj" -c `$BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "$SolutionName.csproj" -c `$BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "$SolutionName.dll"]
"@
}

function Get-BlazorDockerfile {
    param([string]$SolutionName)
    
    return Get-WebApiDockerfile -SolutionName $SolutionName
}

function Generate-SolutionSummary {
    param([string]$SolutionName, [string]$ProjectType, [string]$SolutionPath)
    
    Write-Log "=================== SOLUTION SUMMARY ===================" "SUMMARY"
    Write-Log "Solution Name: $SolutionName" "SUCCESS"
    Write-Log "Project Type: $ProjectType" "SUCCESS"
    Write-Log "Framework: $Framework" "SUCCESS"
    Write-Log "Location: $SolutionPath" "SUCCESS"
    Write-Log "" "INFO"
    Write-Log "Created Projects:" "INFO"
    Write-Log "  ✓ $SolutionName (Main $ProjectType project)" "SUCCESS"
    Write-Log "  ✓ $SolutionName.BL (Business Logic Library)" "SUCCESS"
    Write-Log "  ✓ $SolutionName.BL.Tests (Unit Tests)" "SUCCESS"
    Write-Log "  ✓ $SolutionName.BL.IntegrationTests (Integration Tests)" "SUCCESS"
    Write-Log "" "INFO"
    Write-Log "Next Steps:" "INFO"
    Write-Log "  1. cd `"$SolutionPath`"" "INFO"
    Write-Log "  2. dotnet restore" "INFO"
    Write-Log "  3. dotnet build" "INFO"
    Write-Log "  4. dotnet test" "INFO"
    
    if ($ProjectType -ne "Console") {
        Write-Log "  5. dotnet run --project src\$SolutionName" "INFO"
    }
    
    Write-Log "=========================================================" "SUMMARY"
}

# Main Script Logic
try {
    Write-Log "Starting .NET solution creation..." "INFO"
    Write-Log "Solution: $SolutionName, Type: $ProjectType, Framework: $Framework" "INFO"
    
    # Check prerequisites
    Test-Prerequisites
    
    # Create solution structure
    $solutionPath = Create-SolutionStructure -SolutionName $SolutionName -OutputPath $OutputPath
    
    # Create solution file
    Create-Solution -SolutionName $SolutionName -SolutionPath $solutionPath
    
    # Create main project
    $mainProjectPath = Create-MainProject -SolutionName $SolutionName -ProjectType $ProjectType -Framework $Framework -SolutionPath $solutionPath
    
    # Create business logic library
    $blLibraryPath = Create-BusinessLogicLibrary -SolutionName $SolutionName -Framework $Framework -SolutionPath $solutionPath
    
    # Create test projects
    $testPaths = Create-TestProjects -SolutionName $SolutionName -Framework $Framework -SolutionPath $solutionPath
    
    # Create example tests
    Create-ExampleTests -UnitTestPath $testPaths[0] -IntegrationTestPath $testPaths[1] -SolutionName $SolutionName
    
    # Add Docker support if requested
    Add-DockerSupport -SolutionName $SolutionName -SolutionPath $solutionPath -ProjectType $ProjectType
    
    # Generate summary
    Generate-SolutionSummary -SolutionName $SolutionName -ProjectType $ProjectType -SolutionPath $solutionPath
    
    Write-Log "Solution created successfully!" "SUCCESS"
    exit 0
}
catch {
    Write-Log "Failed to create solution: $_" "ERROR"
    exit 1
}

# Usage Examples:
# .\Create-Solution.ps1 -SolutionName "MyApp" -ProjectType "WebApi"
# .\Create-Solution.ps1 -SolutionName "MyConsoleApp" -ProjectType "Console" -Framework "net8.0"
# .\Create-Solution.ps1 -SolutionName "MyBlazorApp" -ProjectType "BlazorServer" -AddDockerSupport
# .\Create-Solution.ps1 -SolutionName "MyApi" -ProjectType "WebApi" -OutputPath "C:\Projects" -Verbose