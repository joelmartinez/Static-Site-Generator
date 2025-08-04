const path = require('path');

module.exports = {
  entry: {
    app: './src/index.js',
    os: './src/os/index.js'
  },
  output: {
    filename: '[name].gen.js',
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
  module: {
    rules: [
      {
        test: /\.(js|jsx)$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader',
          options: {
            presets: ['@babel/preset-env', '@babel/preset-react']
          }
        }
      }
    ]
  },
  resolve: {
    extensions: ['.js', '.jsx']
  }
};