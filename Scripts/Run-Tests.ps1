# Test Execution Script for Unit and Integration Tests
# Runs all unit tests and integration tests with detailed reporting

param(
    [switch]$UnitTests = $false,
    [switch]$IntegrationTests = $false,
    [switch]$All = $false,
    [string]$Filter = "",
    [string]$Category = "",
    [switch]$Coverage = $false,
    [string]$OutputFormat = "trx",
    [string]$OutputPath = "./TestResults",
    [switch]$Parallel = $true,
    [switch]$Verbose = $false,
    [switch]$FailFast = $false
)

# Configuration
$ErrorActionPreference = "Stop"
$TestProjectsPath = "./Tests"
$SourcePath = "./Source/Code"

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
    
    # Check if test projects exist
    if (-not (Test-Path $TestProjectsPath)) {
        Write-Log "Test projects path not found: $TestProjectsPath" "ERROR"
        throw "Test projects directory not found"
    }
}

function Get-TestProjects {
    param([string]$TestType)
    
    $projects = @()
    
    switch ($TestType) {
        "Unit" {
            $projects = Get-ChildItem -Path $TestProjectsPath -Recurse -Filter "*Unit*.csproj"
            $projects += Get-ChildItem -Path $TestProjectsPath -Recurse -Filter "*UnitTest*.csproj"
        }
        "Integration" {
            $projects = Get-ChildItem -Path $TestProjectsPath -Recurse -Filter "*Integration*.csproj"
            $projects += Get-ChildItem -Path $TestProjectsPath -Recurse -Filter "*IntegrationTest*.csproj"
        }
        "All" {
            $projects = Get-ChildItem -Path $TestProjectsPath -Recurse -Filter "*.csproj"
            # Exclude UI test projects (handled separately)
            $projects = $projects | Where-Object { $_.Name -notmatch "UI|Selenium|WebDriver" }
        }
    }
    
    return $projects
}

function Build-TestProjects {
    param([array]$Projects)
    
    Write-Log "Building test projects..." "INFO"
    
    foreach ($project in $Projects) {
        Write-Log "Building: $($project.Name)" "INFO"
        
        dotnet build $project.FullName --configuration Release --no-restore
        
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed for project: $($project.Name)"
        }
    }
    
    Write-Log "All test projects built successfully." "SUCCESS"
}

function Run-TestSuite {
    param(
        [array]$Projects,
        [string]$TestType,
        [string]$Filter,
        [string]$Category,
        [bool]$Coverage,
        [string]$OutputFormat,
        [string]$OutputPath,
        [bool]$Parallel,
        [bool]$Verbose,
        [bool]$FailFast
    )
    
    Write-Log "Running $TestType tests..." "INFO"
    
    # Prepare output directory
    if (-not (Test-Path $OutputPath)) {
        New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    }
    
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $testResults = @()
    
    foreach ($project in $Projects) {
        Write-Log "Running tests in: $($project.Name)" "INFO"
        
        # Build test command
        $testArgs = @(
            "test"
            $project.FullName
            "--configuration", "Release"
            "--no-build"
            "--logger", "$OutputFormat;LogFileName=$($project.BaseName)_$timestamp.$OutputFormat"
            "--results-directory", $OutputPath
        )
        
        if ($Filter) {
            $testArgs += "--filter", $Filter
        }
        
        if ($Category) {
            $testArgs += "--filter", "Category=$Category"
        }
        
        if ($Coverage) {
            $testArgs += "--collect", "XPlat Code Coverage"
        }
        
        if (-not $Parallel) {
            $testArgs += "--parallel", "false"
        }
        
        if ($Verbose) {
            $testArgs += "--verbosity", "detailed"
        }
        
        if ($FailFast) {
            $testArgs += "--", "MSTest.DeploymentEnabled=false"
        }
        
        try {
            $result = & dotnet $testArgs 2>&1
            
            $testResult = [PSCustomObject]@{
                Project = $project.Name
                Success = $LASTEXITCODE -eq 0
                Output = $result
                ExitCode = $LASTEXITCODE
            }
            
            $testResults += $testResult
            
            if ($testResult.Success) {
                Write-Log "Tests passed for: $($project.Name)" "SUCCESS"
            } else {
                Write-Log "Tests failed for: $($project.Name)" "ERROR"
                if ($FailFast) {
                    break
                }
            }
        }
        catch {
            Write-Log "Error running tests for $($project.Name): $_" "ERROR"
            if ($FailFast) {
                throw
            }
        }
    }
    
    return $testResults
}

function Generate-TestReport {
    param([array]$TestResults, [string]$TestType)
    
    Write-Log "Generating test report for $TestType tests..." "INFO"
    
    $totalProjects = $TestResults.Count
    $passedProjects = ($TestResults | Where-Object { $_.Success }).Count
    $failedProjects = $totalProjects - $passedProjects
    
    Write-Log "=================== TEST SUMMARY ===================" "SUMMARY"
    Write-Log "Test Type: $TestType" "SUMMARY"
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
    Write-Log "====================================================" "SUMMARY"
    
    return $failedProjects -eq 0
}

# Main Script Logic
try {
    Write-Log "Starting test execution..." "INFO"
    
    # Validate parameters
    if (-not $UnitTests -and -not $IntegrationTests -and -not $All) {
        $All = $true
        Write-Log "No specific test type specified, running all tests..." "INFO"
    }
    
    # Check prerequisites
    Test-Prerequisites
    
    # Restore packages first
    Write-Log "Restoring NuGet packages..." "INFO"
    dotnet restore $SourcePath
    if ($LASTEXITCODE -ne 0) {
        throw "Package restore failed"
    }
    
    $overallSuccess = $true
    
    # Run Unit Tests
    if ($UnitTests -or $All) {
        $unitProjects = Get-TestProjects -TestType "Unit"
        
        if ($unitProjects.Count -eq 0) {
            Write-Log "No unit test projects found." "WARNING"
        } else {
            Build-TestProjects -Projects $unitProjects
            $unitResults = Run-TestSuite -Projects $unitProjects -TestType "Unit" -Filter $Filter -Category $Category -Coverage $Coverage -OutputFormat $OutputFormat -OutputPath "$OutputPath/Unit" -Parallel $Parallel -Verbose $Verbose -FailFast $FailFast
            $unitSuccess = Generate-TestReport -TestResults $unitResults -TestType "Unit"
            $overallSuccess = $overallSuccess -and $unitSuccess
        }
    }
    
    # Run Integration Tests
    if ($IntegrationTests -or $All) {
        $integrationProjects = Get-TestProjects -TestType "Integration"
        
        if ($integrationProjects.Count -eq 0) {
            Write-Log "No integration test projects found." "WARNING"
        } else {
            Build-TestProjects -Projects $integrationProjects
            $integrationResults = Run-TestSuite -Projects $integrationProjects -TestType "Integration" -Filter $Filter -Category $Category -Coverage $Coverage -OutputFormat $OutputFormat -OutputPath "$OutputPath/Integration" -Parallel $Parallel -Verbose $Verbose -FailFast $FailFast
            $integrationSuccess = Generate-TestReport -TestResults $integrationResults -TestType "Integration"
            $overallSuccess = $overallSuccess -and $integrationSuccess
        }
    }
    
    if ($overallSuccess) {
        Write-Log "All tests completed successfully!" "SUCCESS"
        exit 0
    } else {
        Write-Log "Some tests failed. Check the detailed results above." "ERROR"
        exit 1
    }
}
catch {
    Write-Log "Script failed: $_" "ERROR"
    exit 1
}

# Usage Examples:
# .\Run-Tests.ps1 -All
# .\Run-Tests.ps1 -UnitTests -Coverage
# .\Run-Tests.ps1 -IntegrationTests -Filter "FullyQualifiedName~Calculator"
# .\Run-Tests.ps1 -All -Category "Smoke" -Verbose
# .\Run-Tests.ps1 -UnitTests -FailFast -OutputFormat "junit"