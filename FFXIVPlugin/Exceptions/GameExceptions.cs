using System;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.Exceptions; 


public class IllegalGameStateException : InvalidOperationException {
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

public class ActionNotFoundException : ArgumentException {
    public ActionNotFoundException(HotbarSlotType actionType, uint actionId) : 
        base(string.Format(UIStrings.Exceptions_ActionNotFound, actionType, actionId)) { }
}