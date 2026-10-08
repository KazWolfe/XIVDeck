export enum RpcErrorCode {
    Unspecified = 1000,
    InvalidRequest = 1001,
    NotFound = 1002,
    IllegalGameState = 1003,
    UserNotLoggedIn = 1004,
    ActionLocked = 1005,
}

export function isContractError(code: number): code is RpcErrorCode {
    return Object.values(RpcErrorCode).includes(code);
}
