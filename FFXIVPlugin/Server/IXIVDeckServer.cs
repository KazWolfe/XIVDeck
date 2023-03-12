using System;
using XIVDeck.FFXIVPlugin.Server.Messages;

namespace XIVDeck.FFXIVPlugin.Server; 

public interface IXIVDeckServer : IDisposable {
    public void StartServer();
    public void StopServer();
    
    public bool IsRunning { get; }

    /// <summary>
    /// Send a message to the bidirectional channel of all clients connected to this server.
    /// </summary>
    /// <remarks>
    /// This method may be called synchronously; it will spawn a task to handle async message delivery.
    /// </remarks>
    /// <param name="message">The base message to send to a connected client.</param>
    public void BroadcastMessage(BaseOutboundMessage message);
}