# CodeCube Static Site Generator

**ALWAYS REFERENCE THESE INSTRUCTIONS FIRST** and only fallback to additional search and context gathering if the information here is incomplete or found to be in error.

CodeCube is a .NET 8 static site generator that processes WordPress export files and markdown content. It features a modern frontend build pipeline with React components and generates a complete static website with CSS, JavaScript, and HTML files.

## Working Effectively

### Prerequisites and Setup
- Requires .NET 8 SDK (verified working with 8.0.118)
- Requires Node.js 20+ and npm (verified working with Node v20.19.4, npm 10.8.2)
- Requires PowerShell Core for complete builds

### Quick Start - Building and Running
Run these commands in sequence for a complete build:

```bash
# Build .NET solution (takes ~30 seconds)
dotnet build CodeCubeConsole.sln

# Install frontend dependencies (takes ~90 seconds on first run)
cd frontend && npm install && cd ..

# Run frontend tests (takes ~2 seconds)
cd frontend && npm test && cd ..

# Build frontend assets (takes ~9 seconds)  
cd frontend && npm run build && cd ..

# Generate static site (takes ~7 seconds)
dotnet run --project CodeCubeConsole

# OR: Complete build with PowerShell script (takes ~37 seconds total)
pwsh ./build.ps1
```

### Production Builds with Versioning
For deployments with cache busting:

```bash
# With version parameter for production
dotnet run --project CodeCubeConsole -- --version "1.2.3"

# Complete build with version
pwsh ./build.ps1 -Version "1.2.3"
```

## Critical Build Information

### Timing Expectations - NEVER CANCEL
- **.NET build**: 30 seconds - Set timeout to 60+ seconds
- **npm install** (first time): 90 seconds - Set timeout to 180+ seconds  
- **npm install** (subsequent): 5 seconds - Set timeout to 30+ seconds
- **Frontend tests**: 2 seconds - Set timeout to 30+ seconds
- **Frontend webpack build**: 9 seconds - Set timeout to 60+ seconds
- **Site generation**: 7 seconds - Set timeout to 60+ seconds
- **Complete PowerShell build**: 37 seconds - Set timeout to 120+ seconds

**NEVER CANCEL builds or tests early.** Wait for completion to avoid corrupted state.

### Key Locations
- **Generated site**: `CodeCubeConsole/bin/Debug/net8.0/out/`
- **Static assets**: `CodeCubeConsole/out/` (CSS, images, static JS)
- **Generated JS**: `CodeCubeConsole/out/script/` (webpack output)
- **Templates**: `CodeCubeConsole/Templates/` (Razor templates)
- **Content**: `CodeCubeConsole/content/` (markdown posts)
- **Frontend source**: `frontend/src/`

## Validation

### Manual Testing Scenarios
After making changes, ALWAYS run these validation steps:

1. **Build Validation**:
   ```bash
   # Test without version
   dotnet build CodeCubeConsole.sln
   dotnet run --project CodeCubeConsole
   
   # Test with version parameter
   dotnet run --project CodeCubeConsole -- --version "test.1.0.0"
   ```

2. **Generated Site Verification**:
   ```bash
   # Check site structure exists
   ls -la CodeCubeConsole/bin/Debug/net8.0/out/
   
   # Verify index.html contains expected content
   head -20 CodeCubeConsole/bin/Debug/net8.0/out/index.html
   
   # Verify version parameter works (should show ?v=test.1.0.0)
   grep "style2.css" CodeCubeConsole/bin/Debug/net8.0/out/index.html
   ```

3. **Frontend Asset Validation**:
   ```bash
   # Verify webpack generated files exist
   ls -la CodeCubeConsole/bin/Debug/net8.0/out/script/
   
   # Should contain: app.gen.js, os.gen.js, map.gen.js, tone.js
   ```

4. **Complete Build Test**:
   ```bash
   # Test full PowerShell build pipeline
   pwsh ./build.ps1
   
   # Verify publish directory created
   ls -la publish/out/
   ```

### Expected File Counts and Sizes
- **Generated pages**: ~250+ HTML files (blog posts from 2007-2025)
- **Index.html**: ~997 lines, ~65KB
- **Sitemap.xml**: ~37KB
- **Static assets**: CSS (~20KB total), JS (~750KB total)

## Architecture and Key Components

### Project Structure
- **CodeCubeConsole.sln**: Main .NET solution
- **CodeCubeConsole/**: Main C# project with Razor templates
- **frontend/**: Node.js build pipeline for React components
- **build.ps1**: PowerShell orchestration script
- **.github/workflows/**: CI/CD with auto-versioning

### Build System Flow
1. Frontend webpack builds React components → `CodeCubeConsole/out/script/`
2. .NET project copies static assets from `out/` folder
3. Razor templates generate HTML using markdown content and WordPress export
4. Version parameter appends query strings for cache busting
5. Generated site outputs to `bin/Debug/net8.0/out/`

### No Unit Tests
This project **has no .NET unit tests**. Only frontend has Jest tests in `frontend/tests/`.

## Common Tasks and Troubleshooting

### Template Changes
When modifying Razor templates in `CodeCubeConsole/Templates/`:
```bash
# Templates are copied on build, so always rebuild
dotnet build CodeCubeConsole.sln
dotnet run --project CodeCubeConsole
```

### Frontend Changes  
When modifying React components in `frontend/src/`:
```bash
cd frontend
npm run build
cd ..
dotnet run --project CodeCubeConsole
```

### Static Asset Changes
When modifying CSS or static files in `CodeCubeConsole/out/`:
```bash
# Files are copied on build, so rebuild is sufficient
dotnet build CodeCubeConsole.sln
dotnet run --project CodeCubeConsole
```

### Content Changes
When adding/modifying markdown in `CodeCubeConsole/content/`:
```bash
# Just regenerate site, no build needed
dotnet run --project CodeCubeConsole
```

### Clean Build
To start fresh:
```bash
# Clean .NET
dotnet clean CodeCubeConsole.sln

# Clean frontend  
cd frontend && npm run clean && cd ..

# Remove generated output
rm -rf CodeCubeConsole/bin/Debug/net8.0/out/
rm -rf publish/

# Full rebuild
pwsh ./build.ps1
```

## CI/CD Integration

The GitHub Actions workflow (`.github/workflows/build-and-run.yml`):
- Uses PowerShell build script with `${{ github.run_number }}` as version
- Deploys to FTP server from `publish/out/` directory
- Uploads artifacts for review

For local testing that matches CI:
```bash
pwsh ./build.ps1 -Version "999"
```

## File Reference

### Key Configuration Files
- `CodeCubeConsole/CodeCubeConsole.csproj`: .NET project with package references
- `frontend/package.json`: Node.js dependencies and scripts
- `frontend/webpack.config.js`: Webpack build configuration
- `build.ps1`: Complete build orchestration script

### Generated Output Structure
```
CodeCubeConsole/bin/Debug/net8.0/out/
├── index.html (main page)
├── about/ (about page)
├── resume/ (resume page)  
├── map/ (link map page)
├── os/ (terminal environment page)
├── feed/ (RSS feed)
├── design/ (CSS and images)
├── script/ (generated JavaScript)
├── 2007/ through 2025/ (blog post directories)
├── sitemap.xml
└── robots.txt
```

This structure contains a complete static website ready for deployment.