import express from 'express';
import { Express, Request } from 'express-serve-static-core';
import { AsyncComponent } from './package/models/aysnc-component';
import { ErrorState, Error } from './package/models/error';
import { Apis } from './package/builders/apis';
import * as http from "http";
import { Service } from './service/service';
import { Api } from './api/api';
import { Services } from './package/builders/services';
import { Setup } from './package/setup';
import { AppResource } from './package/models/app-resource';

export class App {

    /*************** Variables ***************/
    public static setup = new Setup();
    public static instance: Express;
    private static server: http.Server;
    private static views: Array<string> = [];
    private static resources: Array<string> = [];
    private error = new Error();

    /*************** Methods ***************/
    private initServer(): void {
        const bodyParser = require('body-parser');
        const server = express();
        server.use(bodyParser.urlencoded({ extended: false }));
        server.use(bodyParser.json());
        server.set('view engine', 'ejs');
        server.set('view options', { layout: false });
        server.engine('html', require('ejs').renderFile);

        this.loadResources(server);

        App.instance = server;
    }

    private loadResources(server: Express): void {
        const resources = [
            new AppResource('students'),
            new AppResource('school').markAsPublic(),
            new AppResource('parents'),
            new AppResource('accounts')
        ];
        resources.forEach(resource => AppResource.use(resource, server));
    }

    private async initAsyncComponents(components: Array<any>): Promise<void> {
        for (let i = 0; i < components.length; i++) {
            const component = components[i];
            let errorFound = false;
            if (AsyncComponent.implementedBy(component)) {
                await (<AsyncComponent>component).asyncInit().then((err) => {
                    if (err instanceof ErrorState) {
                        errorFound = true;
                        this.error.update(err, component.getClassRef());
                    }
                });
            }

            if (errorFound) {
                return;
            }
        }
    }

    private async initBaseServices(): Promise<void> {
        const services = Services.buildBase();
        await this.initAsyncComponents(services);
    }

    private async initServices(): Promise<void> {
        const services = Services.build();
        await this.initAsyncComponents(services);
    }

    private async initApis(): Promise<void> {
        const apis = Apis.build();
        await this.initAsyncComponents(apis);
    }

    public static loadView(path: string): void {
        if (!this.views.includes(path)) {
            this.views.push(path);
            this.instance.set('views', this.views);
        }
    }

    public static loadCssResource(name: string, path: string) {
        if (!this.resources.includes(path + '-css')) {
            this.instance.use('/app/css/'+ name, express.static(path));
            this.resources.push(path + '-css');
        }
    }

    public static loadImageResource(name: string, path: string) {
        if (!this.resources.includes(path + '-image')) {
            this.instance.use('/app/image/'+ name, express.static(path));
            this.resources.push(path + '-image');
        }
    }

    public async initialize(): Promise<App> {
        this.initServer();
        await this.initBaseServices();

        return App.setup.isOkay().then((err) => {
            if (!err) {
                return this.initServices().then(() => {
                    if (this.error.hasError) {
                        return Promise.resolve(this);
                    } else {
                        return this.initApis().then(() => {
                            if (!this.error.hasError) {
                                this.error.clear();
                            }
                            return Promise.resolve(this);
                        });
                    }
                });
            }
            this.error.update(err, App);
            return Promise.resolve(this);
        });
    }

    public start(port: number, message?: string): App {
        if (this.error.hasInitializeError && !this.error.hasError) {
            this.error.log();
        } else if (!this.error.hasError) {
            App.server = App.instance.listen(port, () => {
                if (message) {
                    console.log(message);
                }
                console.log('Port:', port);
            });
        }
        return this;
    }

    public stop() {
        if (App.server) {
            Service.stop();
            Api.stop();
            App.server.close();
            App.server = null;
            App.instance = null;
        }
    }

}