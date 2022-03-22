using System;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;
// ReSharper disable InconsistentNaming - matching expected documentation things
// ReSharper disable UnusedAutoPropertyAccessor.Local - handled by siggingway and reflection

namespace XIVDeck.FFXIVPlugin.Game {
    public unsafe class SigHelper : IDisposable {
        private static class Signatures {
            internal const string ExecuteHotbarSlot = "E9 ?? ?? ?? ?? 48 8D 91 ?? ?? ?? ?? E9";
            
            // todo: this is *way* too broad. this game is very write-happy when it comes to gearset updates.
            internal const string SaveGearset = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 48 8B F2 48 8B F9 33 D2";
            
            // todo: this doesn't handle icon updates until after the macro dialog is closed, so there's some lag.
            internal const string SaveMacro = "45 33 C9 E9 ?? ?? ?? ?? CC CC CC CC CC CC CC CC 48 85 D2";

            internal const string LoadHotbarSlotIcon =
                "40 53 48 83 EC 20 44 8B 81 ?? ?? ?? ?? 48 8B D9 0F B6 91 ?? ?? ?? ?? E8 ?? ?? ?? ?? 85 C0";

            internal const string SanitizeChatString = "E8 ?? ?? ?? ?? EB 0A 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8D 8D";
        }
        
        /***** functions *****/
        [Signature(Signatures.ExecuteHotbarSlot, Fallibility = Fallibility.Fallible)]
        private readonly delegate* unmanaged<RaptureHotbarModule*, HotBarSlot*, void> _execHotbarSlot = null!;

        [Signature(Signatures.LoadHotbarSlotIcon, Fallibility = Fallibility.Fallible)]
        private readonly delegate* unmanaged<HotBarSlot*, bool> _refreshHotbarIcon = null!;
        
        [Signature(Signatures.SanitizeChatString, Fallibility = Fallibility.Fallible)]
        private readonly delegate* unmanaged<Utf8String*, int, IntPtr, void> _sanitizeChatString = null!;
        
        /***** hooks *****/
        private delegate IntPtr RaptureGearsetModule_WriteFile(IntPtr a1, IntPtr a2);
        private delegate IntPtr RaptureMacroModule_WriteFile(IntPtr a1, IntPtr a2, IntPtr a3);
        
        [Signature(Signatures.SaveGearset, DetourName = nameof(DetourGearsetSave))]
        private Hook<RaptureGearsetModule_WriteFile>? RGM_WriteFileHook { get; init; }
        
        [Signature(Signatures.SaveMacro, DetourName = nameof(DetourMacroSave))]
        private Hook<RaptureMacroModule_WriteFile>? RMM_WriteFileHook { get; init; }

        /***** the actual class *****/
        private readonly XIVDeckPlugin _plugin = XIVDeckPlugin.Instance;
        
        internal SigHelper() {
            SignatureHelper.Initialise(this);

            this.RGM_WriteFileHook?.Enable();
            this.RMM_WriteFileHook?.Enable();
        }

        public void Dispose() {
            this.RGM_WriteFileHook?.Dispose();
            this.RMM_WriteFileHook?.Dispose();
            
            GC.SuppressFinalize(this);
        }

        public void ExecuteHotbarSlot(HotBarSlot* slot) {
            if (this._execHotbarSlot == null) {
                 throw new InvalidOperationException("Couldn't find RaptureHotbarModule::ExecuteSlot");
            }

            this._execHotbarSlot( Framework.Instance()->GetUiModule()->GetRaptureHotbarModule(), slot);
        }
        
        public void ExecuteHotbarAction(HotbarSlotType commandType, uint commandId, bool safemode = true) {
            if (this._execHotbarSlot == null) {
                throw new InvalidOperationException("Couldn't find RaptureHotbarModule::ExecuteSlot");
            }
            var hotbarModulePtr = Framework.Instance()->GetUiModule()->GetRaptureHotbarModule();

            var slot = new HotBarSlot {
                CommandType = commandType,
                CommandId = commandId
            };

            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(slot));
            Marshal.StructureToPtr(slot, ptr, false);
            
            this._execHotbarSlot(hotbarModulePtr, (HotBarSlot*) ptr);
            
            Marshal.FreeHGlobal(ptr);
        }

        public bool RefreshHotbarSlotIcon(HotBarSlot* slot) {
            if (this._refreshHotbarIcon == null) {
                throw new InvalidOperationException("Couldn't find RaptureHotbarModule::ExecuteSlot");
            }

            return this._refreshHotbarIcon( slot );
        }

        public string GetSanitizedString(string input) {
            var uString = Utf8String.FromString(input);
            
            this._sanitizeChatString(uString, 0x27F, IntPtr.Zero);
            var output = uString->ToString();
            
            uString->Dtor();
            IMemorySpace.Free(uString);
            
            return output;
        }

        private IntPtr DetourGearsetSave(IntPtr a1, IntPtr a2) {
            PluginLog.Debug("Gearset update!");
            var tmp =  this.RGM_WriteFileHook!.Original(a1, a2);

            var message = new WSStateUpdateMessage("GearSet");
            this._plugin.XivDeckWsServer.MulticastText(JsonConvert.SerializeObject(message));
            WSEventNotifier.Instance?.BroadcastMessage(message);

            return tmp;
        }

        private IntPtr DetourMacroSave(IntPtr a1, IntPtr a2, IntPtr a3) {
            PluginLog.Debug("Macro update!");
            var tmp = this.RMM_WriteFileHook!.Original(a1, a2, a3);

            var message = new WSStateUpdateMessage("Macro");
            this._plugin.XivDeckWsServer.MulticastText(JsonConvert.SerializeObject(message));
            WSEventNotifier.Instance?.BroadcastMessage(message);

            return tmp;
        }
    }
}