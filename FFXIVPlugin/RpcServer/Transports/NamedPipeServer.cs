using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using XIVDeck.FFXIVPlugin.Config;
using StreamJsonRpc;

namespace XIVDeck.FFXIVPlugin.RpcServer.Transports;

public class NamedPipeServer : IRpcTransport {
    private const string PipeNameBase = "XIVDeck";

    private readonly ILogger _log;

    private CancellationTokenSource? _cts;
    private Task? _acceptLoopTask;

    public TransportType Type => TransportType.NamedPipe;

    public string PipeName { get; } = $"{PipeNameBase}-{Environment.ProcessId}";

    public bool IsRunning => this._acceptLoopTask is {IsCompleted: false};

    public event Action<IJsonRpcMessageHandler>? Connected;

    public NamedPipeServer(ILogger logger) {
        this._log = logger;
    }

    public void StartServer() {
        this._cts = new CancellationTokenSource();

        this._acceptLoopTask = Task.Run(() => this.AcceptLoopAsync(this._cts.Token), this._cts.Token);
    }

    public void StopServer() {
        this._cts?.Cancel();

        try {
            this._acceptLoopTask?.Wait();
        } catch (AggregateException) {
            // expected on cancellation
        }
    }

    public void Dispose() {
        this.StopServer();

        GC.SuppressFinalize(this);
    }

    private async Task AcceptLoopAsync(CancellationToken token) {
        this._log.Information("Starting JSON-RPC pipe server on pipe '{PipeName}'", this.PipeName);

        while (!token.IsCancellationRequested) {
            var stream = new NamedPipeServerStream(
                this.PipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            try {
                await stream.WaitForConnectionAsync(token);
            } catch (OperationCanceledException) {
                await stream.DisposeAsync();
                break;
            }

            this.Connected?.Invoke(new LengthHeaderMessageHandler(stream, stream, RpcFormatter.Create()));
        }

        this._log.Debug("JSON-RPC pipe server has been stopped.");
    }
}
