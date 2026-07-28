export type GlobalPiCommandHandler = (params: unknown) => Promise<unknown> | unknown;

const handlers = new Map<string, GlobalPiCommandHandler>();

export function registerGlobalPiCommand(command: string, handler: GlobalPiCommandHandler): void {
    handlers.set(command, handler);
}

export function getGlobalPiCommand(command: string): GlobalPiCommandHandler | undefined {
    return handlers.get(command);
}
