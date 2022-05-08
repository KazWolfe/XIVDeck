﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace XIVDeck.FFXIVPlugin.Resources.Localization {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class UIStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal UIStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("XIVDeck.FFXIVPlugin.Resources.Localization.UIStrings", typeof(UIStrings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No registered action of type {0} was found..
        /// </summary>
        internal static string ActionController_UnknownActionTypeError {
            get {
                return ResourceManager.GetString("ActionController_UnknownActionTypeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to API key missing or invalid..
        /// </summary>
        internal static string AuthModule_BadAPIKeyError {
            get {
                return ResourceManager.GetString("AuthModule_BadAPIKeyError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot switch to a class with ID less than 1..
        /// </summary>
        internal static string ClassController_ClassLessThan1Error {
            get {
                return ResourceManager.GetString("ClassController_ClassLessThan1Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A class with ID {0} does not exist!.
        /// </summary>
        internal static string ClassController_InvalidClassIdError {
            get {
                return ResourceManager.GetString("ClassController_InvalidClassIdError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Couldn&apos;t switch to {0} because you don&apos;t have a gearset for this class. Make one and try again..
        /// </summary>
        internal static string ClassController_NoGearsetForClassError {
            get {
                return ResourceManager.GetString("ClassController_NoGearsetForClassError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No Collection with ID {0} exists..
        /// </summary>
        internal static string CollectionStrategy_CollectionNotFoundError {
            get {
                return ResourceManager.GetString("CollectionStrategy_CollectionNotFoundError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A command to run must be specified..
        /// </summary>
        internal static string CommandController_MissingCommandError {
            get {
                return ResourceManager.GetString("CommandController_MissingCommandError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Commands must start with a slash..
        /// </summary>
        internal static string CommandController_NotCommandError {
            get {
                return ResourceManager.GetString("CommandController_NotCommandError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The emote &quot;{0}&quot; does not have an associated text command..
        /// </summary>
        internal static string EmoteStrategy_EmoteDoesntHaveCommandError {
            get {
                return ResourceManager.GetString("EmoteStrategy_EmoteDoesntHaveCommandError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The emote &quot;{0}&quot; isn&apos;t unlocked and therefore can&apos;t be used..
        /// </summary>
        internal static string EmoteStrategy_EmoteLockedError {
            get {
                return ResourceManager.GetString("EmoteStrategy_EmoteLockedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} - ERROR.
        /// </summary>
        internal static string ErrorHandler_ErrorPrefix {
            get {
                return ResourceManager.GetString("ErrorHandler_ErrorPrefix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The {0} ID {1} is has not been unlocked and cannot be used..
        /// </summary>
        internal static string Exceptions_ActionLocked {
            get {
                return ResourceManager.GetString("Exceptions_ActionLocked", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No {0} with ID {1} was found.
        /// </summary>
        internal static string Exceptions_ActionNotFound {
            get {
                return ResourceManager.GetString("Exceptions_ActionNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A player is not logged in to the game..
        /// </summary>
        internal static string Exceptions_PlayerNotLoggedIn {
            get {
                return ResourceManager.GetString("Exceptions_PlayerNotLoggedIn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No command exists for Extra Command {0}. REPORT THIS BUG!.
        /// </summary>
        internal static string ExtraCommandStrategy_NoCommandError {
            get {
                return ResourceManager.GetString("ExtraCommandStrategy_NoCommandError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An action of type {0} with ID {1} does not exist..
        /// </summary>
        internal static string FixedCommandStrategy_ActionNotFoundError {
            get {
                return ResourceManager.GetString("FixedCommandStrategy_ActionNotFoundError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The action with ID {0} is marked as illegal and cannot be used..
        /// </summary>
        internal static string FixedCommandStrategy_IllegalActionError {
            get {
                return ResourceManager.GetString("FixedCommandStrategy_IllegalActionError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A sheet of type {0} does not exist..
        /// </summary>
        internal static string FixedCommandStrategy_SheetNotFoundError {
            get {
                return ResourceManager.GetString("FixedCommandStrategy_SheetNotFoundError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The XIVDeck Stream Deck Plugin is critically out of date and has been disabled..
        /// </summary>
        internal static string ForcedUpdateNag_Headline {
            get {
                return ResourceManager.GetString("ForcedUpdateNag_Headline", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please download and install the latest version of the Stream Deck plugin from the XIVDeck GitHub to continue using XIVDeck..
        /// </summary>
        internal static string ForcedUpdateNag_ResolutionHelp {
            get {
                return ResourceManager.GetString("ForcedUpdateNag_ResolutionHelp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A class with ID {0} does not exist..
        /// </summary>
        internal static string GameClass_NotFoundError {
            get {
                return ResourceManager.GetString("GameClass_NotFoundError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unrecognized job category for class ID {0}. REPORT THIS BUG!.
        /// </summary>
        internal static string GameClass_UncategorizedError {
            get {
                return ResourceManager.GetString("GameClass_UncategorizedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No gearset exists in slot number {0}..
        /// </summary>
        internal static string GearsetStrategy_GearsetNotFoundError {
            get {
                return ResourceManager.GetString("GearsetStrategy_GearsetNotFoundError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The action &quot;{0}&quot; (ID {1}) is marked as illegal and cannot be used..
        /// </summary>
        internal static string GeneralActionStrategy_ActionIllegalError {
            get {
                return ResourceManager.GetString("GeneralActionStrategy_ActionIllegalError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The action &quot;{0}&quot; is not yet unlocked..
        /// </summary>
        internal static string GeneralActionStrategy_ActionLockedError {
            get {
                return ResourceManager.GetString("GeneralActionStrategy_ActionLockedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to When Hotbar ID &gt;= 10, Slot ID must be between 0 and 15.
        /// </summary>
        internal static string HotbarController_CrossHotbarInvalidSlotError {
            get {
                return ResourceManager.GetString("HotbarController_CrossHotbarInvalidSlotError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hotbar ID must be between 0 and 17.
        /// </summary>
        internal static string HotbarController_InvalidHotbarIdError {
            get {
                return ResourceManager.GetString("HotbarController_InvalidHotbarIdError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An invalid hotbar or slot was triggered..
        /// </summary>
        internal static string HotbarController_InvalidHotbarOrSlotError {
            get {
                return ResourceManager.GetString("HotbarController_InvalidHotbarOrSlotError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to When Hotbar ID &lt; 10, Slot ID must be between 0 and 11.
        /// </summary>
        internal static string HotbarController_NormalHotbarInvalidSlotError {
            get {
                return ResourceManager.GetString("HotbarController_NormalHotbarInvalidSlotError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot switch instruments while in Performance mode..
        /// </summary>
        internal static string InstrumentStrategy_CurrentlyPerformingError {
            get {
                return ResourceManager.GetString("InstrumentStrategy_CurrentlyPerformingError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No instrument with ID {0} exists..
        /// </summary>
        internal static string InstrumentStrategy_InstrumentNotFoundError {
            get {
                return ResourceManager.GetString("InstrumentStrategy_InstrumentNotFoundError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Performance mode hasn&apos;t been unlocked yet..
        /// </summary>
        internal static string InstrumentStrategy_PerformanceLockedError {
            get {
                return ResourceManager.GetString("InstrumentStrategy_PerformanceLockedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified macro is empty and cannot be used..
        /// </summary>
        internal static string MacroStrategy_MacroEmptyError {
            get {
                return ResourceManager.GetString("MacroStrategy_MacroEmptyError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Main command action ID {0} is not valid..
        /// </summary>
        internal static string MainCommandStrategy_ActionInvalidError {
            get {
                return ResourceManager.GetString("MainCommandStrategy_ActionInvalidError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The minion &quot;{0}&quot; isn&apos;t unlocked and therefore can&apos;t be used..
        /// </summary>
        internal static string MinionStrategy_MinionLockedError {
            get {
                return ResourceManager.GetString("MinionStrategy_MinionLockedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No minion with ID {0} exists..
        /// </summary>
        internal static string MinionStrategy_MinionNotFoundError {
            get {
                return ResourceManager.GetString("MinionStrategy_MinionNotFoundError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The mount &quot;{0}&quot; isn&apos;t unlocked and therefore can&apos;t be used..
        /// </summary>
        internal static string MountStrategy_MountLockedError {
            get {
                return ResourceManager.GetString("MountStrategy_MountLockedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No mount with ID {0} exists..
        /// </summary>
        internal static string MountStrategy_MountNotFoundError {
            get {
                return ResourceManager.GetString("MountStrategy_MountNotFoundError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Open XIVDeck Download Page.
        /// </summary>
        internal static string Nag_OpenGithubDownloadButton {
            get {
                return ResourceManager.GetString("Nag_OpenGithubDownloadButton", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Open XIVDeck Settings.
        /// </summary>
        internal static string Nag_OpenSettingsButton {
            get {
                return ResourceManager.GetString("Nag_OpenSettingsButton", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The fashion accessory &quot;{0}&quot; isn&apos;t unlocked and therefore can&apos;t be used..
        /// </summary>
        internal static string OrnamentStrategy_OrnamentLockedError {
            get {
                return ResourceManager.GetString("OrnamentStrategy_OrnamentLockedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to API Port.
        /// </summary>
        internal static string SettingsWindow_APIPort {
            get {
                return ResourceManager.GetString("SettingsWindow_APIPort", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Default port: {0}
        ///
        ///Range: {1}-{2}.
        /// </summary>
        internal static string SettingsWindow_APIPort_Help {
            get {
                return ResourceManager.GetString("SettingsWindow_APIPort_Help", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Apply Settings.
        /// </summary>
        internal static string SettingsWindow_ApplyButton {
            get {
                return ResourceManager.GetString("SettingsWindow_ApplyButton", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use Penumbra Icons.
        /// </summary>
        internal static string SettingsWindow_EnablePenumbraIPC {
            get {
                return ResourceManager.GetString("SettingsWindow_EnablePenumbraIPC", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to When enabled, this feature will attempt to display icons from Penumbra on the Stream Deck. Note that Penumbra must be installed for this setting to have any effect.
        ///
        ///If disabled, original game icons will be used instead.
        ///
        ///Default: Off.
        /// </summary>
        internal static string SettingsWindow_EnablePenumbraIPC_Help {
            get {
                return ResourceManager.GetString("SettingsWindow_EnablePenumbraIPC_Help", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use /micon Icons.
        /// </summary>
        internal static string SettingsWindow_Experiment_MIcon {
            get {
                return ResourceManager.GetString("SettingsWindow_Experiment_MIcon", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Experimental Settings.
        /// </summary>
        internal static string SettingsWindow_ExperimentalSettings {
            get {
                return ResourceManager.GetString("SettingsWindow_ExperimentalSettings", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to XIVDeck GitHub.
        /// </summary>
        internal static string SettingsWindow_GitHubLink {
            get {
                return ResourceManager.GetString("SettingsWindow_GitHubLink", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Listen IP: {0}.
        /// </summary>
        internal static string SettingsWindow_ListenIP {
            get {
                return ResourceManager.GetString("SettingsWindow_ListenIP", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DANGER: SAFE MODE DISABLED! You may be able to send illegal commands from your Stream Deck to the game..
        /// </summary>
        internal static string SettingsWindow_SafeModeDisabledWarning {
            get {
                return ResourceManager.GetString("SettingsWindow_SafeModeDisabledWarning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to XIVDeck Game Plugin Settings.
        /// </summary>
        internal static string SettingsWindow_Title {
            get {
                return ResourceManager.GetString("SettingsWindow_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to If the XIVDeck Stream Deck Plugin is already installed, please make sure the port is set correctly in the configuration and that you&apos;ve created at least one button..
        /// </summary>
        internal static string SetupNag_AlreadyInstalledHelp {
            get {
                return ResourceManager.GetString("SetupNag_AlreadyInstalledHelp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Current port: {0}.
        /// </summary>
        internal static string SetupNag_CurrentPort {
            get {
                return ResourceManager.GetString("SetupNag_CurrentPort", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To dismiss this message, resolve the above problem or uninstall the XIVDeck plugin..
        /// </summary>
        internal static string SetupNag_DismissHelp {
            get {
                return ResourceManager.GetString("SetupNag_DismissHelp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A Stream Deck has never connected to the game..
        /// </summary>
        internal static string SetupNag_Headline {
            get {
                return ResourceManager.GetString("SetupNag_Headline", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to If you need to change the port the server is hosted on, you may do so from XIVDeck&apos;s settings..
        /// </summary>
        internal static string SetupNag_PortChangeHelp {
            get {
                return ResourceManager.GetString("SetupNag_PortChangeHelp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to If you haven&apos;t done so already, please make sure you&apos;ve downloaded and installed the companion XIVDeck Stream Deck Plugin from GitHub..
        /// </summary>
        internal static string SetupNag_ResolutionHelp {
            get {
                return ResourceManager.GetString("SetupNag_ResolutionHelp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stream Deck.
        /// </summary>
        internal static string StreamDeck {
            get {
                return ResourceManager.GetString("StreamDeck", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was an issue running a task: {0}: {1}.
        /// </summary>
        internal static string TickScheduler_ExceptionHandler {
            get {
                return ResourceManager.GetString("TickScheduler_ExceptionHandler", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No message exists with opcode {0}.
        /// </summary>
        internal static string WSOpcodeWiring_UnknownOpcodeError {
            get {
                return ResourceManager.GetString("WSOpcodeWiring_UnknownOpcodeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to XIVDeck.
        /// </summary>
        internal static string XIVDeck {
            get {
                return ResourceManager.GetString("XIVDeck", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to XIVDeck Game Plugin.
        /// </summary>
        internal static string XIVDeck_Title {
            get {
                return ResourceManager.GetString("XIVDeck_Title", resourceCulture);
            }
        }
    }
}
