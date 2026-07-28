export interface RpcTransport {
    readonly label: string;

    connect(): Promise<void>;
    close(): void;

    send(message: Uint8Array): void;

    onMessage(handler: (message: Uint8Array) => void): void;
    onClose(handler: (err?: Error) => void): void;
}
