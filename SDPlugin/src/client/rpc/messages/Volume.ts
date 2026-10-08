export interface VolumeState {
    channel: string;
    volume: number;
    muted: boolean;
}

export interface GetAllChannelsResponse {
    channels: Record<string, VolumeState>;
}
