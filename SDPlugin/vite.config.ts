import {defineConfig} from "vite";
import {svelte} from "@sveltejs/vite-plugin-svelte";
import * as path from "node:path";
import {manifestNs} from "./build/scripts/manifest";
import packageJson from "./package.json";

export default defineConfig({
    root: path.resolve(__dirname, "src/pi"),
    plugins: [svelte()],
    base: "./",
    define: {
        __BUILD_VERSION__: JSON.stringify(packageJson.version),
    },
    build: {
        outDir: path.resolve(__dirname, "dist", `${manifestNs}.sdPlugin`),
        emptyOutDir: false,
    },
});
