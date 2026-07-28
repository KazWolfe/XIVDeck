import streamDeck from "@elgato/streamdeck";
import {JsonValue} from "@elgato/utils";
import type {PiPushEvents} from "./messages/PiPushEvents";

export async function pushToPropertyInspector<K extends keyof PiPushEvents>(event: K, data: PiPushEvents[K]): Promise<void> {
    await streamDeck.ui.sendToPropertyInspector({event, data} as unknown as JsonValue);
}
