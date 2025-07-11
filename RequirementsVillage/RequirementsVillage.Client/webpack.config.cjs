const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');

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
    static: [
      {
        directory: outputDir,
      },
      {
        directory: path.join(__dirname, 'public'),
        publicPath: '/',
      }
    ],
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
    }),
    new CopyWebpackPlugin({
      patterns: [
        {
          from: 'public',
          to: '.',
          globOptions: {
            ignore: ['**/index.html']
          }
        },
        {
          from: 'src/styles',
          to: 'styles'
        }
      ]
    })
  ],
  resolve: {
    modules: [path.resolve('./node_modules')]
  }
};