# Simple Test Script for dbmig
param(
    [switch]$UnitTests = $false,
    [switch]$IntegrationTests = $false,
    [switch]$All = $false
)

$ErrorActionPreference = "Stop"

Write-Host "Running dbmig tests..." -ForegroundColor Cyan

# Default to all tests if nothing specified
if (-not $UnitTests -and -not $IntegrationTests -and -not $All) {
    $All = $true
}

# Find solution file
$solutionPath = Get-ChildItem -Path "./Source/Code" -Filter "*.sln" | Select-Object -First 1

if (-not $solutionPath) {
    Write-Host "Error: No solution file found in ./Source/Code" -ForegroundColor Red
    exit 1
}

Write-Host "Found solution: $($solutionPath.FullName)" -ForegroundColor Green

try {
    $testFailed = $false
    
    if ($UnitTests -or $All) {
        Write-Host "Running Unit Tests..." -ForegroundColor Yellow
        $unitTestProjects = Get-ChildItem -Path "./Source/Code" -Filter "*Tests.csproj" -Recurse | Where-Object { $_.Name -notmatch "Integration" }
        
        foreach ($project in $unitTestProjects) {
            Write-Host "Testing: $($project.Name)" -ForegroundColor Cyan
            dotnet test $project.FullName --configuration Release --no-build
            
            if ($LASTEXITCODE -ne 0) {
                $testFailed = $true
                Write-Host "Unit tests failed in: $($project.Name)" -ForegroundColor Red
            }
        }
    }
    
    if ($IntegrationTests -or $All) {
        Write-Host "Running Integration Tests..." -ForegroundColor Yellow
        $integrationTestProjects = Get-ChildItem -Path "./Source/Code" -Filter "*IntegrationTests.csproj" -Recurse
        
        foreach ($project in $integrationTestProjects) {
            Write-Host "Testing: $($project.Name)" -ForegroundColor Cyan
            dotnet test $project.FullName --configuration Release --no-build
            
            if ($LASTEXITCODE -ne 0) {
                $testFailed = $true
                Write-Host "Integration tests failed in: $($project.Name)" -ForegroundColor Red
            }
        }
    }
    
    if ($testFailed) {
        Write-Host "Some tests failed!" -ForegroundColor Red
        exit 1
    } else {
        Write-Host "All tests passed!" -ForegroundColor Green
        exit 0
    }
}
catch {
    Write-Host "Test execution failed: $_" -ForegroundColor Red
    exit 1
}