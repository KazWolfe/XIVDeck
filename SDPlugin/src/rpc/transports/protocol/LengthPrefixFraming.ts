const MAX_FRAME_LENGTH = 16 * 1024 * 1024;

export function encodeFrame(payload: Uint8Array): Buffer {
    const header = Buffer.alloc(4);
    header.writeUInt32BE(payload.length, 0);
    return Buffer.concat([header, payload]);
}

export class FrameDecoder {
    private _buffer: Buffer = Buffer.alloc(0);

    constructor(private readonly onMessage: (message: Uint8Array) => void) {
    }

    push(chunk: Buffer): void {
        this._buffer = Buffer.concat([this._buffer, chunk]);

        while (true) {
            if (this._buffer.length < 4) return;

            const contentLength = this._buffer.readUInt32BE(0);
            if (contentLength > MAX_FRAME_LENGTH) {
                throw new Error(`Frame length ${contentLength} exceeds maximum allowed size of ${MAX_FRAME_LENGTH} bytes`);
            }

            const bodyEnd = 4 + contentLength;

            if (this._buffer.length < bodyEnd) return;

            const body = this._buffer.subarray(4, bodyEnd);
            this._buffer = this._buffer.subarray(bodyEnd);

            this.onMessage(body);
        }
    }
}
