import {RpcTransport} from "./transports/RpcTransport";
import {NamedPipeTransport} from "./transports/NamedPipeTransport";
import {UnixSocketTransport} from "./transports/UnixSocketTransport";
import {WebSocketTransport} from "./transports/WebSocketTransport";
import {AnyTransportConfig} from "../settings/types/TransportConfig";

/** Auto-detects running XIVDeck servers. Discovery doesn't check status, so we need to parse this list. */
export function discoverLocalTransports(): RpcTransport[] {
    return process.platform === "win32" ? NamedPipeTransport.discover() : UnixSocketTransport.discover();
}

function createTransports(config: AnyTransportConfig): RpcTransport[] {
    switch (config.type) {
        case "websocket":
            return [WebSocketTransport.fromConfig(config)];
        case "namedPipe":
            return NamedPipeTransport.fromConfig(config);
        case "uds":
            return UnixSocketTransport.fromConfig(config);
        default:
            throw new Error(`Unrecognized transport type "${(config as AnyTransportConfig).type}".`);
    }
}

export type TransportResolver = () => RpcTransport[];

export function makeTransportResolver(configured: AnyTransportConfig[], chosenTransportId?: string | null): TransportResolver {
    return () => {
        const chosen = chosenTransportId ? configured.find(t => t.id === chosenTransportId) : undefined;
        if (chosen) return dedupe(createTransports(chosen));

        const discovered = discoverLocalTransports();
        if (discovered.length > 0) return dedupe(discovered);

        return configured[0] ? dedupe(createTransports(configured[0])) : [];
    };
}

/** Candidate directories can overlap (e.g. /tmp and a symlinked temp dir), so drop repeats of the same target. */
function dedupe(transports: RpcTransport[]): RpcTransport[] {
    const seen = new Set<string>();
    return transports.filter(transport => !seen.has(transport.label) && seen.add(transport.label));
}

export function transportsEqual(a: AnyTransportConfig[], b: AnyTransportConfig[]): boolean {
    return JSON.stringify(a) === JSON.stringify(b);
}
