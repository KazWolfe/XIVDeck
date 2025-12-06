import * as path from 'path';
import * as webpack from 'webpack';

import {manifestNs} from "./build/scripts/manifest";

import CopyWebpackPlugin from "copy-webpack-plugin";


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
            new CopyWebpackPlugin({
                patterns: [
                    {
                        from: 'manifest.json',
                        context: path.resolve(__dirname, 'assets'),
                        to: path.resolve(__dirname, 'dist', `${pluginNs}.sdPlugin`, 'manifest.json'),
                        transform: (content) => {
                            const contentString = new TextDecoder('utf-8').decode(content);
                            const manifest = JSON.parse(contentString);

                            manifest.Version = pluginVersion;

                            return Buffer.from(JSON.stringify(manifest, null, 2), 'utf-8');
                        }
                    },
                    {
                        from: '**/*.html',
                        context: path.resolve(__dirname, 'assets'),
                        to: path.resolve(__dirname, 'dist', `${pluginNs}.sdPlugin`),
                        toType: 'dir',
                        transform: (content) => {
                            const contentString = new TextDecoder('utf-8').decode(content)
                                .replaceAll('{{ PLUGIN_NS }}', pluginNs)
                                .replaceAll('{{ PLUGIN_VERSION }}', pluginVersion);

                            return Buffer.from(contentString, 'utf-8');
                        }
                    },
                    {
                        from: 'assets',
                        to: path.resolve(__dirname, 'dist', `${pluginNs}.sdPlugin`),
                        toType: 'dir'
                    }
                ]
            })
        ],
        module: {
            rules: [
                {
                    test: /\.ts$/,
                    use: 'ts-loader',
                    exclude: /node_modules/
                },
                {
                    test: /\.js$/,
                    enforce: 'pre',
                    use: [
                        {
                            loader: 'source-map-loader',
                            options: {
                                filterSourceMappingUrl: () => false
                            }
                        }
                    ]
                },
                {
                    test: /locales/,
                    loader: "@alienfast/i18next-loader",
                    options: {
                        basenameAsNamespace: true
                    }
                }
            ]
        },
        resolve: {
            extensions: ['.ts', '.js']
        },
        optimization: {
            splitChunks: {}
        }
    };

};

export default config;
