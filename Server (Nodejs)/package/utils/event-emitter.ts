import * as events from "events";

type EventListener<Data> = (data: Data) => void;

export class EventEmitter<Data = void> {

    /*************** Variables ***************/
    private name: string;
    private emitter = new events.EventEmitter();
    
    /*************** Constructor ***************/
    constructor() {
        this.name = 'event';
    }
    
    /*************** Methods ***************/
    subscribe(listener: EventListener<Data>) {
        this.emitter.addListener(this.name, listener);
    }

    unSubscribe(listener: EventListener<Data>) {
        this.emitter.removeListener(this.name, listener);
    }

    unSubscribeAll() {
        this.emitter.removeAllListeners(this.name);
    }

    emit(data: Data) {
        this.emitter.emit(this.name, data);
    }

}
