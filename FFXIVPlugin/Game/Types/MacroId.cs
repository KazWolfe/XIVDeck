namespace XIVDeck.FFXIVPlugin.Game.Types;

/// <summary>
/// A macro slot. XIVDeck's action IDs number individual macros 0-99 and shared macros 100-199, while the game's
/// hotbar command IDs put the page in the high byte instead.
/// </summary>
/// <remarks>
/// 0-199 numbering is hardcoded for now as the game itself uses this information. Any changes to how the game stores
/// macros will require a lot more work, and is somewhat unpredictable right now.
/// </remarks>
public readonly record struct MacroId(bool Shared, uint Index) {
    /// <summary>The game's macro page: 0 for individual macros, 1 for shared ones.</summary>
    public uint Page => this.Shared ? 1u : 0u;

    public static bool TryFromActionId(uint actionId, out MacroId macroId) {
        if (actionId >= 200) {
            macroId = default;
            return false;
        }

        macroId = new MacroId(actionId >= 100, actionId % 100);
        return true;
    }

    public uint ToActionId() => this.Page * 100 + this.Index;

    public uint ToHotbarCommandId() => (this.Page << 8) + this.Index;
}
