import * as fs from "node:fs";
import {NodeSocketTransport} from "./NodeSocketTransport";

const PIPE_PREFIX = "XIVDeck-";

export class NamedPipeTransport extends NodeSocketTransport {
    public constructor(pipeName: string) {
        super(`\\\\.\\pipe\\${pipeName}`);
    }

    public static discover(): string[] {
        let entries: string[];
        try {
            entries = fs.readdirSync("\\\\.\\pipe\\");
        } catch {
            return [];
        }

        return entries.filter(name => name.startsWith(PIPE_PREFIX));
    }
}
