using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using JetBrains.Annotations;

namespace XIVDeck.FFXIVPlugin.RpcServer.Helpers;

[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class RpcServiceAttribute : Attribute {
    public readonly string Name;

    public RpcServiceAttribute(string name) {
        this.Name = name;
    }
}

public static class RpcServiceWiring {
    public static void RegisterAssemblyServices(ContainerBuilder builder) {
        builder.RegisterAssemblyTypes(typeof(RpcServiceAttribute).Assembly)
            .Where(type => type.GetCustomAttribute<RpcServiceAttribute>() != null)
            .InstancePerLifetimeScope();
    }

    public static IReadOnlyList<(string Prefix, object Instance)> ResolveTargets(IComponentContext container) {
        var targets = new List<(string, object)>();

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
            var attribute = type.GetCustomAttribute<RpcServiceAttribute>();
            if (attribute == null) continue;

            targets.Add((attribute.Name, container.Resolve(type)));
        }

        return targets;
    }
}
