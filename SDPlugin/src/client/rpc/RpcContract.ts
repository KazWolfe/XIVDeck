import {ServerHello} from "./messages/Connection";
import {GetClassesResponse, SerializableGameClass} from "./messages/ClassJob";
import {ActionEntry, ActionTypeUpdateBatch, GetActionsResponse} from "./messages/Action";
import {HotbarChangeNotification, HotbarWatchRequest} from "./messages/Hotbar";
import {ActionAppearance, ActionCooldownDetail, CooldownNotification} from "./messages/ActionAppearance";
import {GetAllChannelsResponse, VolumeState} from "./messages/Volume";
import {ClearIconCacheMessage, GetIconResponse} from "./messages/Icon";
import {FocusState} from "./messages/GameState";

export interface RpcRequestMap {
    "Connection.Initialize": {
        params: { clientHello: { clientVersion: string; clientType: string } };
        result: ServerHello
    };

    "Action.ExecuteAction": { params: { type: string; id: number; payload?: unknown }; result: void };
    "Action.GetActionAppearance": { params: { type: string; id: number }; result: ActionAppearance };
    "Action.GetActionEntry": { params: { type: string; id: number }; result: ActionEntry };
    "Action.GetActions": { params?: undefined; result: GetActionsResponse };

    "ClassJob.GetClass": { params: { id: number }; result: SerializableGameClass };
    "ClassJob.GetAvailableClasses": { params?: undefined; result: GetClassesResponse };
    "ClassJob.SwitchClass": { params: { id: number }; result: void };

    "Command.ExecuteCommand": { params: { commandRequest: { command: string } }; result: void };

    "Cooldown.GetActionCooldown": { params: { actionType: string; actionId: number }; result: ActionCooldownDetail };

    "GameState.GetFocusState": { params?: undefined; result: FocusState };

    "Hotbar.GetHotbarSlot": { params: { hotbarId: number; slotId: number }; result: ActionAppearance };
    "Hotbar.SetWatchedSlots": { params: { watchRequest: HotbarWatchRequest }; result: void };
    "Hotbar.TriggerHotbarSlot": { params: { hotbarId: number; slotId: number }; result: void };

    "Icon.GetIcon": { params: { iconId: number }; result: GetIconResponse };

    "Volume.DeltaChannel": { params: { channel: string; delta: number }; result: VolumeState };
    "Volume.GetAllChannels": { params?: undefined; result: GetAllChannelsResponse };
    "Volume.GetChannel": { params: { channel: string }; result: VolumeState };
    "Volume.SetChannel": { params: { channel: string; volume?: number; muted?: boolean }; result: VolumeState };
    "Volume.ToggleMuteChannel": { params: { channel: string }; result: VolumeState };
}

export interface RpcNotificationMap {
    "Action.ActionTypeUpdate": [ActionTypeUpdateBatch];
    "Cooldown.CooldownStart": [CooldownNotification];
    "GameState.FocusChanged": [FocusState];
    "Hotbar.OnHotbarChanged": [HotbarChangeNotification];
    "Icon.ClearIconCache": [ClearIconCacheMessage];
    "Volume.VolumeChanged": [VolumeState];
}
