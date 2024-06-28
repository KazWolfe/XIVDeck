using System;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.Exceptions;

public interface IXIVDeckException { }

public class IllegalGameStateException : InvalidOperationException, IXIVDeckException {
    public IllegalGameStateException(string message) :
        base(message) { }
}

public class PlayerNotLoggedInException : IllegalGameStateException {
    public PlayerNotLoggedInException() :
        base(UIStrings.Exceptions_PlayerNotLoggedIn) { }
}
public class ActionLockedException : IllegalGameStateException {
    public ActionLockedException(HotbarSlotType type, uint actionId) :
        base(string.Format(UIStrings.Exceptions_ActionLocked, type, actionId)) { }

    public ActionLockedException(string message) :
        base(message) { }
}

public class ActionNotFoundException : ArgumentException, IXIVDeckException {
    public ActionNotFoundException(HotbarSlotType actionType, uint actionId) :
        base(string.Format(UIStrings.Exceptions_ActionNotFound, actionType, actionId)) { }

    public ActionNotFoundException(string message) : base(message) { }
}
