import { App } from "../../app";
import { ErrorService } from "../../service/instances/error-service";
import { Service } from "../../service/service";
import { isNullOrUndefined } from "../utils/methods";

export class ErrorState {
    /*************** Static Methods ***************/
    private static Build(code: number) {
        return new ErrorState(code);
    }

    /*************** Variables ***************/
    public static DATABASE = {
        CONNECTION: ErrorState.Build(602),
        QUERY_FAILED: ErrorState.Build(610)
    };
    public static API = {
        ROOT_PATH_ALREADY_EXIST: ErrorState.Build(301),
        LOAD_RESOURCE: ErrorState.Build(310)
    };
    public static APP = {
        NOT_INITIALIZED_YET: ErrorState.Build(100),
        UNAUTHORIZED: ErrorState.Build(101),
        USER_SESSION_EXPIRED: ErrorState.Build(103),
        LICENSE_EXPIRED: ErrorState.Build(105)
    };
    public static UNKNOWN = ErrorState.Build(-100);

    /*************** Constructor ***************/
    private constructor(public code: number, public message?: string) {}

    /*************** Methods ***************/
    addMessage(text: string) {
        return new ErrorState(this.code, text);
    }

    equals(error: ErrorState) {
        return this.code === error.code;
    }
}

export class Error {

    /*************** Variables ***************/
    private detected = false;
    private from: { name: string } = App;
    private value: ErrorState = ErrorState.APP.NOT_INITIALIZED_YET;

    get hasInitializeError(): boolean {
        return !isNullOrUndefined(this.value);
    }

    get hasError(): boolean {
        return this.detected;
    }

    /*************** Methods ***************/
    clear(): void {
        this.detected = false;
        this.update(null);
    }

    update(value: ErrorState, from?: { name: string }): void {
        if (!this.detected) {
            this.detected = Boolean(value);
            this.value = value;
            this.from = from;
            
            if (this.detected) {
                this.log();
            }
        }
    }

    log(): void {
        Service.get(ErrorService).log(this.from, this.value);
    }
}