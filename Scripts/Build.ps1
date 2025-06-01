# Simple Build Script for dbmig
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "Building dbmig solution..." -ForegroundColor Cyan

# Find solution file
$solutionPath = Get-ChildItem -Path "./Source/Code" -Filter "*.sln" | Select-Object -First 1

if (-not $solutionPath) {
    Write-Host "Error: No solution file found in ./Source/Code" -ForegroundColor Red
    exit 1
}

Write-Host "Found solution: $($solutionPath.FullName)" -ForegroundColor Green

try {
    # Restore packages
    Write-Host "Restoring packages..." -ForegroundColor Yellow
    dotnet restore $solutionPath.FullName
    
    if ($LASTEXITCODE -ne 0) {
        throw "Package restore failed"
    }
    
    # Build solution
    Write-Host "Building solution..." -ForegroundColor Yellow
    dotnet build $solutionPath.FullName --configuration $Configuration --no-restore
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    
    Write-Host "Build completed successfully!" -ForegroundColor Green
    exit 0
}
catch {
    Write-Host "Build failed: $_" -ForegroundColor Red
    exit 1
}