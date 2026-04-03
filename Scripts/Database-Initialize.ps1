# Database Initialization Script for dbmig
# Uses the dbmig console application to initialize the migration system

param(
    [Parameter(Mandatory = $true)]
    [string]$ConnectionString,
    [string]$MigrationTableName = "",
    [switch]$Clear = $false,
    [switch]$Initialize = $false,
    [switch]$Force = $false
)

$ErrorActionPreference = "Stop"

# Find the console project
$consolePath = "./Source/dbmig/dbmig.Console/dbmig.Console.csproj"

if (-not (Test-Path $consolePath)) {
    Write-Host "[ERROR] Console project not found at: $consolePath" -ForegroundColor Red
    exit 1
}

try {
    if ($Clear) {
        if (-not $Force) {
            $confirmation = Read-Host "Are you sure you want to clear the database? This will delete ALL data! (y/N)"
            if ($confirmation -ne "y" -and $confirmation -ne "Y") {
                Write-Host "[INFO] Database clearing cancelled." -ForegroundColor Yellow
                exit 0
            }
        }

        Write-Host "[INFO] Clearing database..." -ForegroundColor Yellow
        dotnet run --project $consolePath -- -c $ConnectionString cleardb

        if ($LASTEXITCODE -ne 0) {
            throw "Database clearing failed"
        }
        Write-Host "[SUCCESS] Database cleared." -ForegroundColor Green
    }

    if ($Initialize) {
        Write-Host "[INFO] Initializing migration system..." -ForegroundColor Yellow

        $initArgs = @("-c", $ConnectionString, "init")
        if ($MigrationTableName) {
            $initArgs += $MigrationTableName
        }

        dotnet run --project $consolePath -- @initArgs

        if ($LASTEXITCODE -ne 0) {
            throw "Migration initialization failed"
        }
        Write-Host "[SUCCESS] Migration system initialized." -ForegroundColor Green
    }

    if (-not $Clear -and -not $Initialize) {
        Write-Host "[ERROR] Please specify either -Clear or -Initialize switch." -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "[ERROR] Script failed: $_" -ForegroundColor Red
    exit 1
}

# Usage Examples:
# .\Database-Initialize.ps1 -ConnectionString "Server=localhost;Database=MyDb;Integrated Security=true;TrustServerCertificate=true;" -Initialize
# .\Database-Initialize.ps1 -ConnectionString "Server=localhost;Database=MyDb;Integrated Security=true;TrustServerCertificate=true;" -Clear -Force
# .\Database-Initialize.ps1 -ConnectionString "Server=localhost;Database=MyDb;Integrated Security=true;TrustServerCertificate=true;" -Clear -Initialize -Force
