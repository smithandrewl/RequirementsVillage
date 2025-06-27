const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');

const outputDir = path.join(__dirname, '../RequirementsVillage.Api/wwwroot');

module.exports = {
  mode: 'development',
  entry: './src/App.fs.js',
  output: {
    path: outputDir,
    filename: 'bundle.[contenthash].js',
    clean: true
  },
  devServer: {
    static: {
      directory: outputDir,
    },
    port: 8080,
    hot: true,
    proxy: [
      {
        context: ['/api'],
        target: 'http://localhost:5000',
        changeOrigin: true
      }
    ]
  },
  plugins: [
    new HtmlWebpackPlugin({
      template: './public/index.html',
      filename: 'index.html'
    })
  ],
  resolve: {
    modules: [path.resolve('./node_modules')]
  }
};