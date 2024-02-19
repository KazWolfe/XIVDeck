using System;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Game.Chat; 

public unsafe class ChatHelper {
    // Code heavily borrowed from ascclemens' XivCommon
    // https://git.anna.lgbt/ascclemens/XivCommon/src/branch/main/XivCommon/Functions/Chat.cs

    private static ChatHelper? _instance;

    public static ChatHelper GetInstance() {
        return _instance ??= new ChatHelper();
    }
    
    private static class Signatures {
        internal const string SendChatMessage = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9";
        internal const string SanitizeChatString = "E8 ?? ?? ?? ?? EB 0A 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8D 8D";
    }

    private ChatHelper() {
        Injections.GameInteropProvider.InitializeFromAttributes(this);
    }
    
    [Signature(Signatures.SanitizeChatString, Fallibility = Fallibility.Fallible)]
    private readonly delegate* unmanaged<Utf8String*, int, nint, void> _sanitizeChatString = null!;

    // UIModule, message, unused, byte
    [Signature(Signatures.SendChatMessage, Fallibility = Fallibility.Fallible)]
    private readonly delegate* unmanaged<UIModule*, Utf8String*, nint, byte, void> _processChatBoxEntry = null!;
    
    /// <summary>
    /// Calls the chat message handler akin to sending a message in a chat box. Handles both stripping newlines as well
    /// as calling the native game text sanitization engine, and includes command protections.
    /// </summary>
    /// <param name="text">A normal string to pass to the chat message handler.</param>
    /// <param name="commandOnly">Check that this message is a command (and starts with /).</param>
    public void SendSanitizedChatMessage(string text, bool commandOnly = true) {
        if (commandOnly && !text.StartsWith("/")) {
            throw new ArgumentException(@"The specified message message does not start with a slash while in command-only mode.", nameof(text));
        }

        text = text.ReplaceLineEndings(" ");

        var utfMessage = Utf8String.FromString(text);
        this.SanitizeString(utfMessage);
        
        this.SendChatMessage(utfMessage);
        
        utfMessage->Dtor(true);
    }

    /// <summary>
    /// Sanitize a Utf8String* in-place.
    /// </summary>
    /// <param name="utfString">A pointer to the string to sanitize.</param>
    private void SanitizeString(Utf8String* utfString) {
        if (this._sanitizeChatString == null) {
            throw new InvalidOperationException("Could not find the signature for SanitizeString!");
        }

        this._sanitizeChatString(utfString, 0x27F, nint.Zero);
    }

    private void SendChatMessage(Utf8String* utfMessage) {
        if (this._processChatBoxEntry == null) {
            throw new InvalidOperationException("Could not find the signature for SendChatMessage!");
        }

        switch (utfMessage->Length) {
            case 0:
                throw new ArgumentException(@"Message cannot be empty", nameof(utfMessage));
            case > 500:
                throw new ArgumentException(@"Message cannot exceed 500 byte limit", nameof(utfMessage));
        }
        
        this._processChatBoxEntry(Framework.Instance()->GetUiModule(), utfMessage, nint.Zero, 0);
    }
}