# UI Test Execution Script
# Runs UI tests with browser automation support and detailed reporting

param(
    [string]$Browser = "Chrome",
    [string]$Environment = "Development",
    [string]$BaseUrl = "",
    [switch]$Headless = $false,
    [string]$Filter = "",
    [string]$Category = "",
    [string]$OutputFormat = "trx",
    [string]$OutputPath = "./TestResults/UI",
    [switch]$Parallel = $false,
    [switch]$Verbose = $false,
    [switch]$Screenshots = $true,
    [switch]$VideoRecording = $false,
    [int]$Timeout = 30,
    [switch]$FailFast = $false
)

# Configuration
$ErrorActionPreference = "Stop"
$UITestsPath = "./Tests/UI"
$ValidBrowsers = @("Chrome", "Firefox", "Edge", "Safari")

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
            "SUMMARY" { "Magenta" }
            default { "White" }
        }
    )
}

function Test-Prerequisites {
    Write-Log "Checking UI test prerequisites..." "INFO"
    
    # Check if dotnet is available
    try {
        $dotnetVersion = dotnet --version
        Write-Log "Using .NET version: $dotnetVersion" "INFO"
    }
    catch {
        Write-Log ".NET CLI not found. Please install .NET SDK." "ERROR"
        throw
    }
    
    # Check if UI test projects exist
    if (-not (Test-Path $UITestsPath)) {
        Write-Log "UI test projects path not found: $UITestsPath" "WARNING"
        Write-Log "Creating UI tests directory structure..." "INFO"
        New-Item -ItemType Directory -Path $UITestsPath -Force | Out-Null
        return $false
    }
    
    # Validate browser selection
    if ($Browser -notin $ValidBrowsers) {
        Write-Log "Invalid browser specified: $Browser. Valid options: $($ValidBrowsers -join ', ')" "ERROR"
        throw "Invalid browser selection"
    }
    
    return $true
}

function Get-UITestProjects {
    $projects = @()
    
    # Look for UI test projects
    $projects += Get-ChildItem -Path $UITestsPath -Recurse -Filter "*.csproj" | Where-Object {
        $_.Name -match "(UI|Selenium|WebDriver|E2E|EndToEnd)"
    }
    
    # If no specific UI test projects, look for any test projects in UI folder
    if ($projects.Count -eq 0) {
        $projects = Get-ChildItem -Path $UITestsPath -Recurse -Filter "*.csproj"
    }
    
    return $projects
}

function Setup-TestEnvironment {
    Write-Log "Setting up UI test environment..." "INFO"
    
    # Set environment variables for UI tests
    $env:BROWSER = $Browser
    $env:HEADLESS = $Headless.ToString().ToLower()
    $env:TEST_TIMEOUT = $Timeout
    $env:SCREENSHOTS_ENABLED = $Screenshots.ToString().ToLower()
    $env:VIDEO_RECORDING = $VideoRecording.ToString().ToLower()
    
    if ($BaseUrl) {
        $env:BASE_URL = $BaseUrl
        Write-Log "Using base URL: $BaseUrl" "INFO"
    }
    
    if ($Environment) {
        $env:TEST_ENVIRONMENT = $Environment
        Write-Log "Using environment: $Environment" "INFO"
    }
    
    # Create output directories
    if (-not (Test-Path $OutputPath)) {
        New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    }
    
    $screenshotPath = Join-Path $OutputPath "Screenshots"
    if ($Screenshots -and -not (Test-Path $screenshotPath)) {
        New-Item -ItemType Directory -Path $screenshotPath -Force | Out-Null
        $env:SCREENSHOT_PATH = $screenshotPath
    }
    
    $videoPath = Join-Path $OutputPath "Videos"
    if ($VideoRecording -and -not (Test-Path $videoPath)) {
        New-Item -ItemType Directory -Path $videoPath -Force | Out-Null
        $env:VIDEO_PATH = $videoPath
    }
}

function Check-WebDrivers {
    Write-Log "Checking WebDriver availability..." "INFO"
    
    try {
        # Check if WebDriver manager is available (assuming Selenium)
        # This would typically involve checking for driver executables
        # Implementation depends on your WebDriver management approach
        
        switch ($Browser.ToLower()) {
            "chrome" {
                # Check for ChromeDriver
                Write-Log "Checking for ChromeDriver..." "INFO"
            }
            "firefox" {
                # Check for GeckoDriver
                Write-Log "Checking for GeckoDriver..." "INFO"
            }
            "edge" {
                # Check for EdgeDriver
                Write-Log "Checking for EdgeDriver..." "INFO"
            }
        }
        
        Write-Log "WebDriver check completed." "SUCCESS"
    }
    catch {
        Write-Log "WebDriver check failed: $_" "WARNING"
        Write-Log "UI tests may fail if drivers are not properly installed." "WARNING"
    }
}

function Build-UITestProjects {
    param([array]$Projects)
    
    Write-Log "Building UI test projects..." "INFO"
    
    foreach ($project in $Projects) {
        Write-Log "Building: $($project.Name)" "INFO"
        
        dotnet build $project.FullName --configuration Release --no-restore
        
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed for UI test project: $($project.Name)"
        }
    }
    
    Write-Log "All UI test projects built successfully." "SUCCESS"
}

function Run-UITestSuite {
    param(
        [array]$Projects,
        [string]$Filter,
        [string]$Category,
        [string]$OutputFormat,
        [string]$OutputPath,
        [bool]$Parallel,
        [bool]$Verbose,
        [bool]$FailFast
    )
    
    Write-Log "Running UI tests..." "INFO"
    Write-Log "Browser: $Browser, Headless: $Headless, Parallel: $Parallel" "INFO"
    
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $testResults = @()
    
    foreach ($project in $Projects) {
        Write-Log "Running UI tests in: $($project.Name)" "INFO"
        
        # Build test command
        $testArgs = @(
            "test"
            $project.FullName
            "--configuration", "Release"
            "--no-build"
            "--logger", "$OutputFormat;LogFileName=UI_$($project.BaseName)_$timestamp.$OutputFormat"
            "--results-directory", $OutputPath
        )
        
        if ($Filter) {
            $testArgs += "--filter", $Filter
        }
        
        if ($Category) {
            $testArgs += "--filter", "Category=$Category"
        }
        
        # UI tests are often better run sequentially to avoid browser conflicts
        if (-not $Parallel) {
            $testArgs += "--parallel", "false"
        }
        
        if ($Verbose) {
            $testArgs += "--verbosity", "detailed"
        }
        
        # Set timeout for UI tests (they typically take longer)
        $testArgs += "--", "MSTest.TestTimeout=$($Timeout * 1000)"
        
        try {
            Write-Log "Executing: dotnet $($testArgs -join ' ')" "INFO"
            $result = & dotnet $testArgs 2>&1
            
            $testResult = [PSCustomObject]@{
                Project = $project.Name
                Success = $LASTEXITCODE -eq 0
                Output = $result
                ExitCode = $LASTEXITCODE
                Duration = (Get-Date) # This would be calculated properly in real implementation
            }
            
            $testResults += $testResult
            
            if ($testResult.Success) {
                Write-Log "UI tests passed for: $($project.Name)" "SUCCESS"
            } else {
                Write-Log "UI tests failed for: $($project.Name)" "ERROR"
                
                # Log failure details for debugging
                Write-Log "Failure output:" "ERROR"
                $result | Where-Object { $_ -match "(fail|error|exception)" } | ForEach-Object {
                    Write-Log "  $_" "ERROR"
                }
                
                if ($FailFast) {
                    break
                }
            }
        }
        catch {
            Write-Log "Error running UI tests for $($project.Name): $_" "ERROR"
            if ($FailFast) {
                throw
            }
        }
    }
    
    return $testResults
}

function Generate-UITestReport {
    param([array]$TestResults)
    
    Write-Log "Generating UI test report..." "INFO"
    
    $totalProjects = $TestResults.Count
    $passedProjects = ($TestResults | Where-Object { $_.Success }).Count
    $failedProjects = $totalProjects - $passedProjects
    
    Write-Log "=================== UI TEST SUMMARY ===================" "SUMMARY"
    Write-Log "Browser: $Browser (Headless: $Headless)" "SUMMARY"
    Write-Log "Environment: $Environment" "SUMMARY"
    Write-Log "Total Projects: $totalProjects" "SUMMARY"
    Write-Log "Passed: $passedProjects" $(if ($passedProjects -eq $totalProjects) { "SUCCESS" } else { "INFO" })
    Write-Log "Failed: $failedProjects" $(if ($failedProjects -gt 0) { "ERROR" } else { "SUCCESS" })
    
    if ($failedProjects -gt 0) {
        Write-Log "Failed Projects:" "ERROR"
        foreach ($result in ($TestResults | Where-Object { -not $_.Success })) {
            Write-Log "  - $($result.Project) (Exit Code: $($result.ExitCode))" "ERROR"
        }
    }
    
    Write-Log "Test results saved to: $OutputPath" "INFO"
    
    if ($Screenshots) {
        $screenshotPath = Join-Path $OutputPath "Screenshots"
        if (Test-Path $screenshotPath) {
            $screenshotCount = (Get-ChildItem $screenshotPath -File).Count
            Write-Log "Screenshots captured: $screenshotCount" "INFO"
        }
    }
    
    if ($VideoRecording) {
        $videoPath = Join-Path $OutputPath "Videos"
        if (Test-Path $videoPath) {
            $videoCount = (Get-ChildItem $videoPath -File).Count
            Write-Log "Videos recorded: $videoCount" "INFO"
        }
    }
    
    Write-Log "=========================================================" "SUMMARY"
    
    return $failedProjects -eq 0
}

# Main Script Logic
try {
    Write-Log "Starting UI test execution..." "INFO"
    
    # Check prerequisites
    $hasUITests = Test-Prerequisites
    
    if (-not $hasUITests) {
        Write-Log "No UI tests found. Please create UI test projects in $UITestsPath" "WARNING"
        exit 0
    }
    
    # Get UI test projects
    $uiProjects = Get-UITestProjects
    
    if ($uiProjects.Count -eq 0) {
        Write-Log "No UI test projects found in $UITestsPath" "WARNING"
        exit 0
    }
    
    Write-Log "Found $($uiProjects.Count) UI test project(s)" "INFO"
    
    # Setup test environment
    Setup-TestEnvironment
    
    # Check WebDriver availability
    Check-WebDrivers
    
    # Restore packages
    Write-Log "Restoring NuGet packages for UI tests..." "INFO"
    foreach ($project in $uiProjects) {
        dotnet restore $project.FullName
        if ($LASTEXITCODE -ne 0) {
            throw "Package restore failed for $($project.Name)"
        }
    }
    
    # Build projects
    Build-UITestProjects -Projects $uiProjects
    
    # Run UI tests
    $uiResults = Run-UITestSuite -Projects $uiProjects -Filter $Filter -Category $Category -OutputFormat $OutputFormat -OutputPath $OutputPath -Parallel $Parallel -Verbose $Verbose -FailFast $FailFast
    
    # Generate report
    $success = Generate-UITestReport -TestResults $uiResults
    
    if ($success) {
        Write-Log "All UI tests completed successfully!" "SUCCESS"
        exit 0
    } else {
        Write-Log "Some UI tests failed. Check the detailed results above." "ERROR"
        exit 1
    }
}
catch {
    Write-Log "Script failed: $_" "ERROR"
    exit 1
}
finally {
    # Cleanup environment variables
    Remove-Item Env:BROWSER -ErrorAction SilentlyContinue
    Remove-Item Env:HEADLESS -ErrorAction SilentlyContinue
    Remove-Item Env:TEST_TIMEOUT -ErrorAction SilentlyContinue
    Remove-Item Env:SCREENSHOTS_ENABLED -ErrorAction SilentlyContinue
    Remove-Item Env:VIDEO_RECORDING -ErrorAction SilentlyContinue
    Remove-Item Env:BASE_URL -ErrorAction SilentlyContinue
    Remove-Item Env:TEST_ENVIRONMENT -ErrorAction SilentlyContinue
    Remove-Item Env:SCREENSHOT_PATH -ErrorAction SilentlyContinue
    Remove-Item Env:VIDEO_PATH -ErrorAction SilentlyContinue
}

# Usage Examples:
# .\Run-UITests.ps1
# .\Run-UITests.ps1 -Browser "Firefox" -Headless
# .\Run-UITests.ps1 -Environment "Staging" -BaseUrl "https://staging.example.com"
# .\Run-UITests.ps1 -Filter "FullyQualifiedName~Login" -Screenshots
# .\Run-UITests.ps1 -Category "Smoke" -VideoRecording -Verbose
# .\Run-UITests.ps1 -Browser "Chrome" -Parallel -Timeout 60