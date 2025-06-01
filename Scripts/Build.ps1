# Solution Build Script
# Comprehensive build script for .NET solutions with multiple configurations and validation

param(
    [string]$Configuration = "Release",
    [string]$Platform = "Any CPU",
    [string]$SolutionPath = "",
    [switch]$Clean = $false,
    [switch]$Restore = $true,
    [switch]$NoBuild = $false,
    [switch]$Publish = $false,
    [string]$PublishPath = "./Publish",
    [string]$Runtime = "",
    [switch]$SelfContained = $false,
    [switch]$Verbose = $false,
    [switch]$NoLogo = $true,
    [switch]$WarningsAsErrors = $false,
    [string]$Framework = "",
    [switch]$SkipTests = $false,
    [switch]$CodeAnalysis = $true,
    [string]$OutputPath = ""
)

# Configuration
$ErrorActionPreference = "Stop"
$SourcePath = "./Source"
$ValidConfigurations = @("Debug", "Release")
$ValidPlatforms = @("Any CPU", "x86", "x64", "ARM", "ARM64")

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
    Write-Log "Checking build prerequisites..." "INFO"
    
    # Check if dotnet is available
    try {
        $dotnetVersion = dotnet --version
        Write-Log "Using .NET version: $dotnetVersion" "INFO"
    }
    catch {
        Write-Log ".NET CLI not found. Please install .NET SDK." "ERROR"
        throw
    }
    
    # Display SDK info if verbose
    if ($Verbose) {
        Write-Log "Available .NET SDKs:" "INFO"
        dotnet --list-sdks | ForEach-Object { Write-Log "  $_" "INFO" }
    }
}

function Find-SolutionFile {
    if ($SolutionPath) {
        if (Test-Path $SolutionPath) {
            return $SolutionPath
        } else {
            throw "Solution file not found: $SolutionPath"
        }
    }
    
    # Look for solution files in common locations
    $solutionFiles = @()
    
    # Check root directory
    $solutionFiles += Get-ChildItem -Path "." -Filter "*.sln" -ErrorAction SilentlyContinue
    
    # Check Source directory
    if (Test-Path $SourcePath) {
        $solutionFiles += Get-ChildItem -Path $SourcePath -Filter "*.sln" -Recurse -ErrorAction SilentlyContinue
    }
    
    if ($solutionFiles.Count -eq 0) {
        Write-Log "No solution file found. Looking for project files..." "WARNING"
        return $null
    } elseif ($solutionFiles.Count -eq 1) {
        Write-Log "Found solution file: $($solutionFiles[0].FullName)" "INFO"
        return $solutionFiles[0].FullName
    } else {
        Write-Log "Multiple solution files found:" "WARNING"
        $solutionFiles | ForEach-Object { Write-Log "  $($_.FullName)" "WARNING" }
        Write-Log "Using first solution file: $($solutionFiles[0].FullName)" "INFO"
        return $solutionFiles[0].FullName
    }
}

function Find-ProjectFiles {
    Write-Log "Looking for project files..." "INFO"
    
    $projectFiles = @()
    
    if (Test-Path $SourcePath) {
        $projectFiles = Get-ChildItem -Path $SourcePath -Filter "*.csproj" -Recurse
    } else {
        $projectFiles = Get-ChildItem -Path "." -Filter "*.csproj" -Recurse
    }
    
    Write-Log "Found $($projectFiles.Count) project file(s)" "INFO"
    
    if ($Verbose) {
        $projectFiles | ForEach-Object { Write-Log "  $($_.FullName)" "INFO" }
    }
    
    return $projectFiles
}

function Restore-Packages {
    param([string]$TargetPath)
    
    Write-Log "Restoring NuGet packages..." "INFO"
    
    $restoreArgs = @("restore")
    
    if ($TargetPath) {
        $restoreArgs += $TargetPath
    }
    
    if ($Verbose) {
        $restoreArgs += "--verbosity", "detailed"
    } elseif ($NoLogo) {
        $restoreArgs += "--verbosity", "minimal"
    }
    
    try {
        & dotnet $restoreArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "Package restore failed with exit code: $LASTEXITCODE"
        }
        
        Write-Log "Package restore completed successfully." "SUCCESS"
    }
    catch {
        Write-Log "Error during package restore: $_" "ERROR"
        throw
    }
}

function Clean-Solution {
    param([string]$TargetPath)
    
    Write-Log "Cleaning solution..." "INFO"
    
    $cleanArgs = @("clean")
    
    if ($TargetPath) {
        $cleanArgs += $TargetPath
    }
    
    $cleanArgs += "--configuration", $Configuration
    
    if ($Verbose) {
        $cleanArgs += "--verbosity", "detailed"
    } elseif ($NoLogo) {
        $cleanArgs += "--verbosity", "minimal"
    }
    
    try {
        & dotnet $cleanArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "Clean failed with exit code: $LASTEXITCODE"
        }
        
        Write-Log "Clean completed successfully." "SUCCESS"
    }
    catch {
        Write-Log "Error during clean: $_" "ERROR"
        throw
    }
}

function Build-Solution {
    param([string]$TargetPath)
    
    Write-Log "Building solution..." "INFO"
    Write-Log "Configuration: $Configuration, Platform: $Platform" "INFO"
    
    $buildArgs = @("build")
    
    if ($TargetPath) {
        $buildArgs += $TargetPath
    }
    
    $buildArgs += "--configuration", $Configuration
    
    if (-not $Restore) {
        $buildArgs += "--no-restore"
    }
    
    if ($Framework) {
        $buildArgs += "--framework", $Framework
    }
    
    if ($Runtime) {
        $buildArgs += "--runtime", $Runtime
    }
    
    if ($OutputPath) {
        $buildArgs += "--output", $OutputPath
    }
    
    if ($WarningsAsErrors) {
        $buildArgs += "--warnaserror"
    }
    
    if ($NoLogo) {
        $buildArgs += "--nologo"
    }
    
    if ($Verbose) {
        $buildArgs += "--verbosity", "detailed"
    } else {
        $buildArgs += "--verbosity", "minimal"
    }
    
    try {
        Write-Log "Executing: dotnet $($buildArgs -join ' ')" "INFO"
        $buildOutput = & dotnet $buildArgs 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Log "Build output:" "ERROR"
            $buildOutput | ForEach-Object { Write-Log "  $_" "ERROR" }
            throw "Build failed with exit code: $LASTEXITCODE"
        }
        
        if ($Verbose) {
            $buildOutput | ForEach-Object { Write-Log "  $_" "INFO" }
        }
        
        # Parse build output for warnings and errors
        $warnings = $buildOutput | Where-Object { $_ -match "warning" }
        $errors = $buildOutput | Where-Object { $_ -match "error" }
        
        if ($warnings.Count -gt 0) {
            Write-Log "$($warnings.Count) warning(s) found" "WARNING"
            if ($Verbose) {
                $warnings | ForEach-Object { Write-Log "  $_" "WARNING" }
            }
        }
        
        if ($errors.Count -gt 0) {
            Write-Log "$($errors.Count) error(s) found" "ERROR"
            $errors | ForEach-Object { Write-Log "  $_" "ERROR" }
            throw "Build completed with errors"
        }
        
        Write-Log "Build completed successfully." "SUCCESS"
    }
    catch {
        Write-Log "Error during build: $_" "ERROR"
        throw
    }
}

function Publish-Solution {
    param([string]$TargetPath)
    
    Write-Log "Publishing solution..." "INFO"
    
    if (-not (Test-Path $PublishPath)) {
        New-Item -ItemType Directory -Path $PublishPath -Force | Out-Null
    }
    
    $publishArgs = @("publish")
    
    if ($TargetPath) {
        $publishArgs += $TargetPath
    }
    
    $publishArgs += "--configuration", $Configuration
    $publishArgs += "--output", $PublishPath
    
    if ($Runtime) {
        $publishArgs += "--runtime", $Runtime
    }
    
    if ($SelfContained) {
        $publishArgs += "--self-contained", "true"
    } else {
        $publishArgs += "--self-contained", "false"
    }
    
    if ($Framework) {
        $publishArgs += "--framework", $Framework
    }
    
    if ($NoLogo) {
        $publishArgs += "--nologo"
    }
    
    if ($Verbose) {
        $publishArgs += "--verbosity", "detailed"
    } else {
        $publishArgs += "--verbosity", "minimal"
    }
    
    try {
        Write-Log "Executing: dotnet $($publishArgs -join ' ')" "INFO"
        & dotnet $publishArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "Publish failed with exit code: $LASTEXITCODE"
        }
        
        Write-Log "Publish completed successfully." "SUCCESS"
        Write-Log "Published to: $PublishPath" "INFO"
    }
    catch {
        Write-Log "Error during publish: $_" "ERROR"
        throw
    }
}

function Run-CodeAnalysis {
    param([string]$TargetPath)
    
    if (-not $CodeAnalysis) {
        return
    }
    
    Write-Log "Running code analysis..." "INFO"
    
    try {
        # Run built-in analyzers
        $analyzeArgs = @("build")
        
        if ($TargetPath) {
            $analyzeArgs += $TargetPath
        }
        
        $analyzeArgs += "--configuration", $Configuration
        $analyzeArgs += "--no-restore"
        $analyzeArgs += "--verbosity", "normal"
        $analyzeArgs += "/p:RunAnalyzersDuringBuild=true"
        $analyzeArgs += "/p:RunCodeAnalysis=true"
        
        & dotnet $analyzeArgs
        
        if ($LASTEXITCODE -ne 0) {
            Write-Log "Code analysis found issues." "WARNING"
        } else {
            Write-Log "Code analysis completed successfully." "SUCCESS"
        }
    }
    catch {
        Write-Log "Error during code analysis: $_" "WARNING"
    }
}

function Generate-BuildSummary {
    param([string]$TargetPath, [datetime]$StartTime)
    
    $endTime = Get-Date
    $duration = $endTime - $StartTime
    
    Write-Log "=================== BUILD SUMMARY ===================" "SUMMARY"
    Write-Log "Target: $(if ($TargetPath) { $TargetPath } else { 'All Projects' })" "SUMMARY"
    Write-Log "Configuration: $Configuration" "SUMMARY"
    Write-Log "Platform: $Platform" "SUMMARY"
    Write-Log "Framework: $(if ($Framework) { $Framework } else { 'Default' })" "SUMMARY"
    Write-Log "Runtime: $(if ($Runtime) { $Runtime } else { 'Default' })" "SUMMARY"
    Write-Log "Duration: $($duration.ToString('mm\:ss'))" "SUMMARY"
    Write-Log "Status: SUCCESS" "SUCCESS"
    
    if ($Publish) {
        Write-Log "Published to: $PublishPath" "SUMMARY"
    }
    
    Write-Log "====================================================" "SUMMARY"
}

# Main Script Logic
try {
    $startTime = Get-Date
    Write-Log "Starting build process..." "INFO"
    
    # Validate parameters
    if ($Configuration -notin $ValidConfigurations) {
        Write-Log "Invalid configuration: $Configuration. Valid options: $($ValidConfigurations -join ', ')" "ERROR"
        exit 1
    }
    
    if ($Platform -notin $ValidPlatforms) {
        Write-Log "Invalid platform: $Platform. Valid options: $($ValidPlatforms -join ', ')" "ERROR"
        exit 1
    }
    
    # Check prerequisites
    Test-Prerequisites
    
    # Find target to build
    $targetPath = Find-SolutionFile
    
    if (-not $targetPath) {
        $projectFiles = Find-ProjectFiles
        if ($projectFiles.Count -eq 0) {
            Write-Log "No solution or project files found." "ERROR"
            exit 1
        }
        # Build each project individually if no solution
        $targetPath = $null
    }
    
    # Clean if requested
    if ($Clean) {
        Clean-Solution -TargetPath $targetPath
    }
    
    # Restore packages
    if ($Restore) {
        Restore-Packages -TargetPath $targetPath
    }
    
    # Build solution/projects
    if (-not $NoBuild) {
        Build-Solution -TargetPath $targetPath
        
        # Run code analysis
        Run-CodeAnalysis -TargetPath $targetPath
    }
    
    # Publish if requested
    if ($Publish) {
        Publish-Solution -TargetPath $targetPath
    }
    
    # Run tests if not skipped
    if (-not $SkipTests -and -not $NoBuild) {
        Write-Log "Running quick test validation..." "INFO"
        try {
            & "$(Split-Path $MyInvocation.MyCommand.Path)\Run-Tests.ps1" -UnitTests -FailFast
            if ($LASTEXITCODE -eq 0) {
                Write-Log "Test validation passed." "SUCCESS"
            } else {
                Write-Log "Test validation failed." "WARNING"
            }
        }
        catch {
            Write-Log "Could not run test validation: $_" "WARNING"
        }
    }
    
    # Generate summary
    Generate-BuildSummary -TargetPath $targetPath -StartTime $startTime
    
    Write-Log "Build process completed successfully!" "SUCCESS"
    exit 0
}
catch {
    Write-Log "Build failed: $_" "ERROR"
    exit 1
}

# Usage Examples:
# .\Build.ps1
# .\Build.ps1 -Configuration "Debug" -Clean
# .\Build.ps1 -Publish -PublishPath "./Release"
# .\Build.ps1 -SolutionPath "./Source/MyApp.sln" -Verbose
# .\Build.ps1 -Runtime "win-x64" -SelfContained -Framework "net8.0"
# .\Build.ps1 -Clean -Configuration "Release" -WarningsAsErrors -Publish