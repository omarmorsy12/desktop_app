export abstract class Service {

    /*************** Variables ***************/
    private static instances: { [code: string]: Service } = {};

    /*************** Constructor ***************/
    constructor() {
        Service.add(this);
    }

    /*************** Methods ***************/
    private static add(service: Service) {
        this.instances[service.constructor.name] = service;
    }

    public static get<T extends Service>(object: new (...args: any[]) => T): T {
        return <T> this.instances[object.name];
    }

    public static stop() {
        this.instances = {};
    }

}