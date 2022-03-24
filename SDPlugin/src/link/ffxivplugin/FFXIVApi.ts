import {WebUtils} from "../../util/WebUtils";
import {FFXIVAction, FFXIVClass, FFXIVHotbarSlot} from "./GameTypes";
import {FFXIVPluginLink} from "./FFXIVPluginLink";

type HTTPVerb = "GET" | "HEAD" | "POST" | "PUT" | "PATCH" | "DELETE"

export class FFXIVApi {
    private static getBaseUrl(): string | null {
        return FFXIVPluginLink.instance.baseUrl;
    }
    
    private static async _requestWrapper(url: string, method: HTTPVerb = "GET", body: unknown = null) : Promise<unknown> {
        if (this.getBaseUrl() == null) {
            throw new Error("XIV API not initialized yet! Requests should not be getting made...")
        }
        
        let response: Response
        if (method == "GET" || method == "HEAD" ) {
            response = await fetch(url, {method: method})
        } else {
            response = await fetch(url, {method: method, body: JSON.stringify(body)})
        }
        
        if (!response.ok) {
            throw new Error(`Got error ${response.status} (${response.statusText}) from plugin`)
        }
        
        let contentType = response.headers.get('content-type')
        if (contentType && contentType.indexOf("application/json") !== -1) {
            return await response.json();
        }
        
        return;
    }

    public static async getIcon(iconId: number, hq: boolean = false): Promise<string> {
        return await WebUtils.urlContentToDataUri(this.getBaseUrl() + `/icon/${iconId}?hq=${hq}`)
    }
    
    public static async runTextCommand(command: string) : Promise<void> {
        await FFXIVApi._requestWrapper(this.getBaseUrl() + `/command`, "POST", {
            "command": command
        });
    }

    public static Hotbar = class {
        private static get base(): string { return FFXIVApi.getBaseUrl() + "/hotbar" }

        public static async getHotbarSlot(hotbarId: number, slotId: number): Promise<FFXIVHotbarSlot> {
            return await FFXIVApi._requestWrapper(FFXIVApi.Hotbar.base + `/${hotbarId}/${slotId}`) as FFXIVHotbarSlot;
        }

        public static async triggerHotbarSlot(hotbarId: number, slotId: number): Promise<void> {
            await FFXIVApi._requestWrapper(FFXIVApi.Hotbar.base + `/${hotbarId}/${slotId}/execute`, "POST");
        }
    }

    public static GameClass = class {
        private static get base(): string { return FFXIVApi.getBaseUrl() + "/classes" }

        public static async getClasses(unlocked: boolean = false): Promise<FFXIVClass[]> {
            return await FFXIVApi._requestWrapper(FFXIVApi.GameClass.base + (unlocked ? "/available" : "")) as FFXIVClass[];
        }

        public static async getClass(classId: number): Promise<FFXIVClass> {
            return await FFXIVApi._requestWrapper(FFXIVApi.GameClass.base + `/${classId}`) as FFXIVClass;
        }

        public static async triggerClass(classId: number): Promise<void> {
            await FFXIVApi._requestWrapper(FFXIVApi.GameClass.base + `/${classId}/execute`, "POST");
        }
    }
    
    public static Action = class {
        private static get base() : string { return FFXIVApi.getBaseUrl() + "/action" }

        public static async getActions() : Promise<Map<string, FFXIVAction[]>> {
            // This gets deserialized to object, so we need to convert it to a Map for our iterators and all to work.
            let obj = await FFXIVApi._requestWrapper(FFXIVApi.Action.base) as object;
            return new Map<string, FFXIVAction[]>(Object.entries(obj));
        }
        
        public static async getActionsByType(type: string) : Promise<FFXIVAction[]> {
            return await FFXIVApi._requestWrapper(FFXIVApi.Action.base + `/${type}`) as FFXIVAction[];
        }

        public static async getAction(type: string, id: number) : Promise<FFXIVAction> {
            return await FFXIVApi._requestWrapper(FFXIVApi.Action.base + `/${type}/${id}`) as FFXIVAction;
        }

        public static async executeAction(type: string, id: number) : Promise<void> {
            await FFXIVApi._requestWrapper(FFXIVApi.Action.base + `/${type}/${id}/execute`, "POST");
        }
    }
}
