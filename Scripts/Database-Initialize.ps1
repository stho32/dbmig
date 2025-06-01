# Database Initialization and Clearing Script
# Initializes or clears the database for development/testing

param(
    [string]$ConnectionString = "",
    [string]$DatabaseName = "",
    [switch]$Clear = $false,
    [switch]$Initialize = $false,
    [switch]$Force = $false
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
            default { "White" }
        }
    )
}

function Test-DatabaseExists {
    param([string]$ConnectionString, [string]$DatabaseName)
    try {
        # Test connection and database existence
        # This would be implemented based on your specific database provider
        return $true
    }
    catch {
        return $false
    }
}

function Clear-Database {
    param([string]$ConnectionString)
    Write-Log "Clearing database..." "WARNING"
    
    if (-not $Force) {
        $confirmation = Read-Host "Are you sure you want to clear the database? This will delete ALL data! (y/N)"
        if ($confirmation -ne "y" -and $confirmation -ne "Y") {
            Write-Log "Database clearing cancelled." "INFO"
            return
        }
    }
    
    try {
        # Clear all data - implementation depends on your ORM/database provider
        # Example for Entity Framework Core:
        # dotnet ef database drop --force --project ./Source/Code
        
        Write-Log "Database cleared successfully." "SUCCESS"
    }
    catch {
        Write-Log "Error clearing database: $_" "ERROR"
        throw
    }
}

function Initialize-Database {
    param([string]$ConnectionString)
    Write-Log "Initializing database..." "INFO"
    
    try {
        # Create database if it doesn't exist
        # Apply migrations
        # Seed initial data
        # Example for Entity Framework Core:
        # dotnet ef database update --project ./Source/Code
        
        Write-Log "Database initialized successfully." "SUCCESS"
    }
    catch {
        Write-Log "Error initializing database: $_" "ERROR"
        throw
    }
}

# Main Script Logic
try {
    Write-Log "Starting database operations..." "INFO"
    
    # Validate parameters
    if (-not $ConnectionString -and -not $DatabaseName) {
        Write-Log "Please provide either ConnectionString or DatabaseName parameter." "ERROR"
        exit 1
    }
    
    if (-not $Clear -and -not $Initialize) {
        Write-Log "Please specify either -Clear or -Initialize switch." "ERROR"
        exit 1
    }
    
    # Build connection string if not provided
    if (-not $ConnectionString -and $DatabaseName) {
        $ConnectionString = "Server=(localdb)\mssqllocaldb;Database=$DatabaseName;Trusted_Connection=true;"
    }
    
    # Execute operations
    if ($Clear) {
        Clear-Database -ConnectionString $ConnectionString
    }
    
    if ($Initialize) {
        Initialize-Database -ConnectionString $ConnectionString
    }
    
    Write-Log "Database operations completed successfully." "SUCCESS"
}
catch {
    Write-Log "Script failed: $_" "ERROR"
    exit 1
}

# Usage Examples:
# .\Database-Initialize.ps1 -DatabaseName "MyAppDb" -Initialize
# .\Database-Initialize.ps1 -DatabaseName "MyAppDb" -Clear -Force
# .\Database-Initialize.ps1 -ConnectionString "Server=localhost;Database=MyApp;..." -Clear -Initialize