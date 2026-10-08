import {JsonObject} from "@elgato/utils";

export interface SerializableGameClass extends JsonObject {
    id: number;
    name: string;
    abbreviation: string;
    categoryName: string;
    sortOrder: number;
    iconId: number;
    parentClass: number;
}

export interface GetClassesResponse {
    classes: SerializableGameClass[];
}
