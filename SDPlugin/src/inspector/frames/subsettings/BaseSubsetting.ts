import { EmotePayload } from "../../../button/payloads/actions/EmotePayload";

export interface BaseSubsetting {
    onUpdate?: (payload: object) => void;
    
    getHtml(): HTMLElement;
}