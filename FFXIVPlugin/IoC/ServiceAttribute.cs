using System;
using JetBrains.Annotations;

namespace XIVDeck.FFXIVPlugin.IoC;

[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Class)]
public class ServiceAttribute : Attribute {
    public readonly ServiceFlags Flags;

    public ServiceAttribute(ServiceFlags flags) {
        if (flags.HasFlag(ServiceFlags.Singleton) == flags.HasFlag(ServiceFlags.Transient)) {
            throw new ArgumentException(
                @$"ServiceAttribute must specify exactly one of {nameof(ServiceFlags.Singleton)} or {nameof(ServiceFlags.Transient)}.",
                nameof(flags));
        }

        this.Flags = flags;
    }
}
