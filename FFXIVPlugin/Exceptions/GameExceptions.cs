using System;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Contract;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.Exceptions;

/// <summary>
/// A failure whose category is part of the RPC contract: it reaches the client as <see cref="ErrorCode"/>.
/// </summary>
public interface IRpcException {
    RpcErrorCode ErrorCode { get; }
}

/// <summary>
/// An <see cref="IRpcException"/> the player should also see, as an in-game toast.
/// </summary>
public interface IXIVDeckException : IRpcException;

public class IllegalGameStateException(string message) : InvalidOperationException(message), IXIVDeckException {
    public virtual RpcErrorCode ErrorCode => RpcErrorCode.IllegalGameState;
}

public class PlayerNotLoggedInException() : IllegalGameStateException(UIStrings.PlayerNotLoggedInException_Message) {
    public override RpcErrorCode ErrorCode => RpcErrorCode.UserNotLoggedIn;
}
public class ActionLockedException : IllegalGameStateException {
    public override RpcErrorCode ErrorCode => RpcErrorCode.ActionLocked;

    public ActionLockedException(HotbarSlotType type, uint actionId) :
        base(string.Format(UIStrings.ActionLockedException_Message, type, actionId)) { }

    public ActionLockedException(string message) :
        base(message) { }
}

public class ActionNotFoundException : ArgumentException, IXIVDeckException {
    public RpcErrorCode ErrorCode => RpcErrorCode.NotFound;

    public ActionNotFoundException(HotbarSlotType actionType, uint actionId) :
        base(string.Format(UIStrings.ActionNotFoundException_Message, actionType, actionId)) { }

    public ActionNotFoundException(string message) : base(message) { }
}

public class ActionTypeNotFoundException(string typeName)
    : ArgumentException(string.Format(UIStrings.ActionTypeNotFoundException_Message, typeName)), IXIVDeckException {
    public RpcErrorCode ErrorCode => RpcErrorCode.NotFound;
}

public class MissingCommandException()
    : ArgumentException(UIStrings.MissingCommandException_Message), IXIVDeckException {
    public RpcErrorCode ErrorCode => RpcErrorCode.InvalidRequest;
}

public class UnsafeCommandException()
    : ArgumentException(UIStrings.UnsafeCommandException_Message), IXIVDeckException {
    public RpcErrorCode ErrorCode => RpcErrorCode.InvalidRequest;
}

public class ActionInvalidException(string message) : ArgumentException(message), IXIVDeckException {
    public RpcErrorCode ErrorCode => RpcErrorCode.InvalidRequest;
}

/// <summary>
/// A missing icon. Deliberately not an <see cref="IXIVDeckException"/>: icons are fetched in the background while
/// rendering, so a miss shouldn't toast the player.
/// </summary>
public class IconNotFoundException(int iconId) : Exception($"Icon {iconId} was not found."), IRpcException {
    public RpcErrorCode ErrorCode => RpcErrorCode.NotFound;
}
