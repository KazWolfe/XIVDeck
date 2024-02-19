using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.IPC; 

public class IPCManager : IDisposable {
    // This entire class may seem unnecessary, and honestly it is.
    // All of the IPC implementations used by this plugin can *absolutely* just be static classes and never have to go
    // through this mess (and in fact, they *are* accessed statically), but it's very frustrating to track disposals
    // and set up listeners on statics. So, we use the IPCManager to actually handle the heavy lifting of instantiation
    // and disposal. Actual IPC classes can be referred to using their static (nullable) Instance, which should be 
    // initialized within here. Note though, that proper nullability checks *must* be applied (though the IDE should 
    // complain if it isn't anyways).
    //
    // This is absolutely an anti-pattern and a massive code smell, but it works. If you can think of a better way to
    // achieve this sort of behavior without having to manually manage X objects, please let me know!
    
    private readonly List<IPluginIpcClient> _registeredIpcs = new();

    public IPCManager() {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
            if (!type.GetInterfaces().Contains(typeof(IPluginIpcClient))) {
                continue;
            }
            
            var attr = type.GetCustomAttribute<PluginIpcAttribute>();
            if (attr == null) continue;
            
            var handler = (IPluginIpcClient) Activator.CreateInstance(type, nonPublic: true)!;

            Injections.PluginLog.Debug($"Registered IPC: {handler.GetType()}");
            this._registeredIpcs.Add(handler);
        }
    }

    public void Dispose() {
        foreach (var ipcObject in this._registeredIpcs) {
            ipcObject.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}

public interface IPluginIpcClient : IDisposable {
    public bool Enabled { get; }
    
    public int Version { get; }
}

[AttributeUsage(AttributeTargets.Class)]
public class PluginIpcAttribute : Attribute { }