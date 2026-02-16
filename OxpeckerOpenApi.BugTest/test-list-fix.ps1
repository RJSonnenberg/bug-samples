#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Test script to validate F# list schema transformation fix
.DESCRIPTION
    This script rebuilds the project, starts the server, generates the OpenAPI spec,
    and validates that F# lists are now mapped to arrays instead of discriminated unions.
#>

param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

Write-Host "`n=== F# List Schema Fix Validation ===" -ForegroundColor Cyan

# Step 1: Kill any running server instances
Write-Host "`n[1/6] Stopping any running server instances..." -ForegroundColor Yellow
$processes = Get-Process | Where-Object { $_.ProcessName -eq 'OxpeckerOpenApi.BugTest' }
if ($processes) {
    $processes | ForEach-Object {
        Write-Host "  Stopping process $($_.Id)..." -ForegroundColor Gray
        Stop-Process -Id $_.Id -Force
    }
    Start-Sleep -Seconds 2
    Write-Host "  ✓ Server stopped" -ForegroundColor Green
} else {
    Write-Host "  ✓ No running server found" -ForegroundColor Green
}

# Step 2: Build (if not skipped)
if (-not $SkipBuild) {
    Write-Host "`n[2/6] Building project..." -ForegroundColor Yellow
    dotnet build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ✗ Build failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "  ✓ Build succeeded" -ForegroundColor Green
}else {
    Write-Host "`n[2/6] Skipping build..." -ForegroundColor Gray
}

# Step 3: Start server in background
Write-Host "`n[3/6] Starting server..." -ForegroundColor Yellow
$serverJob = Start-Job -ScriptBlock {
    Set-Location "d:\bug-samples\OxpeckerOpenApi.BugTest"
    dotnet run
}
Write-Host "  Server job started (ID: $($serverJob.Id))" -ForegroundColor Gray

# Wait for server to be ready
Write-Host "  Waiting for server to start..." -ForegroundColor Gray
$maxAttempts = 20
$attempt = 0
$serverReady = $false

while ($attempt -lt $maxAttempts -and -not $serverReady) {
    Start-Sleep -Seconds 1
    $attempt++
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5166/hello" -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $serverReady = $true
            Write-Host "  ✓ Server is ready" -ForegroundColor Green
        }
    } catch {
        # Server not ready yet
    }

    # Check server output for schema transformation logs
    $jobOutput = Receive-Job -Job $serverJob 2>&1 | Out-String
    if ($jobOutput) {
        Write-Host "Server output:" -ForegroundColor Cyan
        Write-Host $jobOutput -ForegroundColor Gray
    }
}

if (-not $serverReady) {
    Write-Host "  ✗ Server failed to start" -ForegroundColor Red
    Stop-Job -Job $serverJob
    Remove-Job -Job $serverJob
    exit 1
}

# Step 4: Fetch OpenAPI spec
Write-Host "`n[4/6] Fetching OpenAPI specification..." -ForegroundColor Yellow
try {
    $spec = Invoke-RestMethod -Uri "http://localhost:5166/openapi/v1.json"
    $spec | ConvertTo-Json -Depth 100 | Out-File -FilePath "sample_spec_generated.json" -Encoding UTF8
    Write-Host "  ✓ Spec fetched and saved" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Failed to fetch spec: $_" -ForegroundColor Red
    Stop-Job -Job $serverJob
    Remove-Job -Job $serverJob
    exit 1
}

# Step 5: Validate list schemas
Write-Host "`n[5/6] Validating list schemas..." -ForegroundColor Yellow
$listSchemas = @('FSharpList`1', 'FSharpListOfPerson', 'FSharpListOfAnimal')
$allCorrect = $true

foreach ($schemaName in $listSchemas) {
    if ($spec.components.schemas.PSObject.Properties.Name -contains $schemaName) {
        $schema = $spec.components.schemas.$schemaName

        if ($schema.type -eq 'array' -and $schema.items) {
            Write-Host "  ✓ $schemaName is correctly an array type" -ForegroundColor Green
        } elseif ($schema.oneOf -or $schema.discriminator) {
            Write-Host "  ✗ $schemaName is still a discriminated union (FAILED)" -ForegroundColor Red
            $allCorrect = $false
        } else {
            Write-Host "  ? $schemaName has unexpected structure" -ForegroundColor Yellow
            $allCorrect = $false
        }
    } else {
        Write-Host "  ? $schemaName not found in schemas" -ForegroundColor Yellow
    }
}

# Step 6: Check for missing complex union types
Write-Host "`n[6/6] Checking for complex union types..." -ForegroundColor Yellow
$expectedTypes = @('Shape', 'AnimalColor', 'Person', 'Animal')
foreach ($typeName in $expectedTypes) {
    if ($spec.components.schemas.PSObject.Properties.Name -contains $typeName) {
        Write-Host "  ✓ $typeName exists" -ForegroundColor Green
    } else {
        Write-Host "  ✗ $typeName missing" -ForegroundColor Red
        $allCorrect = $false
    }
}

# Cleanup
Write-Host "`n[Cleanup] Stopping server..." -ForegroundColor Yellow
Stop-Job -Job $serverJob
Remove-Job -Job $serverJob
Write-Host "  ✓ Server stopped" -ForegroundColor Green

# Summary
Write-Host "`n=== Summary ===" -ForegroundColor Cyan
if ($allCorrect) {
    Write-Host "✓ ALL VALIDATIONS PASSED" -ForegroundColor Green
    exit 0
} else {
    Write-Host "✗ SOME VALIDATIONS FAILED" -ForegroundColor Red
    Write-Host "`nThe F# list types are still being generated as discriminated unions." -ForegroundColor Yellow
    Write-Host "This indicates the schema transformer fix needs further investigation." -ForegroundColor Yellow
    exit 1
}
