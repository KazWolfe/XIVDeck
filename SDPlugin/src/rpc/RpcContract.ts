import {ServerHello} from "./messages/Connection";
import {GetClassesResponse, SerializableGameClass} from "./messages/ClassJob";
import {ActionEntry, ActionTypeUpdateBatch, GetActionsResponse} from "./messages/Action";
import {HotbarChangeNotification, HotbarWatchRequest} from "./messages/Hotbar";
import {ActionAppearance, CooldownNotification} from "./messages/ActionAppearance";
import {GetAllChannelsResponse, VolumeState} from "./messages/Volume";
import {ClearIconCacheMessage, GetIconResponse} from "./messages/Icon";

export interface RpcRequestMap {
    "Connection.Initialize": {params: {clientHello: {clientVersion: string; clientType: string}}; result: ServerHello};
    "ClassJob.GetClass": {params: {id: number}; result: SerializableGameClass};
    "ClassJob.GetAvailableClasses": {params?: undefined; result: GetClassesResponse};
    "ClassJob.SwitchClass": {params: {id: number}; result: void};
    "Icon.GetIcon": {params: {iconId: number}; result: GetIconResponse};
    "Command.ExecuteCommand": {params: {commandRequest: {command: string}}; result: void};
    "Action.GetActionEntry": {params: {type: string; id: number}; result: ActionEntry};
    "Action.GetActionAppearance": {params: {type: string; id: number}; result: ActionAppearance};
    "Action.GetActions": {params?: undefined; result: GetActionsResponse};
    "Action.ExecuteAction": {params: {type: string; id: number; payload?: unknown}; result: void};
    "Hotbar.GetHotbarSlot": {params: {hotbarId: number; slotId: number}; result: ActionAppearance};
    "Hotbar.TriggerHotbarSlot": {params: {hotbarId: number; slotId: number}; result: void};
    "Hotbar.SetWatchedSlots": {params: {watchRequest: HotbarWatchRequest}; result: void};
    "Volume.GetAllChannels": {params?: undefined; result: GetAllChannelsResponse};
    "Volume.GetChannel": {params: {channel: string}; result: VolumeState};
    "Volume.SetChannel": {params: {channel: string; volume?: number; muted?: boolean}; result: VolumeState};
    "Volume.DeltaChannel": {params: {channel: string; delta: number}; result: VolumeState};
    "Volume.ToggleMuteChannel": {params: {channel: string}; result: VolumeState};
}

export interface RpcNotificationMap {
    "Volume.VolumeChanged": [VolumeState];
    "Hotbar.OnHotbarChanged": [HotbarChangeNotification];
    "Action.ActionTypeUpdate": [ActionTypeUpdateBatch];
    "Cooldown.CooldownStart": [CooldownNotification];
    "Icon.ClearIconCache": [ClearIconCacheMessage];
}
