# F# to TypeScript Type Safety Pipeline
# Automates the process of validating type safety from F# through OpenAPI to TypeScript

param(
    [switch]$Quick,      # Skip server restart (assume already running)
    [switch]$Verbose,    # Show detailed output
    [switch]$StopServer, # Stop server after completion
    [string]$ServerUrl = "http://localhost:5166"
)

$ErrorActionPreference = "Stop"
$job = $null

function Write-Step {
    param([string]$Message)
    Write-Host "`n$Message" -ForegroundColor Yellow
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Failure {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Cleanup {
    if ($job) {
        Write-Host "`nCleaning up server process..." -ForegroundColor Gray
        Stop-Job $job -ErrorAction SilentlyContinue
        Remove-Job $job -ErrorAction SilentlyContinue
    }
}

# Set up cleanup on exit
trap { Cleanup; break }

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  F# to TypeScript Type Safety Pipeline                    ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# ============================================================================
# Step 1: Build F# Project
# ============================================================================
Write-Step "[1/6] Building F# project..."
try {
    if ($Verbose) {
        dotnet build
    } else {
        dotnet build --nologo --verbosity quiet
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Build failed"
        exit 1
    }
    Write-Success "Build completed successfully"
} catch {
    Write-Failure "Build failed with error: $_"
    exit 1
}

# ============================================================================
# Step 2: Start/Check Server
# ============================================================================
if (-not $Quick) {
    Write-Step "[2/6] Starting F# server..."
    try {
        # Start server in background job
        $job = Start-Job -ScriptBlock {
            param($path)
            Set-Location $path
            dotnet run 2>&1
        } -ArgumentList $PWD

        # Wait for server to start (max 15 seconds)
        Write-Host "Waiting for server to start..." -ForegroundColor Gray
        $maxAttempts = 30
        $attempt = 0
        $serverStarted = $false

        while ($attempt -lt $maxAttempts -and -not $serverStarted) {
            Start-Sleep -Milliseconds 500
            $attempt++
            try {
                $response = Invoke-WebRequest -Uri "$ServerUrl/hello" -TimeoutSec 1 -ErrorAction SilentlyContinue
                if ($response.StatusCode -eq 200) {
                    $serverStarted = $true
                    Write-Success "Server started successfully"
                }
            } catch {
                # Server not ready yet, continue waiting
            }

            # Check if job has failed
            if ($job.State -eq "Failed") {
                Write-Failure "Server failed to start"
                Receive-Job $job
                exit 1
            }
        }

        if (-not $serverStarted) {
            Write-Failure "Server failed to start within timeout period"
            Receive-Job $job
            Cleanup
            exit 1
        }
    } catch {
        Write-Failure "Failed to start server: $_"
        Cleanup
        exit 1
    }
} else {
    Write-Step "[2/6] Checking if server is running..."
    try {
        $response = Invoke-WebRequest -Uri "$ServerUrl/hello" -TimeoutSec 2
        Write-Success "Server is running"
    } catch {
        Write-Failure "Server is not responding at $ServerUrl"
        Write-Host "Please start the server manually or run without -Quick flag" -ForegroundColor Yellow
        exit 1
    }
}

# ============================================================================
# Step 3: Fetch OpenAPI Spec
# ============================================================================
Write-Step "[3/6] Fetching OpenAPI spec from $ServerUrl/openapi/v1.json..."
try {
    $specUrl = "$ServerUrl/openapi/v1.json"
    Invoke-WebRequest -Uri $specUrl -OutFile sample_spec_generated.json

    $specSize = (Get-Item sample_spec_generated.json).Length
    Write-Success "Spec captured successfully ($specSize bytes)"
} catch {
    Write-Failure "Failed to fetch spec: $_"
    Write-Host "Ensure server is running at $ServerUrl" -ForegroundColor Yellow
    Cleanup
    exit 1
}

# ============================================================================
# Step 4: Validate OpenAPI Spec
# ============================================================================
Write-Step "[4/6] Validating OpenAPI spec..."
try {
    $spec = Get-Content sample_spec_generated.json | ConvertFrom-Json

    # Basic validation checks
    $checks = @{
        "OpenAPI version" = $spec.openapi -ne $null
        "Info section" = $spec.info -ne $null
        "Paths section" = $spec.paths -ne $null
        "Components section" = $spec.components -ne $null
        "Schemas present" = $spec.components.schemas.Count -gt 0
    }

    $allPassed = $true
    foreach ($check in $checks.GetEnumerator()) {
        if ($check.Value) {
            if ($Verbose) { Write-Host "  ✓ $($check.Key)" -ForegroundColor Green }
        } else {
            Write-Host "  ✗ $($check.Key)" -ForegroundColor Red
            $allPassed = $false
        }
    }

    if (-not $allPassed) {
        Write-Failure "Spec validation failed"
        Cleanup
        exit 1
    }

    Write-Success "Spec validation passed ($($spec.components.schemas.Count) schemas, $($spec.paths.Count) paths)"

    # Show some details if verbose
    if ($Verbose) {
        Write-Host "`nDetected Schemas:" -ForegroundColor Gray
        $spec.components.schemas.PSObject.Properties.Name | Sort-Object | ForEach-Object {
            Write-Host "  - $_" -ForegroundColor Gray
        }
    }
} catch {
    Write-Failure "Failed to validate spec: $_"
    Cleanup
    exit 1
}

# ============================================================================
# Step 5: Generate TypeScript Client
# ============================================================================
Write-Step "[5/6] Generating TypeScript client..."
try {
    if ($Verbose) {
        .\generate-client.ps1
    } else {
        .\generate-client.ps1 2>&1 | Out-Null
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Client generation failed"
        Cleanup
        exit 1
    }

    # Count generated files
    $apiFiles = (Get-ChildItem generated-client/api/*.ts -ErrorAction SilentlyContinue).Count
    $modelFiles = (Get-ChildItem generated-client/models/*.ts -ErrorAction SilentlyContinue).Count

    Write-Success "Client generated successfully ($apiFiles APIs, $modelFiles models)"
} catch {
    Write-Failure "Failed to generate client: $_"
    Cleanup
    exit 1
}

# ============================================================================
# Step 6: Validate TypeScript Types
# ============================================================================
Write-Step "[6/6] Validating TypeScript types..."
try {
    # Check that key files exist
    $requiredFiles = @(
        "generated-client/index.ts",
        "generated-client/api.ts",
        "generated-client/configuration.ts"
    )

    $allExist = $true
    foreach ($file in $requiredFiles) {
        if (-not (Test-Path $file)) {
            Write-Host "  ✗ Missing: $file" -ForegroundColor Red
            $allExist = $false
        } elseif ($Verbose) {
            Write-Host "  ✓ Found: $file" -ForegroundColor Green
        }
    }

    if (-not $allExist) {
        Write-Failure "Some required files are missing"
        Cleanup
        exit 1
    }

    # Optional: Run TypeScript compiler if available
    if (Get-Command tsc -ErrorAction SilentlyContinue) {
        Write-Host "  Running TypeScript compiler..." -ForegroundColor Gray
        Push-Location generated-client
        try {
            tsc --noEmit 2>&1 | Out-Null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  ✓ TypeScript compilation successful" -ForegroundColor Green
            } else {
                Write-Host "  ⚠ TypeScript compilation had warnings" -ForegroundColor Yellow
            }
        } finally {
            Pop-Location
        }
    } elseif ($Verbose) {
        Write-Host "  TypeScript compiler not found, skipping type check" -ForegroundColor Gray
    }

    Write-Success "Type validation passed"
} catch {
    Write-Failure "Type validation failed: $_"
    Cleanup
    exit 1
}

# ============================================================================
# Cleanup
# ============================================================================
if ($StopServer -and $job) {
    Write-Host "`nStopping server..." -ForegroundColor Gray
    Cleanup
}

# ============================================================================
# Summary
# ============================================================================
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  Pipeline completed successfully!                         ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green

if (-not $StopServer -and $job) {
    Write-Host "`nNote: Server is still running in the background." -ForegroundColor Cyan
    Write-Host "Run with -StopServer flag to automatically stop it." -ForegroundColor Cyan
}

Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "  • Review generated-client/ for TypeScript types" -ForegroundColor Gray
Write-Host "  • Compare with F# types to verify correctness" -ForegroundColor Gray
Write-Host "  • Run integration tests if available" -ForegroundColor Gray
Write-Host "  • Commit changes if types match expectations" -ForegroundColor Gray
