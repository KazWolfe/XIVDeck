import {execFileSync} from "node:child_process";
import * as path from "node:path";

/** Helpers for OS-level details that Node doesn't expose portably. */
export class NativeUtils {
    private static _darwinUserTempDir?: string | null;

    /**
     * Workaround for an edge case where macOS will refuse to give $TMPDIR to certain processes.
     */
    public static darwinUserTempDir(): string | undefined {
        // If we have $TMPDIR already, let's just use that.
        const fromEnv = process.env.TMPDIR?.trim();
        if (fromEnv) return NativeUtils.normalizeDir(fromEnv);

        if (NativeUtils._darwinUserTempDir !== undefined) {
            return NativeUtils._darwinUserTempDir ?? undefined;
        }

        let resolved: string | null = null;
        try {
            const out = execFileSync("getconf", ["DARWIN_USER_TEMP_DIR"], {encoding: "utf8"}).trim();
            if (out) resolved = NativeUtils.normalizeDir(out);
        } catch {
            resolved = null;
        }

        NativeUtils._darwinUserTempDir = resolved;
        return resolved ?? undefined;
    }

    private static normalizeDir(dir: string): string {
        return path.normalize(dir).replace(/\/+$/, "");
    }
}
