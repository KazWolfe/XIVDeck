export class EventEmitter {
    private _eventList: Map<string, PubSub> = new Map<string, PubSub>();

    on(name: string, fn: Function) {
        if (!this._eventList.has(name)) {
            this._eventList.set(name, new PubSub());
        }
        return this._eventList.get(name)?.sub(fn);
    };

    has(name: string) {
        return this._eventList.has(name)
    }
    
    emit(name: string, data: object) {
        this._eventList.has(name) && this._eventList.get(name)?.pub(data);
    }
}

class PubSub {
    private subscribers: Set<Function> = new Set<Function>();
    
    sub(fn: Function): Function {
        this.subscribers.add(fn);
        
        return () => {
            this.subscribers.delete(fn);
        }
    }
    
    pub(data: object) {
        this.subscribers.forEach(fn => fn(data));
    }
}