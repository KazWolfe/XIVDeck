using PolyType;

namespace XIVDeck.FFXIVPlugin.Contract;

[GenerateShape]
public partial record ClientHello {
    /// <summary>
    /// the version of the StreamDeck client that is connecting. Used for API comparison.
    /// </summary>
    public string? ClientVersion;

    /// <summary>
    /// The "user agent" indicating the type of client connecting.
    /// Defined in spec now in case it's needed in the future.
    /// </summary>
    public string? ClientType;
}

[GenerateShape]
public partial record ServerHello {
    public required bool Accepted;
    public string? FfxivPluginVersion;
    public string? ApiVersion;
}
