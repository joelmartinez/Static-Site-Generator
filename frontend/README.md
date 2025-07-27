# Frontend Build Workflow

This directory contains the modern frontend build pipeline for the CodeCube static site generator.

## Overview

The frontend workflow uses webpack to bundle JavaScript files and output them to the `CodeCubeConsole/out/script/` directory with a `.gen.js` suffix to clearly mark them as generated assets.

## Structure

```
frontend/
├── package.json          # NPM dependencies and scripts
├── webpack.config.js     # Webpack bundling configuration
└── src/
    └── index.js          # Main entry point for frontend code
```

## Scripts

- `npm run build` - Build production assets
- `npm run dev` - Build development assets (unminified)
- `npm run clean` - Remove generated `.gen.js` files

## Development Workflow

1. **Edit source files**: Modify files in `frontend/src/`
2. **Build frontend**: Run `npm run build` in the frontend directory
3. **Build site**: Use the root-level `build.ps1` script which handles both frontend and .NET builds
4. **Generated files**: Check `CodeCubeConsole/out/script/app.gen.js` for bundled output

## Integration

The frontend build is integrated into the main build process:

- **PowerShell script** (`../build.ps1`): Orchestrates frontend build + .NET build + site generation
- **GitHub Actions**: Uses the PowerShell script for CI/CD
- **Generated files**: Use `.gen.js` pattern and are excluded from version control

## Adding Features

To add new frontend functionality:

1. Create new JavaScript modules in `frontend/src/`
2. Import them in `index.js`
3. Run `npm run build` to bundle
4. The bundled output will be included in the static site

## Future Enhancements

This foundation supports adding:
- CSS preprocessing (Sass/Less)
- Modern JavaScript transpilation (Babel)
- Development server with hot reloading
- Additional bundled assets (images, fonts)
- Multiple entry points for different pages