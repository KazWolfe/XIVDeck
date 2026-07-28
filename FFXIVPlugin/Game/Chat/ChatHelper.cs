using System;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.Game.Chat;

public static unsafe class ChatHelper {
    private const int MaxMessageBytes = 500;

    // Code originally from ascclemens' XivCommon and evolved.

    /// <summary>
    /// Calls the chat message handler akin to sending a message in a chat box. Handles both stripping newlines as well
    /// as calling the native game text sanitization engine, and includes command protections.
    /// </summary>
    /// <param name="text">A normal string to pass to the chat message handler.</param>
    /// <param name="commandOnly">Check that this message is a command (and starts with /).</param>
    public static void SendSanitizedChatMessage(string text, bool commandOnly = true) {
        if (commandOnly && !text.StartsWith('/')) {
            throw new ArgumentException(@"The specified message does not start with a slash while in command-only mode.", nameof(text));
        }

        text = text.ReplaceLineEndings(" ");
        var utfMessage = Utf8String.FromString(text);
        try {
            utfMessage->SanitizeString((AllowedEntities)0x27F);
            SendChatMessage(utfMessage);
        } finally {
            utfMessage->Dtor(true);
        }
    }

    private static void SendChatMessage(Utf8String* utfMessage) {
        switch (utfMessage->Length) {
            case 0:
                throw new ActionInvalidException(UIStrings.ChatHelper_MessageEmptyError);
            case > MaxMessageBytes:
                throw new ActionInvalidException(string.Format(UIStrings.ChatHelper_MessageTooLongError, MaxMessageBytes));
        }

        UIModule.Instance()->ProcessChatBoxEntry(utfMessage, nint.Zero);
    }
}
