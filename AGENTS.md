# AGENT INSTRUCTIONS

- Build the project with `dotnet build CodeCubeConsole.sln` after making code changes.
- You can run the generator manually with `dotnet run --project CodeCubeConsole`.
- For complete builds including frontend assets, use `pwsh ./build.ps1`.
- Static assets like CSS live under `CodeCubeConsole/out`. It's okay to modify them.
- The repo targets **.NET 8**.
- There are no unit tests.

## Script and Style Versioning

The build system supports versioning for cache busting:

- **Local development**: Run without version for clean URLs
  ```bash
  dotnet run --project CodeCubeConsole
  pwsh ./build.ps1
  ```

- **Production builds**: Include version for cache busting
  ```bash
  dotnet run --project CodeCubeConsole -- --version "1.2.3"
  pwsh ./build.ps1 -Version "1.2.3"
  ```

- The version parameter appends query strings to CSS/JS references (e.g., `?v=1.2.3`)
- GitHub Actions automatically uses build numbers as versions for deployments
- All static asset references in templates support consistent versioning
