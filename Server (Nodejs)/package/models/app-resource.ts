import express from 'express';
import { Express, Request } from 'express-serve-static-core';
import { SessionService } from '../../service/instances/session-service';
import { Service } from '../../service/service';
import { ErrorState } from './error';
import { AppResponse } from './response/app-response';

export class AppResource {
    
    /*************** Variables ***************/
    isSessionRequired = true;
    
    /*************** Constructor ***************/
    constructor(public name: string) {}

    /*************** Methods ***************/
    markAsPublic() {
        this.isSessionRequired = false;
        return this;
    }

    toString() {
        return this.name;
    }

    static use(resource: AppResource, server: Express) {
        server.use('/app/resources/'+resource, async (req: Request, res) => {
            if (resource.isSessionRequired) {
                const token = <string>req.query.token;
                const isValid = await Service.get(SessionService).isSessionValid(token);
                
                if (isValid) {
                    express.static('./resources/' + resource)(req, res);
                    return;
                }

                res.send(AppResponse.EmptyResponse(ErrorState.APP.USER_SESSION_EXPIRED));

            } else {
                express.static('./resources/' + resource)(req, res);
            }
        });
    }
}