const path = require('path');

module.exports = {
  entry: './src/index.js',
  output: {
    filename: 'app.gen.js',
    path: path.resolve(__dirname, '../CodeCubeConsole/out/script'),
    clean: {
      keep: (asset) => {
        // Keep all files that don't end with .gen.js
        return !asset.endsWith('.gen.js');
      }
    }
  },
  mode: 'production',
  optimization: {
    minimize: true
  },
  resolve: {
    extensions: ['.js', '.jsx']
  }
};