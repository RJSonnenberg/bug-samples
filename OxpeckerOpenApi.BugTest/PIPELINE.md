# F# to TypeScript Type Safety Pipeline

This document describes the automated pipeline for maintaining type safety between the F# backend and TypeScript client.

## Overview

The pipeline ensures that changes to F# types are correctly reflected in the generated TypeScript client through OpenAPI specification generation and validation.

## Quick Start

```powershell
# Run the full pipeline (builds, starts server, generates client)
.\test-pipeline.ps1

# Run with existing server (faster)
.\test-pipeline.ps1 -Quick

# Run with detailed output
.\test-pipeline.ps1 -Verbose

# Run and stop server when done
.\test-pipeline.ps1 -StopServer
```

## Pipeline Steps

### 1. Build F# Project
- Compiles the F# application
- Verifies no compilation errors
- Ensures all dependencies are resolved

### 2. Start Server
- Launches the F# web server
- Waits for server to be ready
- Verifies server responds to requests
- (Skipped with `-Quick` flag if server already running)

### 3. Fetch OpenAPI Spec
- Retrieves the live OpenAPI specification from `/openapi/v1.json`
- Saves to `sample_spec_generated.json`
- This file can be committed for tracking API contract changes

### 4. Validate Spec
- Checks OpenAPI version and structure
- Verifies schemas and paths are present
- Counts endpoints and models
- Ensures spec is well-formed

### 5. Generate TypeScript Client
- Uses OpenAPI Generator with Docker
- Generates TypeScript-Axios client
- Creates type-safe API methods
- Generates TypeScript interfaces for all F# types

### 6. Validate TypeScript Types
- Checks that all expected files were generated
- Optionally runs TypeScript compiler (`tsc --noEmit`)
- Verifies type correctness

## What to Check After Running

### F# Discriminated Unions → TypeScript

**F# Type:**
```fsharp
type Shape =
    | Circle of radius: float
    | Rectangle of width: float * height: float
    | Triangle of base: float * height: float
```

**Expected TypeScript:**
```typescript
export type Shape = 
  | { type: 'Circle', radius: number }
  | { type: 'Rectangle', width: number, height: number }
  | { type: 'Triangle', base: number, height: number };
```

**Where to Check:** `generated-client/models/shape.ts`

### F# Records → TypeScript Interfaces

**F# Type:**
```fsharp
type Person = {
    Name: string
    Age: int
    Occupation: string option
}
```

**Expected TypeScript:**
```typescript
export interface Person {
    name: string;
    age: number;
    occupation?: string | null;
}
```

**Where to Check:** `generated-client/models/person.ts`

### F# Enum-like Unions → TypeScript Enums

**F# Type:**
```fsharp
type AnimalColor =
    | Brown
    | Black
    | White
    | Spotted
```

**Expected TypeScript:**
```typescript
export enum AnimalColor {
    Brown = 'Brown',
    Black = 'Black',
    White = 'White',
    Spotted = 'Spotted'
}
```

**Where to Check:** `generated-client/models/animal-color.ts`

## Common Issues and Solutions

### Issue: Server won't start
**Solution:** Check for port conflicts or existing processes
```powershell
# Find process using port 5166
netstat -ano | findstr :5166

# Kill the process if needed
taskkill /PID <process_id> /F
```

### Issue: Spec fetch fails
**Solution:** Verify server is running and accessible
```powershell
# Test server is responding
curl http://localhost:5166/hello
```

### Issue: TypeScript types don't match F# types
**Solution:** Check schema transformers and serialization settings
- Review `FSharpUnionSchemaTransformer.fs`
- Check `JsonFSharpConverter` configuration in `Program.fs`
- Verify discriminator settings

### Issue: Field names don't match (PascalCase vs camelCase)
**Solution:** This is expected - OpenAPI converts to camelCase
- F# uses PascalCase: `Name`, `Age`
- TypeScript uses camelCase: `name`, `age`

### Issue: Generic types look wrong
**Solution:** F# generics may need special handling
- Check the `--skip-validate-spec` flag is used
- Review inline schema naming in the spec
- May need custom schema transformers

## Integration with CI/CD

### GitHub Actions Example

Create `.github/workflows/type-safety.yml`:

```yaml
name: Type Safety Check

on:
  push:
    paths:
      - '**/*.fs'
      - '**/*.fsproj'

jobs:
  validate-types:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Run Type Safety Pipeline
        run: .\test-pipeline.ps1 -StopServer
        working-directory: ./OxpeckerOpenApi.BugTest
      
      - name: Upload Generated Client
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: typescript-client
          path: OxpeckerOpenApi.BugTest/generated-client
      
      - name: Upload OpenAPI Spec
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: openapi-spec
          path: OxpeckerOpenApi.BugTest/sample_spec_generated.json
```

## Best Practices

1. **Run Before Committing**: Always run the pipeline before committing F# type changes
2. **Commit the Spec**: Include `sample_spec_generated.json` in version control to track API changes
3. **Review Generated Code**: Manually inspect the generated TypeScript to ensure correctness
4. **Add Tests**: Create integration tests that use both F# and TypeScript code
5. **Document Limitations**: Note any F# patterns that don't translate perfectly to TypeScript
6. **Keep Config Updated**: Ensure serialization settings match schema transformers

## Files in This Pipeline

| File | Purpose |
|------|---------|
| `test-pipeline.ps1` | Main automation script |
| `generate-client.ps1` | TypeScript client generation |
| `openapi-generator-config.json` | Generator configuration |
| `sample_spec_generated.json` | Generated OpenAPI spec |
| `Program.fs` | F# app config & JSON serialization |
| `FSharpUnionSchemaTransformer.fs` | Custom union schema generation |
| `generated-client/` | Generated TypeScript client |

## Troubleshooting

### Enable Verbose Output
```powershell
.\test-pipeline.ps1 -Verbose
```

### Check Server Logs
```powershell
# Start server manually to see logs
dotnet run
```

### Validate Spec Manually
```powershell
# Open spec in browser or editor
code sample_spec_generated.json
```

### Test TypeScript Compilation
```powershell
cd generated-client
tsc --noEmit
```

## Further Help

For detailed guidance on the pipeline workflow, the Copilot skill is available:
- Skill: `fsharp-openapi-typescript-pipeline`
- Location: `.copilot/skills/fsharp-openapi-typescript-pipeline/SKILL.md`

Ask Copilot:
> "Help me with the F# to TypeScript type safety pipeline"
