import { AccountType } from "../../types";

export interface PermissionsSQLDocument {
    values: {
        [type in AccountType]: Array<string>
    };
}