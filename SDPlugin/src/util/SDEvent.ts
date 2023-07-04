import {
    DialDownEvent,
    DialPressEvent,
    DialRotateEvent, DialUpEvent,
    KeyDownEvent,
    KeyUpEvent, TitleParametersDidChangeEvent, TouchTapEvent
} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";

export type InteractiveEvent = KeyUpEvent | KeyDownEvent
    | DialDownEvent | DialRotateEvent | DialPressEvent | DialUpEvent | TouchTapEvent
    | TitleParametersDidChangeEvent;