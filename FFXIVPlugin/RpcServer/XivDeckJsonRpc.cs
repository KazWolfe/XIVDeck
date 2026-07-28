using System;
using System.Threading.Tasks;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using XIVDeck.FFXIVPlugin.Exceptions;

namespace XIVDeck.FFXIVPlugin.RpcServer;

public class XivDeckJsonRpc(IJsonRpcMessageHandler handler) : JsonRpc(handler) {
    public event Action<Exception>? OnError;

    private volatile bool _disconnectAfterResponse;

    /// <summary>
    /// Hang up once the response currently being produced has been written to the transport.
    /// </summary>
    public void DisconnectAfterResponse() {
        this._disconnectAfterResponse = true;
    }

    protected override JsonRpcError.ErrorDetail CreateErrorDetails(JsonRpcRequest request, Exception exception) {
        this.OnError?.Invoke(exception);

        if (exception is IRpcException rpcException) {
            return new JsonRpcError.ErrorDetail {
                Code = (JsonRpcErrorCode) rpcException.ErrorCode,
                Message = exception.Message,
            };
        }

        return base.CreateErrorDetails(request, exception);
    }

    protected override void OnResponseSent(JsonRpcMessage response) {
        base.OnResponseSent(response);

        if (!this._disconnectAfterResponse) return;

        // just defer a dispose.
        Task.Run(this.Dispose);
    }
}
