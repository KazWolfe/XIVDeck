import * as fs from "node:fs";
import {NodeSocketTransport} from "./NodeSocketTransport";
import type {NamedPipeTransportConfig} from "../../settings/types/TransportConfig";

const PIPE_PREFIX = "XIVDeck-";

export class NamedPipeTransport extends NodeSocketTransport {
    constructor(pipeName: string) {
        super(`pipe:${pipeName}`, `\\\\.\\pipe\\${pipeName}`);
    }

    /** Scan for named pipes that we may be able to connect to, in the order the pipe namespace lists them. */
    static discover(): NamedPipeTransport[] {
        let entries: string[];
        try {
            entries = fs.readdirSync("\\\\.\\pipe\\");
        } catch {
            return [];
        }

        return entries
            .filter(name => name.startsWith(PIPE_PREFIX))
            .map(name => new NamedPipeTransport(name));
    }

    static fromConfig(config: NamedPipeTransportConfig): NamedPipeTransport[] {
        return config.name ? [new NamedPipeTransport(config.name)] : NamedPipeTransport.discover();
    }
}
