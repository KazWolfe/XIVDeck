using PolyType;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;

public enum EmoteLogMode {
    /// <summary>
    /// Use the game's configuration per <c>/elist</c>.
    /// </summary>
    [EnumMemberShape(Name = "default")]
    Default,

    /// <summary>
    /// Always send an emote execution message to chat.
    /// </summary>
    [EnumMemberShape(Name = "always")]
    Always,

    /// <summary>
    /// Always suppress the emote execution message.
    /// </summary>
    [EnumMemberShape(Name = "never")]
    Never
}

/// <summary>
/// Custom execution parameters for emotes.
/// </summary>
/// <param name="LogMode">Whether to send a log message or not.</param>
[GenerateShape]
public partial record EmotePayload(EmoteLogMode LogMode = EmoteLogMode.Default) : ActionPayload;
