using System;
using System.Security.Cryptography;

namespace XIVDeck.FFXIVPlugin.Utils; 

public static class CryptoUtils {
    public static string GenerateToken(int length) {
        using var cryptRng = new RNGCryptoServiceProvider();
        var tokenBuffer = new byte[length];
        cryptRng.GetBytes(tokenBuffer);
        return Convert.ToBase64String(tokenBuffer);
    }
}