using System;

namespace XIVDeck.FFXIVPlugin.RpcServer;

// HACK: Flag exception when a transport fails to start but shouldn't display a generic error.
public class HandledTransportStartException(string message, Exception? innerException = null)
    : Exception(message, innerException);
