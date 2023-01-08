
export class GameNotRunningError extends Error {
    constructor(message?: string) {
        if (message == null) {
            message = "The game does not appear to be alive, or a websocket has not been created yet."
        }
        
        super(message);
    }
}

