import {injectable} from "inversify";
import streamDeck, {
    DialAction,
    DialDownEvent,
    DialUpEvent,
    DialRotateEvent,
    DidReceiveSettingsEvent,
    KeyAction,
    KeyDownEvent,
    KeyUpEvent,
    TouchTapEvent,
    WillAppearEvent,
    WillDisappearEvent,
} from "@elgato/streamdeck";
import {DisposableStack, JsonObject} from "@elgato/utils";
import {Control} from "./Control";
import {ControlContext} from "./ControlContext";
import {ControlInput} from "./ControlInput";
import {ControlFactory} from "./ControlFactory";
import {SettingsMigrator} from "#/settings/SettingsMigrator";

interface IAlertableActionEvent {
    readonly type: string;
    readonly action: KeyAction<JsonObject> | DialAction<JsonObject>;
}

@injectable()
export class ControlDispatcher implements Disposable {
    private readonly _controls = new Map<string, Control<JsonObject>>();
    private readonly _subscriptions = new DisposableStack();

    public constructor(private readonly factory: ControlFactory) {
    }

    public get(actionId: string): Control<JsonObject> | undefined {
        return this._controls.get(actionId);
    }

    public initialize(): void {
        this._subscriptions.use(streamDeck.actions.onWillAppear(this.onWillAppear.bind(this)));
        this._subscriptions.use(streamDeck.actions.onWillDisappear(this.onWillDisappear.bind(this)));
        this._subscriptions.use(streamDeck.actions.onKeyDown(this.onKeyDown.bind(this)));
        this._subscriptions.use(streamDeck.actions.onKeyUp(this.onKeyUp.bind(this)));
        this._subscriptions.use(streamDeck.actions.onDialDown(this.onDialDown.bind(this)));
        this._subscriptions.use(streamDeck.actions.onDialUp(this.onDialUp.bind(this)));
        this._subscriptions.use(streamDeck.actions.onDialRotate(this.onDialRotate.bind(this)));
        this._subscriptions.use(streamDeck.actions.onTouchTap(this.onTouchTap.bind(this)));
        this._subscriptions.use(streamDeck.settings.onDidReceiveSettings(this.onDidReceiveSettings.bind(this)));
    }

    /** Stops routing Stream Deck events and disposes every live control. */
    public [Symbol.dispose](): void {
        this._subscriptions.dispose();

        for (const control of this._controls.values()) {
            control[Symbol.dispose]();
        }

        this._controls.clear();
    }

    private async onWillAppear(ev: WillAppearEvent<JsonObject>): Promise<void> {
        let control: Control<JsonObject> | undefined;
        try {
            control = this.factory.create(ev);
        } catch (err) {
            streamDeck.logger.error(`Failed to create control for "${ev.action.manifestId}" (${ev.action.id}):`, err);
            return;
        }

        if (!control) {
            return;
        }

        // Track before awaiting anything, so a willDisappear that races the initial load still disposes it.
        this._controls.set(ev.action.id, control);

        try {
            await this.factory.loadSettings(control, ev);
        } catch (err) {
            streamDeck.logger.error(`Failed to load settings for control ${ev.action.id}:`, err);
        }
    }

    private onWillDisappear(ev: WillDisappearEvent<JsonObject>): void {
        const control = this.get(ev.action.id);
        if (!control) {
            return;
        }

        this._controls.delete(ev.action.id);
        control[Symbol.dispose]();
    }

    private async onKeyDown(ev: KeyDownEvent<JsonObject>): Promise<void> {
        const control = this.get(ev.action.id);
        if (!control || !ControlInput.isKeyControl(control) || !control.onKeyDown) {
            return;
        }

        try {
            await control.onKeyDown(ev);
        } catch (err) {
            await this.handleInputError(ev, err);
        }
    }

    private async onKeyUp(ev: KeyUpEvent<JsonObject>): Promise<void> {
        const control = this.get(ev.action.id);

        if (!control || !ControlInput.isKeyControl(control) || !control.onKeyUp) {
            return;
        }

        try {
            await control.onKeyUp(ev);
        } catch (err) {
            await this.handleInputError(ev, err);
        }
    }

    private async onDialDown(ev: DialDownEvent<JsonObject>): Promise<void> {
        const control = this.get(ev.action.id);
        if (!control || !ControlInput.isDialControl(control) || !control.onDialDown) {
            return;
        }

        try {
            await control.onDialDown(ev);
        } catch (err) {
            await this.handleInputError(ev, err);
        }
    }

    private async onDialUp(ev: DialUpEvent<JsonObject>): Promise<void> {
        const control = this.get(ev.action.id);
        if (!control || !ControlInput.isDialControl(control) || !control.onDialUp) {
            return;
        }

        try {
            await control.onDialUp(ev);
        } catch (err) {
            await this.handleInputError(ev, err);
        }
    }

    private async onDialRotate(ev: DialRotateEvent<JsonObject>): Promise<void> {
        const control = this.get(ev.action.id);
        if (!control || !ControlInput.isDialControl(control) || !control.onDialRotate) {
            return;
        }

        try {
            await control.onDialRotate(ev);
        } catch (err) {
            await this.handleInputError(ev, err);
        }
    }

    private async onTouchTap(ev: TouchTapEvent<JsonObject>): Promise<void> {
        const control = this.get(ev.action.id);
        if (!control || !ControlInput.isDialControl(control) || !control.onTouchTap) {
            return;
        }

        try {
            await control.onTouchTap(ev);
        } catch (err) {
            await this.handleInputError(ev, err);
        }
    }

    private async onDidReceiveSettings(ev: DidReceiveSettingsEvent<JsonObject>): Promise<void> {
        const control = this.get(ev.action.id);
        if (!control) {
            return;
        }

        try {
            await control.applySettings(SettingsMigrator.orEmpty(ev.payload.settings));
        } catch (err) {
            streamDeck.logger.error(`Could not apply settings to ${ev.action.id}:`, err, ev.payload.settings);
        }
    }

    private async handleInputError(ev: IAlertableActionEvent, err: unknown): Promise<void> {
        ControlContext.logActionError(ev.action, `handling ${ev.type} for`, err);

        try {
            await ev.action.showAlert();
        } catch (alertErr) {
            streamDeck.logger.error(`Couldn't show an alert on ${ev.action.id}:`, alertErr);
        }
    }
}
