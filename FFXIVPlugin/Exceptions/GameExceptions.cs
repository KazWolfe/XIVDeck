using System;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace XIVDeck.FFXIVPlugin.Exceptions; 


public class IllegalGameStateException : InvalidOperationException {
    public IllegalGameStateException(string message) : 
        base(message) { }
}

public class PlayerNotLoggedInException : IllegalGameStateException {
    public PlayerNotLoggedInException() :
        base("A player is not logged in to the game.") { } 
}
public class ActionLockedException : IllegalGameStateException {
    public ActionLockedException(HotbarSlotType type, uint actionId) :
        base($"The {type} ID {actionId} is has not been unlocked and cannot be used.") { }

    public ActionLockedException(string message) : 
        base(message) { }
}

public class ActionNotFoundException : ArgumentException {
    public ActionNotFoundException(HotbarSlotType actionType, uint actionId) : 
        base($"No {actionType} with ID {actionId} was found") { }
}