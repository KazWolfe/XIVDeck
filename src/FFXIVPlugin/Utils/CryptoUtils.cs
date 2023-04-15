using System;
using System.Security.Cryptography;

namespace XIVDeck.FFXIVPlugin.Utils; 

public static class CryptoUtils {
    public static string GenerateToken(int length) {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(length));
    }
}