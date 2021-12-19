import { ValidationApiOperation } from "../api/operation/instances/validation-api-operation";
import { SessionApiOperation } from "../api/operation/instances/session-api-operation";
import { ApiOperation } from "../api/operation/api-operation";
import { FeatureAuthenticationApiOperation } from "../api/operation/instances/feature-authentication-api-operation";

export class ApiOperationsGroup {

    /*************** Variables ***************/
    public static VALIDATION: ApiOperation[] = [ new ValidationApiOperation() ];
    public static SESSION: ApiOperation[] = [ new SessionApiOperation() ];
    public static VALIDATION_AND_SESSION = [
        ...ApiOperationsGroup.VALIDATION,
        ...ApiOperationsGroup.SESSION
    ];
    public static FEATURE_OPERATIONS = [
        ...ApiOperationsGroup.VALIDATION_AND_SESSION,
        new FeatureAuthenticationApiOperation()
    ];
    
    /*************** Constructor ***************/
    private constructor() {}

}