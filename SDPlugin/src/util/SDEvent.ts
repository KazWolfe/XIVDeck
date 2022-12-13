import {
    DialPressEvent,
    DialRotateEvent,
    KeyDownEvent,
    KeyUpEvent, TitleParametersDidChangeEvent, TouchTapEvent
} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";

export type InteractiveEvent = KeyUpEvent | KeyDownEvent
    | DialRotateEvent | DialPressEvent | TouchTapEvent
    | TitleParametersDidChangeEvent;