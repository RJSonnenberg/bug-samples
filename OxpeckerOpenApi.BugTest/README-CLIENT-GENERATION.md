# TypeScript Client Generation

This project is configured to generate a TypeScript client using the OpenAPI Generator with the `typescript-axios` generator.

# TypeScript Client Generation

This project is configured to generate a TypeScript client using the OpenAPI Generator with the `typescript-axios` generator.

## OpenAPI Spec Files

- **sample_spec.json** - Static example spec
- **sample_spec_generated.json** - Live spec fetched from running server (recommended)

## Prerequisites

Choose one of the following methods:

### Method 1: Using Docker (Recommended for Windows)
- Install [Docker](https://www.docker.com/)
- **Note**: This method is recommended on Windows due to path handling issues with the OpenAPI Generator CLI

### Method 2: Using npm
- Install [Node.js](https://nodejs.org/) (includes npm)
- **Note**: On Windows, the npm method may encounter path issues. Use Docker or the PowerShell script instead.

### Method 3: Using OpenAPI Generator CLI directly
- Install via npm: `npm install -g @openapitools/openapi-generator-cli`
- Or via Homebrew (Mac): `brew install openapi-generator`

## Quick Start

### Using PowerShell Script (Recommended for Windows)

```powershell
# Generate from the live server (fetches spec automatically)
.\generate-client.ps1 -FetchLive

# Or generate from existing spec file (default: sample_spec_generated.json)
.\generate-client.ps1

# Generate from a specific spec file
.\generate-client.ps1 -SpecFile "sample_spec.json"
```

### Using Docker (Recommended for Windows)

```bash
# First, fetch the live spec from your running server
Invoke-WebRequest -Uri http://localhost:5166/openapi/v1.json -OutFile sample_spec_generated.json

# Then generate the client
docker run --rm -v "${PWD}:/local" openapitools/openapi-generator-cli generate \
  -i /local/sample_spec_generated.json \
  -g typescript-axios \
  -o /local/generated-client \
  -c /local/openapi-generator-config.json \
  --skip-validate-spec
```

### Using npm

```bash
# Install dependencies (this will also generate the client automatically)
npm install

# Or generate manually
npm run generate-client
```

**Note**: On Windows, the npm method may fail due to path issues. Use the PowerShell script or Docker method instead.

## Fetching Live OpenAPI Spec

To get the most up-to-date spec from your running server:

```powershell
# Make sure your F# server is running (dotnet run)
# Then fetch the spec
Invoke-WebRequest -Uri http://localhost:5166/openapi/v1.json -OutFile sample_spec_generated.json

# Generate the client
.\generate-client.ps1
```

## Output

The generated client will be placed in the `generated-client/` directory with the following structure:

```
generated-client/
├── api/           # API endpoint classes
├── models/        # TypeScript models/interfaces
├── base.ts        # Base configuration
├── common.ts      # Common utilities
├── configuration.ts # Configuration class
└── index.ts       # Main export file
```

## Configuration Files

- **package.json**: Contains npm scripts and dependencies
- **openapitools.json**: OpenAPI Generator CLI configuration
- **openapi-generator-config.json**: TypeScript-axios generator specific options
- **.openapi-generator-ignore**: Files to ignore during generation (like .gitignore)

## Configuration Options

The generator is configured with the following options (in `openapi-generator-config.json`):

- `supportsES6`: true - Use modern JavaScript features
- `withInterfaces`: true - Generate TypeScript interfaces
- `useSingleRequestParameter`: true - Use a single parameter object for API calls
- `withSeparateModelsAndApi`: true - Separate models and API into different directories
- `stringEnums`: true - Generate string enums instead of numeric
- `sortParamsByRequiredFlag`: true - Sort parameters by required status

## Customization

To customize the generation:

1. Edit `openapi-generator-config.json` to change generator options
2. Edit `openapitools.json` to change the output directory or generator version
3. Edit `.openapi-generator-ignore` to prevent specific files from being overwritten

## Using the Generated Client

```typescript
import { Configuration, DefaultApi } from './generated-client';

const config = new Configuration({
  basePath: 'https://localhost:7124',
});

const api = new DefaultApi(config);

// Example: Call the hello endpoint
const response = await api.hello();
console.log(response.data);
```

## Regenerating the Client

Whenever your OpenAPI spec changes, regenerate the client:

```powershell
# Fetch live spec and generate
.\generate-client.ps1 -FetchLive

# Or just regenerate from existing spec
.\generate-client.ps1
```

## Automated Type Safety Pipeline

For a complete workflow that builds, tests, and validates type safety:

```powershell
# Run the full pipeline
.\test-pipeline.ps1

# Quick mode (server already running)
.\test-pipeline.ps1 -Quick

# See detailed output
.\test-pipeline.ps1 -Verbose
```

See [PIPELINE.md](PIPELINE.md) for complete documentation.

## Documentation

For more information:
- [OpenAPI Generator Docs](https://openapi-generator.tech/docs/generators/typescript-axios/)
- [TypeScript-Axios Generator Options](https://openapi-generator.tech/docs/generators/typescript-axios/)
- [Global Configuration](https://openapi-generator.tech/docs/globals/)
