import * as fs from "node:fs";
import * as os from "node:os";
import * as path from "node:path";
import {NodeSocketTransport} from "./NodeSocketTransport";
import {NativeUtils} from "../../util/NativeUtils";
import type {UDSTransportConfig} from "../../settings/types/TransportConfig";

const SOCKET_PREFIX = "XIVDeck-";
const SOCKET_SUFFIX = ".sock";

export class UnixSocketTransport extends NodeSocketTransport {
    constructor(socketPath: string) {
        super(`uds:${socketPath}`, socketPath);
    }

    /**
     * Scans the given directories (default: candidate game/launcher temp dirs) for XIVDeck sockets.
     *
     * If multiple sockets are found, process them in order of age (oldest first). Skip any sockets that fail to
     * connect.
     */
    static discover(dirs: string[] = UnixSocketTransport.candidateDirs()): UnixSocketTransport[] {
        const found: {socketPath: string, mtimeMs: number}[] = [];

        for (const dir of dirs) {
            let entries: string[];
            try {
                entries = fs.readdirSync(dir);
            } catch {
                continue;
            }

            for (const name of entries) {
                if (!name.startsWith(SOCKET_PREFIX) || !name.endsWith(SOCKET_SUFFIX)) continue;

                const socketPath = path.join(dir, name);
                try {
                    found.push({socketPath, mtimeMs: fs.statSync(socketPath).mtimeMs});
                } catch {
                    // so, it just vanished. nice.
                }
            }
        }

        return found
            .sort((a, b) => a.mtimeMs - b.mtimeMs)
            .map(entry => new UnixSocketTransport(entry.socketPath));
    }

    /**
     * If no config set, autodiscover.
     * If config set to a directory, discover from that directory.
     * If config is set to a file, use it directly.
     */
    static fromConfig(config: UDSTransportConfig): UnixSocketTransport[] {
        if (!config.filename) return UnixSocketTransport.discover();

        let stat: fs.Stats;
        try {
            stat = fs.statSync(config.filename);
        } catch {
            return [];
        }

        return stat.isDirectory() ? UnixSocketTransport.discover([config.filename]) : [new UnixSocketTransport(config.filename)];
    }

    /** Scans for possible candidate socket directories. See UnixSocketServer.cs (and WineUtil.cs) for the scan behavior. */
    private static candidateDirs(): string[] {
        const dirs = new Set<string>([os.tmpdir(), "/tmp"]);
        const addAll = (paths: string[]) => paths.forEach(dir => dirs.add(dir));

        if (process.platform === "linux") {
            const xdgRuntimeDir = process.env.XDG_RUNTIME_DIR;
            if (xdgRuntimeDir) {
                dirs.add(xdgRuntimeDir);
                dirs.add(path.join(xdgRuntimeDir, "app", "dev.goats.xivlauncher"));  // XLCore Flatpak
            }

            addAll(UnixSocketTransport.wineTempDirs(path.join(os.homedir(), ".xlcore", "wineprefix"))); // XLCore

            const xdgDataHome = process.env.XDG_DATA_HOME || path.join(os.homedir(), ".local", "share");
            addAll(UnixSocketTransport.wineTempDirs(path.join(xdgDataHome, "dev.goats.xivlauncher", "wineprefix")));  // XLCore-RB Wine
            addAll(UnixSocketTransport.wineTempDirs(path.join(xdgDataHome, "dev.goats.xivlauncher", "protonprefix", "pfx"))); // XLCore-RB Proton
        } else if (process.platform === "darwin") {
            const userTemp = NativeUtils.darwinUserTempDir();
            if (userTemp) dirs.add(userTemp);

            // wineprefix default locations
            addAll(UnixSocketTransport.wineTempDirs(
                path.join(os.homedir(), "Library", "Application Support", "XIV on Mac", "wineprefix")));  // XIV On Mac
        }

        return [...dirs];
    }

    /** Resolve a wine root's users/temp path. */
    private static wineTempDirs(prefixPath: string): string[] {
        const usersDir = path.join(prefixPath, "drive_c", "users");

        let entries: fs.Dirent[];
        try {
            entries = fs.readdirSync(usersDir, {withFileTypes: true});
        } catch {
            return [];
        }

        return entries
            .filter(entry => entry.isDirectory() && entry.name !== "Public")
            .map(entry => path.join(usersDir, entry.name, "AppData", "Local", "Temp"));
    }
}
