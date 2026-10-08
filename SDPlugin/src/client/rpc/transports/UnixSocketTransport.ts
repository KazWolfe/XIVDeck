import * as fs from "node:fs";
import * as os from "node:os";
import * as path from "node:path";
import {NodeSocketTransport} from "./NodeSocketTransport";
import {NativeUtils} from "#/util/NativeUtils";

const SOCKET_PREFIX = "XIVDeck-";
const SOCKET_SUFFIX = ".sock";

export class UnixSocketTransport extends NodeSocketTransport {
    public constructor(socketPath: string) {
        super(socketPath);
    }

    /**
     * Scans the given directories (if none, the candidate game/launcher temp dirs) for XIVDeck sockets, returning their
     * paths oldest first.
     */
    public static discover(dirs: readonly string[]): string[] {
        const found: {socketPath: string, mtimeMs: number}[] = [];

        for (const dir of dirs.length > 0 ? dirs : UnixSocketTransport.candidateDirs()) {
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
            .map(entry => entry.socketPath);
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
