using System.Runtime.InteropServices;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace FFXIVPlugin.GameStructs {
    [StructLayout(LayoutKind.Explicit, Size = 0x27278)]
    public unsafe partial struct ExtendedRaptureHotbarModule {
        [FieldOffset(0x48)] public UIModule* UiModule;
        [FieldOffset(0x90)] public HotBars HotBar;

        [FieldOffset(0x11974)] public SavedHotBars SavedClassJob;

        [MemberFunction("E9 ?? ?? ?? ?? 48 8D 91 ?? ?? ?? ?? E9")]
        partial void ExecuteHotbarSlot(RaptureHotbarModule* ptr, HotBarSlot slot);

        public void ExecuteHotbarSlot(HotBarSlot slot) {
            this.ExecuteHotbarSlot(Framework.Instance()->UIModule->GetRaptureHotbarModule(), slot);
        }
    }
}