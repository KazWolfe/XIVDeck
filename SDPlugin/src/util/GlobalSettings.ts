export type GlobalSettings = {
    ws: {
        hostname?: string,
        port: number
    }
}

let DefaultGlobalSettings: GlobalSettings = {
    ws: {
        port: 37984
    }
}

export { DefaultGlobalSettings };