namespace XIVDeck.FFXIVPlugin.Contract;

/// <summary>
/// The JSON-RPC <c>error.code</c> for failures that are part of the contract. Anything else is an unexpected failure
/// and carries StreamJsonRpc's default error instead.
/// </summary>
public enum RpcErrorCode {
    Unspecified = 1000,
    InvalidRequest = 1001,
    NotFound = 1002,

    /// <summary>E.g. an empty macro or a missing gearset.</summary>
    IllegalGameState = 1003,

    UserNotLoggedIn = 1004,
    ActionLocked = 1005,
}
