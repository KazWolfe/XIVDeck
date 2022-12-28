import * as path from 'path';
import * as webpack from 'webpack';

import {manifestNs} from "./build/scripts/manifest";

import copyWebpackPlugin from 'copy-webpack-plugin';

const config = (environment: unknown, options: { mode: string; env: unknown }): webpack.Configuration => {
  let pluginNs = manifestNs;
  let pluginVersion = process.env.npm_package_version!;

  return {
    entry: {
      plugin: './build/entries/PluginEntry.ts',
      propertyInspector: './build/entries/PropertyInspectorEntry.ts'
    },
    target: 'web',
    output: {
      library: 'connectElgatoStreamDeckSocket',
      libraryExport: 'default',
      path: path.resolve(__dirname, 'dist/' + pluginNs + '.sdPlugin/js')
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
                // This is hacky as hell, but for whatever reason actually parsing JSON fails. So we'll just do this
                // the cheaty way.
                return content.toString()
                    .replaceAll('assets/', '')
                    .replaceAll('{{ PLUGIN_VERSION }}', pluginVersion);
              }
              
              // patch variables in HTML
              if (/\.html/.test(path)) {
                return content.toString()
                    .replaceAll('{{ PLUGIN_NS }}', pluginNs)
                    .replaceAll('{{ PLUGIN_VERSION }}', pluginVersion);
                
              }

              // otherwise, return unmodified content
              return content;
            },
          }
        ],
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
        {
          test: /locales/,
          loader: "@alienfast/i18next-loader",
          options: {
            basenameAsNamespace: true,
          }
        },
      ],
    },
    resolve: {
      extensions: ['.ts', '.js'],
    },
    optimization: {
      splitChunks: { }
    }
  };

};

export default config;
