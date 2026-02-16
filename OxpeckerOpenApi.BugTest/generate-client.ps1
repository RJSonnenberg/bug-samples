# PowerShell script to generate TypeScript client from OpenAPI spec
# On Windows, Docker is recommended due to path handling issues with the npm method

param(
    [string]$SpecFile = "sample_spec_generated.json",
    [string]$OutputDir = "generated-client",
    [switch]$FetchLive = $false,
    [string]$ServerUrl = "http://localhost:5166"
)

Write-Host "Generating TypeScript client from OpenAPI specification..." -ForegroundColor Green

# Fetch live spec if requested
if ($FetchLive) {
    Write-Host "Fetching live OpenAPI spec from $ServerUrl/openapi/v1.json..." -ForegroundColor Cyan
    try {
        Invoke-WebRequest -Uri "$ServerUrl/openapi/v1.json" -OutFile $SpecFile
        Write-Host "Successfully fetched live spec to $SpecFile" -ForegroundColor Green
    } catch {
        Write-Host "Error fetching live spec: $_" -ForegroundColor Red
        Write-Host "Make sure the server is running at $ServerUrl" -ForegroundColor Yellow
        exit 1
    }
}

# Check if spec file exists
if (-not (Test-Path $SpecFile)) {
    Write-Host "Error: Spec file '$SpecFile' not found." -ForegroundColor Red
    Write-Host "Run with -FetchLive switch to download from running server" -ForegroundColor Yellow
    Write-Host "Or ensure the spec file exists at the specified path" -ForegroundColor Yellow
    exit 1
}

# Check if Docker is available (recommended for Windows)
if (Get-Command docker -ErrorAction SilentlyContinue) {
    Write-Host "Using Docker to run OpenAPI Generator (recommended for Windows)..." -ForegroundColor Cyan
    docker run --rm `
        -v "${PWD}:/local" `
        openapitools/openapi-generator-cli generate `
        -i /local/$SpecFile `
        -g typescript-axios `
        -o /local/$OutputDir `
        -c /local/openapi-generator-config.json `
        --skip-validate-spec
} elseif (Get-Command openapi-generator-cli -ErrorAction SilentlyContinue) {
    Write-Host "Using global OpenAPI Generator CLI..." -ForegroundColor Cyan
    openapi-generator-cli generate `
        -i $SpecFile `
        -g typescript-axios `
        -o $OutputDir `
        -c openapi-generator-config.json `
        --skip-validate-spec
} elseif (Get-Command npm -ErrorAction SilentlyContinue) {
    Write-Host "Using npm to run OpenAPI Generator CLI..." -ForegroundColor Yellow
    Write-Host "Note: npm method may have issues with Windows paths. Docker is recommended." -ForegroundColor Yellow
    npm run generate-client
} else {
    Write-Host "Error: No suitable OpenAPI Generator found." -ForegroundColor Red
    Write-Host "Please install one of the following:" -ForegroundColor Yellow
    Write-Host "  1. Docker - https://www.docker.com/ (RECOMMENDED for Windows)" -ForegroundColor Yellow
    Write-Host "  2. OpenAPI Generator CLI - npm install -g @openapitools/openapi-generator-cli" -ForegroundColor Yellow
    Write-Host "  3. npm (Node.js) - https://nodejs.org/" -ForegroundColor Yellow
    exit 1
}

if ($LASTEXITCODE -eq 0) {
    Write-Host "Client generation complete! Output directory: $OutputDir" -ForegroundColor Green
} else {
    Write-Host "Client generation failed. See errors above." -ForegroundColor Red
    exit 1
}
