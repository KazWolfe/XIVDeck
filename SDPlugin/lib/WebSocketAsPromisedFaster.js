import WebSocketAsPromised from "websocket-as-promised";

/* The mere fact that I can do this is terrifying and is one of the *many* reasons I dislike JavaScript.
 *
 * So, to explain. The native implementation of WebSocketAsPromised forces the message channels to execute asynchronously.
 * This is great and all, except that for some reason this forces everything to happen on very frustrating ticks causing
 * up to a second of speed loss while we wait for the next tick to roll around.
 * 
 * So, in order to fix this, we're overriding the socket here and just... swapping the dispatch to be synchronous. This
 * is honestly a terrible idea and will probably break something, but it's JavaScript.
 * 
 * This language is cursed.
 */

export class WebSocketAsPromisedFaster extends WebSocketAsPromised {
    _handleUnpackedData(data) {
        if (data !== undefined) {
            this._onUnpackedMessage.dispatch(data);
            this._tryHandleResponse(data);
        }
        this._tryHandleWaitingMessage(data);
    }
}