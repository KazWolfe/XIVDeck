using System;
using Dalamud.Game;
using Dalamud.Logging;

namespace FFXIVPlugin.Utils {
    // Graciously borrowed from https://github.com/Eternita-S/NotificationMaster/blob/master/NotificationMaster/TickScheduler.cs
    class TickScheduler : IDisposable {
        long executeAt;
        Action function;
        Framework framework;
        bool disposed = false;

        public TickScheduler(Action function, Framework framework, long delayMS = 0) {
            this.executeAt = Environment.TickCount64 + delayMS;
            this.function = function;
            this.framework = framework;
            framework.Update += Execute;
        }

        public void Dispose() {
            if (!disposed) {
                framework.Update -= Execute;
            }
            disposed = true;
        }

        void Execute(object _) {
            if (Environment.TickCount64 < executeAt) return;
            
            try {
                function();
            } catch (Exception e) {
                PluginLog.Error(e, "Exception running a Framework tick event");
            }
            this.Dispose();
        }
    }
}

