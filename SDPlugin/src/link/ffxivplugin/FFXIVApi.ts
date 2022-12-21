import { WebUtils } from "../../util/WebUtils";
import { Aetheryte, FFXIVAction, FFXIVClass, FFXIVHotbarSlot, VolumePayload } from "./GameTypes";
import { FFXIVPluginLink } from "./FFXIVPluginLink";

type HTTPVerb = "GET" | "HEAD" | "POST" | "PUT" | "PATCH" | "DELETE"

export class FFXIVApi {
    private static getBaseUrl(): string | undefined {
        return FFXIVPluginLink.instance.baseUrl;
    }

    private static async _requestWrapper(url: string, method: HTTPVerb = "GET", queryParams?: Record<string, string>, body?: unknown): Promise<Response | any> {
        if (this.getBaseUrl() == null || !FFXIVPluginLink.instance.isReady() || FFXIVPluginLink.instance.apiKey == "") {
            throw new Error("XIV API not initialized yet! Requests should not be getting made...");
        }

        let headers = {
            "Authorization": `Bearer ${FFXIVPluginLink.instance.apiKey}`
        };

        let urlObject = new URL(url);

        if (queryParams != null) {
            urlObject.search = new URLSearchParams(queryParams).toString();
        }

        let response: Response;
        if (method == "GET" || method == "HEAD") {
            response = await fetch(urlObject.toString(), {method: method, headers: headers});
        } else {
            response = await fetch(urlObject.toString(), {
                method: method,
                body: JSON.stringify(body),
                headers: headers
            });
        }

        if (!response.ok) {
            throw new Error(`Got error ${response.status} (${response.statusText}) from plugin`);
        }

        let contentType = response.headers.get('content-type');
        if (contentType && contentType.indexOf("application/json") !== -1) {
            return await response.json();
        }

        return response;
    }

    public static async getIcon(iconId: number, hq: boolean = false): Promise<string> {
        let queryParams: Record<string, string> = {};
        if (hq) queryParams["hq"] = `${hq}`;

        let response = await this._requestWrapper(this.getBaseUrl() + `/icon/${iconId}`, "GET", queryParams);
        let blob = await response.blob();

        return new Promise(callback => {
            let reader = new FileReader();
            reader.onload = function () {
                callback(this.result!.toString()!);
            };
            reader.readAsDataURL(blob);
        });
    }

    public static async runTextCommand(command: string): Promise<void> {
        await FFXIVApi._requestWrapper(this.getBaseUrl() + `/command`, "POST", undefined, {
            "command": command
        });
    }

    public static Hotbar = class {
        private static get base(): string {
            return FFXIVApi.getBaseUrl() + "/hotbar";
        }

        public static async getHotbarSlot(hotbarId: number, slotId: number): Promise<FFXIVHotbarSlot> {
            return await FFXIVApi._requestWrapper(FFXIVApi.Hotbar.base + `/${hotbarId}/${slotId}`) as FFXIVHotbarSlot;
        }

        public static async triggerHotbarSlot(hotbarId: number, slotId: number): Promise<void> {
            await FFXIVApi._requestWrapper(FFXIVApi.Hotbar.base + `/${hotbarId}/${slotId}/execute`, "POST");
        }
    };

    public static GameClass = class {
        private static get base(): string {
            return FFXIVApi.getBaseUrl() + "/classes";
        }

        public static async getClasses(unlocked: boolean = false): Promise<FFXIVClass[]> {
            return await FFXIVApi._requestWrapper(FFXIVApi.GameClass.base + (unlocked ? "/available" : "")) as FFXIVClass[];
        }

        public static async getClass(classId: number): Promise<FFXIVClass> {
            return await FFXIVApi._requestWrapper(FFXIVApi.GameClass.base + `/${classId}`) as FFXIVClass;
        }

        public static async triggerClass(classId: number): Promise<void> {
            await FFXIVApi._requestWrapper(FFXIVApi.GameClass.base + `/${classId}/execute`, "POST");
        }
    };

    public static Action = class {
        private static get base(): string {
            return FFXIVApi.getBaseUrl() + "/action";
        }

        public static async getActions(): Promise<Map<string, FFXIVAction[]>> {
            // This gets deserialized to object, so we need to convert it to a Map for our iterators and all to work.
            let obj = await FFXIVApi._requestWrapper(FFXIVApi.Action.base) as object;
            return new Map<string, FFXIVAction[]>(Object.entries(obj));
        }

        public static async getActionsByType(type: string): Promise<FFXIVAction[]> {
            return await FFXIVApi._requestWrapper(FFXIVApi.Action.base + `/${type}`) as FFXIVAction[];
        }

        public static async getAction(type: string, id: number): Promise<FFXIVAction> {
            return await FFXIVApi._requestWrapper(FFXIVApi.Action.base + `/${type}/${id}`) as FFXIVAction;
        }

        public static async executeAction(type: string, id: number, payload: unknown = undefined): Promise<void> {
            await FFXIVApi._requestWrapper(
                FFXIVApi.Action.base + `/${type}/${id}/execute`,
                "POST",
                undefined,
                payload);
        }
    };

    public static Volume = class {
        private static get base(): string {
            return FFXIVApi.getBaseUrl() + "/volume";
        }

        public static async getChannels(): Promise<Map<string, VolumePayload>> {
            let obj = await FFXIVApi._requestWrapper(FFXIVApi.Volume.base) as object;
            return new Map<string, VolumePayload>(Object.entries(obj));
        }

        public static async getChannel(channel: string) {
            return await FFXIVApi._requestWrapper(FFXIVApi.Volume.base + `/${channel}`) as VolumePayload;
        }
    };
    
    public static Teleport = class {
        private static get base(): string {
            return FFXIVApi.getBaseUrl() + "/teleport";
        }
        
        public static async getAetherytes(): Promise<Aetheryte[]> {
            return await FFXIVApi._requestWrapper(FFXIVApi.Teleport.base) as Aetheryte[];
        }

        public static async triggerTeleport(aetheryteId: number, subId: number = 0): Promise<void> {
            await FFXIVApi._requestWrapper(FFXIVApi.Teleport.base + `/${aetheryteId}/${subId}/execute`, "POST");
        }
    };
}
