import { Api } from "../../api/api";
import { AppApi } from "../../api/app-api";
import { TestFeatureApi } from "../../api/instances/feature/teacher/test-feature-api";
import { LoginApi } from "../../api/instances/login-api";
import { LogoutApi } from "../../api/instances/logout-api";
import { StudentsApi } from "../../api/instances/students-api";
import { Test2Api } from "../../api/instances/test2/test2-api";
import { ApiOperationsGroup } from "../api-operations-group";

export class Apis {

    /*************** Methods ***************/
    public static build(): Array<Api> {
        return [
            new LoginApi('login/', ApiOperationsGroup.VALIDATION),
            new LogoutApi('logout/'),
            new Test2Api('test-2/', ApiOperationsGroup.VALIDATION),
            new AppApi('app/'),
            new StudentsApi('students/', ApiOperationsGroup.VALIDATION_AND_SESSION),
            new TestFeatureApi()
        ];
    }

    /*************** Constructor ***************/
    private constructor() {}
}