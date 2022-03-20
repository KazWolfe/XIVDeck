export type GlobalSettings = {
    ws: {
        port: number
    }
}

let DefaultGlobalSettings: GlobalSettings = {
    ws: {
        port: 37984
    }
}

export { DefaultGlobalSettings };