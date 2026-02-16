# F# to TypeScript Type Safety Pipeline

## Overview

This skill provides guidance for maintaining type safety across the F# backend and TypeScript client by validating that F# discriminated unions, records, and types are correctly translated through OpenAPI specifications into TypeScript models.

## When to Use This Skill

- After modifying F# types, records, or discriminated unions
- After changing API endpoint signatures
- After modifying JSON serialization configuration
- Before committing changes that affect the API contract
- When setting up CI/CD pipelines for type-safe APIs
- When troubleshooting type mismatches between backend and frontend

## The Pipeline Workflow

### 1. Make Changes to F# Project

Common changes that require pipeline validation:
- Adding/modifying discriminated unions
- Changing record types
- Adding/removing API endpoints
- Modifying serialization settings (JsonFSharpConverter options)
- Changing schema transformers

### 2. Run the F# Project

```powershell
cd d:\bug-samples\OxpeckerOpenApi.BugTest
dotnet run
```

Wait for the server to start and listen on the configured port (default: http://localhost:5166).

### 3. Capture the Generated OpenAPI Spec

```powershell
# Fetch the live spec from the running server
Invoke-WebRequest -Uri http://localhost:5166/openapi/v1.json -OutFile sample_spec_generated.json
```

The spec should be saved as `sample_spec_generated.json` for version control and comparison.

### 4. Verify the Spec Matches Expectations

#### Manual Verification

Open `sample_spec_generated.json` and check:

**For F# Discriminated Unions:**
- Verify the union is represented with `oneOf` schema
- Check that each case has the correct discriminator field (e.g., `"type": "Circle"`)
- Ensure fields for each case are properly named and typed

**For F# Records:**
- Check all fields are present in the schema
- Verify field types are correct (string, number, boolean, etc.)
- Check nullable handling for Option types

**For Enum-like Unions:**
- Verify simple unions (no fields) are represented as enums
- Check all cases are listed in the enum values

#### Automated Verification (Recommended)

Create test scripts that validate:
```powershell
# Example validation checks
$spec = Get-Content sample_spec_generated.json | ConvertFrom-Json

# Check a specific schema exists
if (-not $spec.components.schemas.Shape) {
    Write-Error "Shape schema missing!"
}

# Verify discriminator is present
if ($spec.components.schemas.Shape.discriminator.propertyName -ne "type") {
    Write-Error "Shape discriminator incorrect!"
}
```

### 5. Generate TypeScript Client

```powershell
# Using the enhanced generation script
.\generate-client.ps1

# Or directly with Docker
docker run --rm -v "${PWD}:/local" openapitools/openapi-generator-cli generate `
  -i /local/sample_spec_generated.json `
  -g typescript-axios `
  -o /local/generated-client `
  -c /local/openapi-generator-config.json `
  --skip-validate-spec
```

### 6. Examine TypeScript Models and Endpoints

#### Check Discriminated Unions

For F# type:
```fsharp
type Shape =
    | Circle of radius: float
    | Rectangle of width: float * height: float
    | Triangle of base: float * height: float
```

Verify TypeScript type:
```typescript
export type Shape =
  | { type: 'Circle', radius: number }
  | { type: 'Rectangle', width: number, height: number }
  | { type: 'Triangle', base: number, height: number };
```

#### Check Records

For F# type:
```fsharp
type Person = {
    Name: string
    Age: int
    Occupation: string option
}
```

Verify TypeScript interface:
```typescript
export interface Person {
    name: string;
    age: number;
    occupation?: string | null;
}
```

#### Check API Endpoints

For F# handler:
```fsharp
let getGreeting (name: string) : EndpointHandler =
    // ...
```

Verify TypeScript method:
```typescript
getGreeting(requestParameters: { name: string }): AxiosPromise<string>
```

#### Common Issues to Look For

1. **Discriminator Field Name**: Ensure F# union discriminator matches TypeScript
2. **Field Naming**: Check camelCase conversion (F# PascalCase → TypeScript camelCase)
3. **Optional Fields**: Verify Option<'T> becomes `T | null` or `T | undefined`
4. **Nested Unions**: Check complex union types are properly flattened
5. **Generic Types**: F# generics like `FSharpList<'T>` need special handling

### 7. Iterate Until Types Match

If discrepancies are found:

**Fix in F# Schema Transformers:**
- Modify `FSharpUnionSchemaTransformer.fs`
- Adjust `FSharpOptionSchemaTransformer`
- Update JsonFSharpConverter configuration

**Fix OpenAPI Generator Configuration:**
- Adjust `openapi-generator-config.json` options
- Use `--model-name-mappings` for renaming
- Use `--inline-schema-name-mappings` for inline schemas

**Document Known Limitations:**
- Some F# patterns may not translate perfectly
- Complex recursive types may need manual handling
- Generic constraints might require workarounds

## Automation Script Example

Create `test-pipeline.ps1`:

```powershell
param(
    [switch]$Quick,  # Skip server restart
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "=== F# to TypeScript Type Safety Pipeline ===" -ForegroundColor Cyan

# Step 1: Build F# project
Write-Host "`n[1/6] Building F# project..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) { exit 1 }

# Step 2: Start server (if not Quick mode)
if (-not $Quick) {
    Write-Host "`n[2/6] Starting F# server..." -ForegroundColor Yellow
    $job = Start-Job { cd $using:PWD; dotnet run }
    Start-Sleep -Seconds 5  # Wait for server to start
}

# Step 3: Fetch OpenAPI spec
Write-Host "`n[3/6] Fetching OpenAPI spec..." -ForegroundColor Yellow
try {
    Invoke-WebRequest -Uri http://localhost:5166/openapi/v1.json -OutFile sample_spec_generated.json
    Write-Host "✓ Spec captured successfully" -ForegroundColor Green
} catch {
    Write-Error "Failed to fetch spec. Is server running?"
    if ($job) { Stop-Job $job; Remove-Job $job }
    exit 1
}

# Step 4: Validate spec
Write-Host "`n[4/6] Validating OpenAPI spec..." -ForegroundColor Yellow
$spec = Get-Content sample_spec_generated.json | ConvertFrom-Json
# Add your validation checks here
Write-Host "✓ Spec validation passed" -ForegroundColor Green

# Step 5: Generate TypeScript client
Write-Host "`n[5/6] Generating TypeScript client..." -ForegroundColor Yellow
.\generate-client.ps1
if ($LASTEXITCODE -ne 0) {
    if ($job) { Stop-Job $job; Remove-Job $job }
    exit 1
}
Write-Host "✓ Client generated successfully" -ForegroundColor Green

# Step 6: Validate TypeScript types
Write-Host "`n[6/6] Validating TypeScript types..." -ForegroundColor Yellow
# Add TypeScript type validation here (e.g., tsc --noEmit)
Write-Host "✓ Type validation passed" -ForegroundColor Green

# Cleanup
if ($job) {
    Stop-Job $job
    Remove-Job $job
}

Write-Host "`n=== Pipeline completed successfully! ===" -ForegroundColor Green
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Type Safety Pipeline

on:
  push:
    paths:
      - 'OxpeckerOpenApi.BugTest/**/*.fs'
      - 'OxpeckerOpenApi.BugTest/**/*.fsproj'

jobs:
  type-safety-check:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Build F# project
        run: dotnet build
        working-directory: ./OxpeckerOpenApi.BugTest

      - name: Start F# server
        run: dotnet run &
        working-directory: ./OxpeckerOpenApi.BugTest

      - name: Wait for server
        run: sleep 10

      - name: Fetch OpenAPI spec
        run: curl http://localhost:5166/openapi/v1.json -o sample_spec_generated.json
        working-directory: ./OxpeckerOpenApi.BugTest

      - name: Generate TypeScript client
        run: |
          docker run --rm \
            -v "$PWD:/local" \
            openapitools/openapi-generator-cli generate \
            -i /local/sample_spec_generated.json \
            -g typescript-axios \
            -o /local/generated-client \
            -c /local/openapi-generator-config.json \
            --skip-validate-spec
        working-directory: ./OxpeckerOpenApi.BugTest

      - name: Upload generated client
        uses: actions/upload-artifact@v3
        with:
          name: typescript-client
          path: OxpeckerOpenApi.BugTest/generated-client
```

## Best Practices

1. **Version Control the Generated Spec**: Commit `sample_spec_generated.json` to track API contract changes
2. **Automate in CI**: Run the pipeline on every commit to catch type mismatches early
3. **Document Transformers**: Keep clear documentation of custom schema transformers
4. **Test Union Cases**: Add test endpoints that return each union case
5. **Validate at Runtime**: Add integration tests that call both F# and TypeScript code
6. **Keep Config in Sync**: Ensure JsonFSharpConverter settings match schema transformers

## Troubleshooting

### Server Won't Start
- Check for port conflicts
- Verify all NuGet packages are restored
- Check for compilation errors

### Spec Doesn't Match Expected
- Review schema transformer implementations
- Check JsonFSharpConverter configuration
- Verify F# types are public and properly annotated

### TypeScript Types Are Wrong
- Review OpenAPI Generator configuration
- Check for naming convention mismatches
- Verify discriminator configuration in the spec

### Generic Types Look Wrong
- F# generic types may need special handling
- Consider using `--inline-schema-name-mappings`
- May need custom schema transformers

## Related Files

- `Program.fs` - F# application entry point with serialization config
- `FSharpUnionSchemaTransformer.fs` - Custom union schema generation
- `generate-client.ps1` - Client generation script
- `openapi-generator-config.json` - TypeScript generator configuration
- `sample_spec_generated.json` - Generated OpenAPI specification
- `client-usage-example.ts` - TypeScript usage examples

## Examples

### Testing a New Union Type

1. Add to F# (`Types.fs`):
```fsharp
type PaymentMethod =
    | CreditCard of cardNumber: string * cvv: string
    | PayPal of email: string
    | Cash
```

2. Add handler (`Handlers.fs`):
```fsharp
let getPaymentMethod: EndpointHandler =
    fun ctx -> task {
        let payment = CreditCard("1234-5678", "123")
        return ctx.WriteJson(payment)
    }
```

3. Add endpoint (`Endpoints.fs`):
```fsharp
routef "/payment-method" getPaymentMethod
```

4. Run pipeline:
```powershell
.\test-pipeline.ps1
```

5. Verify TypeScript type in `generated-client/models/payment-method.ts`:
```typescript
export type PaymentMethod =
  | { type: 'CreditCard', cardNumber: string, cvv: string }
  | { type: 'PayPal', email: string }
  | { type: 'Cash' };
```

## Further Reading

- [OpenAPI Generator TypeScript-Axios](https://openapi-generator.tech/docs/generators/typescript-axios/)
- [System.Text.Json in F#](https://github.com/Tarmil/FSharp.SystemTextJson)
- [Oxpecker OpenAPI Documentation](https://github.com/Lanayx/Oxpecker)
- [TypeScript Discriminated Unions](https://www.typescriptlang.org/docs/handbook/unions-and-intersections.html)
