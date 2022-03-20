import * as path from 'path';
import * as webpack from 'webpack';

import {manifestNs} from "./build/scripts/manifest";

import copyWebpackPlugin from 'copy-webpack-plugin';

const config = (environment: unknown, options: { mode: string; env: unknown }): webpack.Configuration => {
  let pluginNs = manifestNs;

  return {
    entry: {
      plugin: './build/entries/PluginEntry.ts',
      propertyInspector: './build/entries/PropertyInspectorEntry.ts'
    },
    target: 'web',
    output: {
      library: 'connectElgatoStreamDeckSocket',
      libraryExport: 'default',
      path: path.resolve(__dirname, 'dist/' + pluginNs + '.sdPlugin/js'),
    },
    plugins: [
      new copyWebpackPlugin({
        patterns: [
          {
            from: 'assets',
            to: path.resolve(__dirname, 'dist/' + pluginNs + '.sdPlugin'),
            toType: 'dir',
            transform: (content, path) => {
              if (/manifest\.json$/.test(path)) {
                return content.toString().replaceAll('assets/', '')
              }
              if (!/\.html/.test(path)) {
                return content;
              }
              return content.toString().replace('{{ PLUGIN_NS }}', pluginNs);
            },
          }
        ],
      }),
      new webpack.ProvidePlugin({
        $: 'jquery/src/jquery',
        jquery: 'jquery/src/jquery'
      })
    ],
    module: {
      rules: [
        {
          test: /\.ts$/,
          use: 'ts-loader',
          exclude: /node_modules/,
        },
        {
          test: /\.js$/,
          enforce: 'pre',
          use: [
            {
              loader: 'source-map-loader',
              options: {
                filterSourceMappingUrl: () => false,
              },
            },
          ],
        },
      ],
    },
    resolve: {
      extensions: ['.ts', '.js'],
    },
    optimization: {
      splitChunks: {}
    }
  };

};

export default config;
