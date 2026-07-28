import type {ConnectionInfo} from "./ConnectionInfo";

export interface PiPushEvents {
    connectionStateChanged: ConnectionInfo;
}
