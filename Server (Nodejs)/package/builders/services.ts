import { DatabaseService } from "../../service/instances/database-service";
import { ErrorService } from "../../service/instances/error-service";
import { SessionService } from "../../service/instances/session-service";
import { ValidationService } from "../../service/instances/validation-service";
import { Service } from "../../service/service";

export class Services {

    /*************** Methods ***************/
    public static build(): Array<Service> {
        return [
            new SessionService(),
            new ValidationService()
        ];
    }

    public static buildBase(): Array<Service> {
        return [
            new ErrorService(),
            new DatabaseService()
        ]
    }

    /*************** Constructor ***************/
    private constructor() {}
}