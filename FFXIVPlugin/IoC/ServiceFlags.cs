using System;

namespace XIVDeck.FFXIVPlugin.IoC;

[Flags]
public enum ServiceFlags {
    Singleton = 1 << 0,
    Transient = 1 << 1,

    // Automatically construct this service after everything's been registered.
    AutoLoad = 1 << 2,
}
