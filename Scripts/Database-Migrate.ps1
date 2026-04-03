# Database Migration Script for dbmig
# Uses the dbmig console application to run migrations

param(
    [Parameter(Mandatory = $true)]
    [string]$ConnectionString,
    [Parameter(Mandatory = $true)]
    [string]$MigrationDirectory,
    [string]$MigrationTableName = ""
)

$ErrorActionPreference = "Stop"

# Find the console project
$consolePath = "./Source/dbmig/dbmig.Console/dbmig.Console.csproj"

if (-not (Test-Path $consolePath)) {
    Write-Host "[ERROR] Console project not found at: $consolePath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $MigrationDirectory)) {
    Write-Host "[ERROR] Migration directory not found: $MigrationDirectory" -ForegroundColor Red
    exit 1
}

try {
    Write-Host "[INFO] Running migrations from: $MigrationDirectory" -ForegroundColor Cyan

    $migrateArgs = @("-c", $ConnectionString, "migrate", $MigrationDirectory)
    if ($MigrationTableName) {
        $migrateArgs += $MigrationTableName
    }

    dotnet run --project $consolePath -- @migrateArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Migration failed!" -ForegroundColor Red
        exit 1
    }

    Write-Host "[SUCCESS] Migrations completed successfully." -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] Script failed: $_" -ForegroundColor Red
    exit 1
}

# Usage Examples:
# .\Database-Migrate.ps1 -ConnectionString "Server=localhost;Database=MyDb;Integrated Security=true;TrustServerCertificate=true;" -MigrationDirectory "./Source/DBMigrations"
# .\Database-Migrate.ps1 -ConnectionString "Server=localhost;Database=MyDb;Integrated Security=true;TrustServerCertificate=true;" -MigrationDirectory "./Source/DBMigrations" -MigrationTableName "CustomMigrations"
