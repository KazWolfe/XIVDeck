import {Container, injectable, Newable} from "inversify";
import streamDeck, {DidReceiveSettingsEvent, WillAppearEvent} from "@elgato/streamdeck";
import {JsonObject} from "@elgato/utils";
import {Control} from "./Control";
import {ControlContext} from "./ControlContext";
import {IDialControl, IKeyControl} from "./ControlInput";
import {ClientProxy} from "./proxy/ClientProxy";
import {HotbarTrackerProxy} from "./proxy/HotbarTrackerProxy";
import {CooldownTrackerProxy} from "./proxy/CooldownTrackerProxy";
import {GameNotificationProxy} from "./proxy/GameNotificationProxy";
import {VirtualSlotPresenter} from "./virtual_slot/VirtualSlotPresenter";
import {CommandButton} from "./controls/CommandButton";
import {HotbarButton} from "./controls/HotbarButton";
import {ExecActionButton} from "./controls/ExecActionButton";
import {MacroButton} from "./controls/MacroButton";
import {ClassButton} from "./controls/ClassButton";
import {VolumeKeyControl} from "./controls/VolumeKeyControl";
import {VolumeDialControl} from "./controls/VolumeDialControl";
import {MigrationChain, SettingsMigrator} from "#/settings/SettingsMigrator";
import {CommandSettingsMigrations} from "#/settings/migrations/CommandSettingsMigrations";
import {HotbarSettingsMigrations} from "#/settings/migrations/HotbarSettingsMigrations";
import {ExecActionSettingsMigrations} from "#/settings/migrations/ExecActionSettingsMigrations";
import {MacroSettingsMigrations} from "#/settings/migrations/MacroSettingsMigrations";
import {ClassSettingsMigrations} from "#/settings/migrations/ClassSettingsMigrations";
import {VolumeSettingsMigrations} from "#/settings/migrations/VolumeSettingsMigrations";

interface IControlClasses<TSettings extends JsonObject> {
    Keypad?: Newable<Control<TSettings> & IKeyControl>;
    Encoder?: Newable<Control<TSettings> & IDialControl>;

    Neo?: Newable<Control<TSettings>>;
}

interface IControlRegistration<TSettings extends JsonObject = JsonObject> {
    migrations: MigrationChain<TSettings>;
    controls: IControlClasses<TSettings>;
}

@injectable()
export class ControlFactory {
    private readonly _registrations: Record<string, IControlRegistration> = {
        "dev.wolf.xivdeck.sdplugin.actions.sendcommand":
            ControlFactory.register(CommandSettingsMigrations, {Keypad: CommandButton}),
        "dev.wolf.xivdeck.sdplugin.actions.exechotbar":
            ControlFactory.register(HotbarSettingsMigrations, {Keypad: HotbarButton}),
        "dev.wolf.xivdeck.sdplugin.actions.execaction":
            ControlFactory.register(ExecActionSettingsMigrations, {Keypad: ExecActionButton}),
        "dev.wolf.xivdeck.sdplugin.actions.execmacro":
            ControlFactory.register(MacroSettingsMigrations, {Keypad: MacroButton}),
        "dev.wolf.xivdeck.sdplugin.actions.switchclass":
            ControlFactory.register(ClassSettingsMigrations, {Keypad: ClassButton}),
        "dev.wolf.xivdeck.sdplugin.actions.volume":
            ControlFactory.register(VolumeSettingsMigrations, {Keypad: VolumeKeyControl, Encoder: VolumeDialControl}),
    };

    private static register<TSettings extends JsonObject>(
        migrations: MigrationChain<TSettings>,
        controls: NoInfer<IControlClasses<TSettings>>,
    ): IControlRegistration {
        return {migrations, controls};
    }

    public constructor(private readonly root: Container) {
    }

    /**
     * Create the control and all associated services for its lifetime.
     * @param ev
     */
    public create(ev: WillAppearEvent<JsonObject>): Control<JsonObject> | undefined {
        const registration = this.registrationOf(ev.action.manifestId);
        if (!registration) {
            streamDeck.logger.warn(`No control registered for action "${ev.action.manifestId}".`);
            return undefined;
        }

        const controlClass: Newable<Control<JsonObject>> | undefined = registration.controls[ev.action.controllerType];
        if (!controlClass) {
            streamDeck.logger.warn(`Action "${ev.action.manifestId}" is not supported on a ${ev.action.controllerType}.`);
            return undefined;
        }

        const scope = new Container({parent: this.root});
        scope.bind(ControlContext).toConstantValue(new ControlContext(ev.action));
        scope.bind(controlClass).toSelf().inSingletonScope();
        scope.bind(ClientProxy).toSelf().inSingletonScope();
        scope.bind(HotbarTrackerProxy).toSelf().inSingletonScope();
        scope.bind(CooldownTrackerProxy).toSelf().inSingletonScope();
        scope.bind(GameNotificationProxy).toSelf().inSingletonScope();
        scope.bind(VirtualSlotPresenter).toSelf().inSingletonScope();

        return scope.get(controlClass);
    }

    public async loadSettings(control: Control<JsonObject>, ev: WillAppearEvent<JsonObject>): Promise<void> {
        const raw = ev.payload.settings;

        if (SettingsMigrator.isEmpty(raw)) {
            // call `onSettingsChanged` in the control anyways, in case it wants to do something.
            await control.applySettings(undefined);
            return;
        }

        const migrations = this.registrationOf(ev.action.manifestId)!.migrations;
        const migrated = SettingsMigrator.migrate(migrations, raw);
        if (migrated !== raw) {
            await ev.action.setSettings(migrated);
        }

        await control.applySettings(migrated);
    }

    private registrationOf(manifestId: string): IControlRegistration | undefined {
        return this._registrations[manifestId.toLowerCase()];
    }
}
