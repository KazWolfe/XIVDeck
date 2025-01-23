using System;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace XIVDeck.FFXIVPlugin.Game.Chat;

public unsafe class ChatHelper {
    // Code heavily borrowed from ascclemens' XivCommon
    // https://git.anna.lgbt/ascclemens/XivCommon/src/branch/main/XivCommon/Functions/Chat.cs

    private static ChatHelper? _instance;

    public static ChatHelper GetInstance() {
        return _instance ??= new ChatHelper();
    }

    /// <summary>
    /// Calls the chat message handler akin to sending a message in a chat box. Handles both stripping newlines as well
    /// as calling the native game text sanitization engine, and includes command protections.
    /// </summary>
    /// <param name="text">A normal string to pass to the chat message handler.</param>
    /// <param name="commandOnly">Check that this message is a command (and starts with /).</param>
    public void SendSanitizedChatMessage(string text, bool commandOnly = true) {
        if (commandOnly && !text.StartsWith('/')) {
            throw new ArgumentException(@"The specified message message does not start with a slash while in command-only mode.", nameof(text));
        }

        text = text.ReplaceLineEndings(" ");

        var utfMessage = Utf8String.FromString(text);
        utfMessage->SanitizeString(0x27F, (Utf8String*)nint.Zero);

        this.SendChatMessage(utfMessage);

        utfMessage->Dtor(true);
    }

    private void SendChatMessage(Utf8String* utfMessage) {
        switch (utfMessage->Length) {
            case 0:
                throw new ArgumentException(@"Message cannot be empty", nameof(utfMessage));
            case > 500:
                throw new ArgumentException(@"Message cannot exceed 500 byte limit", nameof(utfMessage));
        }

        UIModule.Instance()->ProcessChatBoxEntry(utfMessage, nint.Zero, false);
    }
}
