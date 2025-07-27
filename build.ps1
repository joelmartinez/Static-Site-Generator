#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Build script for CodeCube Static Site Generator with modern frontend workflow
.DESCRIPTION
    This script orchestrates the complete build process:
    1. Installs frontend dependencies and bundles JavaScript
    2. Builds and runs the .NET static site generator
.NOTES
    Requires PowerShell Core, Node.js/NPM, and .NET 8
#>

param(
    [switch]$SkipFrontend = $false,
    [switch]$Verbose = $false
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Function to write colored output
function Write-BuildStep {
    param([string]$Message, [string]$Color = "Green")
    Write-Host ">>> $Message" -ForegroundColor $Color
}

function Write-BuildError {
    param([string]$Message)
    Write-Host "ERROR: $Message" -ForegroundColor Red
}

try {
    Write-BuildStep "Starting CodeCube build process..."
    
    # Get the script directory (repository root)
    $RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
    Set-Location $RepoRoot
    
    if (-not $SkipFrontend) {
        Write-BuildStep "Step 1: Building frontend assets..."
        
        # Check if Node.js and NPM are available
        if (-not (Get-Command "npm" -ErrorAction SilentlyContinue)) {
            throw "NPM is not available. Please install Node.js."
        }
        
        # Navigate to frontend directory
        Set-Location "$RepoRoot/frontend"
        
        # Install dependencies if node_modules doesn't exist or package.json is newer
        if (-not (Test-Path "node_modules") -or 
            (Get-Item "package.json").LastWriteTime -gt (Get-Item "node_modules" -ErrorAction SilentlyContinue).LastWriteTime) {
            Write-BuildStep "Installing NPM dependencies..."
            npm install
            if ($LASTEXITCODE -ne 0) { throw "NPM install failed" }
        }
        
        # Build frontend assets
        Write-BuildStep "Bundling frontend JavaScript..."
        npm run build
        if ($LASTEXITCODE -ne 0) { throw "Frontend build failed" }
        
        Write-BuildStep "Frontend build completed successfully"
        Set-Location $RepoRoot
    } else {
        Write-BuildStep "Skipping frontend build as requested" "Yellow"
    }
    
    Write-BuildStep "Step 2: Building .NET application..."
    
    # Check if .NET is available
    if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
        throw ".NET CLI is not available. Please install .NET 8."
    }
    
    # Build the .NET solution
    Write-BuildStep "Building CodeCubeConsole.sln..."
    dotnet build CodeCubeConsole.sln
    if ($LASTEXITCODE -ne 0) { throw ".NET build failed" }
    
    Write-BuildStep "Step 3: Running static site generator..."
    
    # Run the site generator
    dotnet run --project CodeCubeConsole
    if ($LASTEXITCODE -ne 0) { throw "Site generation failed" }
    
    Write-BuildStep "Build completed successfully!" "Green"
    Write-Host ""
    Write-Host "Generated site is available in: CodeCubeConsole/bin/Debug/net8.0/out/" -ForegroundColor Cyan
    
} catch {
    Write-BuildError $_.Exception.Message
    exit 1
}