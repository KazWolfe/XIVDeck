using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.GameStructs;
using Framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace FFXIVPlugin.helpers {
    public class SigHelper {
        private static class Signatures {
            internal const string ExecuteHotbarSlot = "E9 ?? ?? ?? ?? 48 8D 91 ?? ?? ?? ?? E9";
        }
        
        private delegate void ExecHotbarSlotDelegate(IntPtr rHotbarModulePtr, IntPtr hotbarSlot);
        
        private ExecHotbarSlotDelegate HotbarSlotExecutor { get; }

        internal SigHelper(SigScanner scanner) {
            if (scanner.TryScanText(Signatures.ExecuteHotbarSlot, out var ptr)) {
                this.HotbarSlotExecutor = Marshal.GetDelegateForFunctionPointer<ExecHotbarSlotDelegate>(ptr);
            }
        }

        public unsafe void ExecuteHotbarSlot(HotBarSlot* slot) {
            if (this.HotbarSlotExecutor == null) {
                throw new InvalidOperationException("Couldn't find RaptureHotbarModule::ExecuteSlot");
            }

            var hotbarModulePtr = (IntPtr) Framework.Instance()->GetUiModule()->GetRaptureHotbarModule();
            this.HotbarSlotExecutor(hotbarModulePtr, (IntPtr) slot);
        }
        
        public unsafe void ExecuteHotbarAction(HotbarSlotType commandType, uint commandId, bool safemode = true) {
            if (this.HotbarSlotExecutor == null) {
                throw new InvalidOperationException("Couldn't find RaptureHotbarModule::ExecuteSlot");
            }
            var hotbarModulePtr = (IntPtr) Framework.Instance()->GetUiModule()->GetRaptureHotbarModule();

            HotBarSlot slot = new HotBarSlot {
                CommandType = commandType,
                CommandId = commandId
            };
            

            this.HotbarSlotExecutor(hotbarModulePtr, **(IntPtr**) &slot);
        }
    }
}