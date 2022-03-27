using System.Collections.Generic;
using Dalamud.Interface.Windowing;

namespace XIVDeck.FFXIVPlugin.UI; 

public class PatchedWindowSystem : WindowSystem {
    private readonly List<Window> _deletionQueue = new();
    private readonly List<Window> _additionQueue = new();

    public PatchedWindowSystem(string ns) : base(ns) { }

    public new void AddWindow(Window window) {
        this._additionQueue.Add(window);
    }
    
    public new void RemoveWindow(Window window) {
        this._deletionQueue.Add(window);
    }
    
    public new void Draw() {
        this._deletionQueue.ForEach(base.RemoveWindow);
        this._deletionQueue.Clear();
        
        this._additionQueue.ForEach(base.AddWindow);
        this._additionQueue.Clear();

        base.Draw();
    }
}