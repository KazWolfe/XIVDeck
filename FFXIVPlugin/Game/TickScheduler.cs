using System;
using Dalamud.Game;
using Dalamud.Logging;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Game {
    // borrowed from https://github.com/Eternita-S/NotificationMaster/blob/master/NotificationMaster/TickScheduler.cs
    internal class TickScheduler : IDisposable {
        internal static TickScheduler Schedule(Action function, Framework? framework = null, long delay = 0) {
            framework ??= Injections.Framework;

            return new TickScheduler(function, framework, delay);
        }

        private readonly long _executeAt;
        private readonly Action _function;
        private readonly Framework _framework;
        private bool _disposed;

        private TickScheduler(Action function, Framework framework, long delayMillis = 0) {
            this._executeAt = Environment.TickCount64 + delayMillis;
            this._function = function;
            this._framework = framework;
            framework.Update += this.Execute;
        }

        public void Dispose() {
            if (!this._disposed) {
                this._framework.Update -= this.Execute;
            }

            this._disposed = true;
        }

        private void Execute(object _) {
            if (Environment.TickCount64 < this._executeAt) return;
            
            try {
                this._function();
            } catch (Exception e) {
                PluginLog.Error(e, "Exception running a Framework tick event");
                Injections.Chat.PrintError($"[XIVDeck] There was an issue running a task: {e.GetType()}: {e.Message}");
            }
            this.Dispose();
        }
    }
}

