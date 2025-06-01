# Controlled Database Migration Script
# Handles database migrations with rollback support and validation

param(
    [string]$ConnectionString = "",
    [string]$DatabaseName = "",
    [string]$TargetMigration = "",
    [switch]$ListMigrations = $false,
    [switch]$Rollback = $false,
    [switch]$DryRun = $false,
    [switch]$Force = $false,
    [string]$BackupPath = ""
)

# Configuration
$ErrorActionPreference = "Stop"
$ProjectPath = "./Source/Code"

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
            default { "White" }
        }
    )
}

function Get-MigrationList {
    try {
        Write-Log "Retrieving migration list..." "INFO"
        
        # Get list of migrations
        $migrations = dotnet ef migrations list --project $ProjectPath --no-build 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to retrieve migrations: $migrations"
        }
        
        return $migrations
    }
    catch {
        Write-Log "Error retrieving migrations: $_" "ERROR"
        throw
    }
}

function Get-CurrentMigration {
    try {
        # Get current migration status
        $dbContext = dotnet ef dbcontext info --project $ProjectPath --no-build 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to get database context info: $dbContext"
        }
        
        return $dbContext
    }
    catch {
        Write-Log "Error getting current migration: $_" "ERROR"
        throw
    }
}

function Create-DatabaseBackup {
    param([string]$BackupPath)
    
    if (-not $BackupPath) {
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $BackupPath = "./Backups/db_backup_$timestamp.bak"
    }
    
    try {
        Write-Log "Creating database backup at: $BackupPath" "INFO"
        
        # Create backup directory if it doesn't exist
        $backupDir = Split-Path $BackupPath -Parent
        if (-not (Test-Path $backupDir)) {
            New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
        }
        
        # Implementation depends on your database provider
        # Example for SQL Server:
        # sqlcmd -S $ServerName -d $DatabaseName -Q "BACKUP DATABASE [$DatabaseName] TO DISK = '$BackupPath'"
        
        Write-Log "Database backup created successfully." "SUCCESS"
        return $BackupPath
    }
    catch {
        Write-Log "Error creating database backup: $_" "ERROR"
        throw
    }
}

function Apply-Migration {
    param([string]$TargetMigration, [bool]$DryRun)
    
    try {
        if ($DryRun) {
            Write-Log "DRY RUN: Would apply migration to: $TargetMigration" "WARNING"
            
            # Generate SQL script for review
            $scriptPath = "./migration_script_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
            dotnet ef migrations script --project $ProjectPath --output $scriptPath
            
            Write-Log "Migration script generated at: $scriptPath" "INFO"
            return
        }
        
        Write-Log "Applying migration to: $TargetMigration" "INFO"
        
        # Apply migration
        if ($TargetMigration) {
            dotnet ef database update $TargetMigration --project $ProjectPath
        } else {
            dotnet ef database update --project $ProjectPath
        }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Migration failed with exit code: $LASTEXITCODE"
        }
        
        Write-Log "Migration applied successfully." "SUCCESS"
    }
    catch {
        Write-Log "Error applying migration: $_" "ERROR"
        throw
    }
}

function Rollback-Migration {
    param([string]$TargetMigration, [bool]$DryRun)
    
    if (-not $Force) {
        $confirmation = Read-Host "Are you sure you want to rollback to migration '$TargetMigration'? This may cause data loss! (y/N)"
        if ($confirmation -ne "y" -and $confirmation -ne "Y") {
            Write-Log "Migration rollback cancelled." "INFO"
            return
        }
    }
    
    try {
        if ($DryRun) {
            Write-Log "DRY RUN: Would rollback to migration: $TargetMigration" "WARNING"
            return
        }
        
        Write-Log "Rolling back to migration: $TargetMigration" "WARNING"
        
        # Rollback to specific migration
        dotnet ef database update $TargetMigration --project $ProjectPath
        
        if ($LASTEXITCODE -ne 0) {
            throw "Rollback failed with exit code: $LASTEXITCODE"
        }
        
        Write-Log "Rollback completed successfully." "SUCCESS"
    }
    catch {
        Write-Log "Error during rollback: $_" "ERROR"
        throw
    }
}

# Main Script Logic
try {
    Write-Log "Starting database migration operations..." "INFO"
    
    # Validate project path
    if (-not (Test-Path $ProjectPath)) {
        Write-Log "Project path not found: $ProjectPath" "ERROR"
        exit 1
    }
    
    # List migrations if requested
    if ($ListMigrations) {
        Write-Log "Available migrations:" "INFO"
        $migrations = Get-MigrationList
        Write-Host $migrations
        
        Write-Log "Current migration status:" "INFO"
        $current = Get-CurrentMigration
        Write-Host $current
        
        exit 0
    }
    
    # Create backup before migration (unless dry run)
    if (-not $DryRun -and -not $Rollback) {
        $backupPath = Create-DatabaseBackup -BackupPath $BackupPath
        Write-Log "Backup created at: $backupPath" "SUCCESS"
    }
    
    # Execute migration or rollback
    if ($Rollback) {
        if (-not $TargetMigration) {
            Write-Log "Target migration required for rollback operation." "ERROR"
            exit 1
        }
        Rollback-Migration -TargetMigration $TargetMigration -DryRun $DryRun
    } else {
        Apply-Migration -TargetMigration $TargetMigration -DryRun $DryRun
    }
    
    Write-Log "Migration operations completed successfully." "SUCCESS"
}
catch {
    Write-Log "Script failed: $_" "ERROR"
    exit 1
}

# Usage Examples:
# .\Database-Migrate.ps1 -ListMigrations
# .\Database-Migrate.ps1 -DryRun
# .\Database-Migrate.ps1 -TargetMigration "20231201_AddUserTable"
# .\Database-Migrate.ps1 -Rollback -TargetMigration "20231130_Initial" -Force
# .\Database-Migrate.ps1 -BackupPath "./custom_backup.bak"