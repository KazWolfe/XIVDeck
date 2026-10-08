import * as path from 'path';
import * as webpack from 'webpack';
import {execFile} from 'child_process';

import {manifestNs} from "./build/scripts/manifest";
import packageJson from "./package.json";

import CopyWebpackPlugin from "copy-webpack-plugin";

class RestartPluginOnRebuild implements webpack.WebpackPluginInstance {
    constructor(private readonly uuid: string) {
    }

    apply(compiler: webpack.Compiler): void {
        compiler.hooks.afterEmit.tap('RestartPluginOnRebuild', () => {
            execFile('npx', ['streamdeck', 'restart', this.uuid], {shell: true}, (err, _stdout, stderr) => {
                if (err) console.warn(`[RestartPluginOnRebuild] Failed to restart plugin: ${stderr || err.message}`);
            });
        });
    }
}

const config = (environment: unknown, argv: { mode: string; env: unknown; watch?: boolean }): webpack.Configuration => {
    let pluginNs = manifestNs;
    let pluginVersion = packageJson.version;
    let outDir = path.resolve(__dirname, 'dist', `${pluginNs}.sdPlugin`);

    return {
        entry: {
            plugin: './src/host.ts',
        },
        target: 'node',
        output: {
            filename: '[name].js',
            path: outDir,
        },
        plugins: [
            new webpack.DefinePlugin({
                __BUILD_VERSION__: JSON.stringify(pluginVersion),
            }),

            new CopyWebpackPlugin({
                patterns: [
                    {
                        from: 'manifest.json',
                        context: path.resolve(__dirname, 'assets'),
                        to: path.resolve(outDir, 'manifest.json'),
                        transform: (content) => {
                            const contentString = new TextDecoder('utf-8').decode(content)
                                .replaceAll("assets/", "");

                            const manifest = JSON.parse(contentString);
                            manifest.Version = pluginVersion;
                            return Buffer.from(JSON.stringify(manifest, null, 2), 'utf-8');
                        }
                    },
                    {
                        from: 'assets',
                        to: outDir,
                        toType: 'dir',
                        globOptions: {
                            // bundled into plugin.js instead
                            ignore: ['**/templates/**'],
                        },
                    },
                ]
            }),

            ...(argv.mode !== 'production' ? [new RestartPluginOnRebuild(pluginNs)] : []),

            // ignored deps
            new webpack.IgnorePlugin({
                resourceRegExp: /^(bufferutil|utf-8-validate)$/,
            }),
        ],
        module: {
            rules: [
                {
                    test: /\.ts$/,
                    use: 'ts-loader',
                    exclude: /node_modules/
                },
                {
                    // SVG templates are bundled as strings rather than read from disk at runtime.
                    test: /\.svg$/,
                    include: path.resolve(__dirname, 'assets', 'templates'),
                    type: 'asset/source',
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
