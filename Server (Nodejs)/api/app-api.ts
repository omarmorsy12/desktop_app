import { Api } from "../api";
import { Service } from "../service/service";
import { ValidationService } from "../../service/instances/validation-service";
import { AppResponse } from "../../package/models/response/app-response";
import { App } from "../app";
import { ErrorState } from "../package/models/error";
import { ApiParams } from "../../package/models/api-models";
import { SetupSchoolInfoSQLDocument } from "../package/models/sql/documents/setup-sql-docs";
import { SQLRecord } from "../package/models/sql/sql-structure";
import { SQLResultData } from "../package/models/sql/sql-result";

export class AppApi extends Api {
    
    /*************** Methods ***************/
    initialize(): void {
        this.get('license/status', this.isLicenseValid);
        this.get('startup', this.getStartupInformation, [ this.getSchoolInformation ]);
    }

    isLicenseValid(_apiParams: ApiParams<any, boolean, any>) {
        const isValid = Service.get(ValidationService).isLicenseValid();
        return AppResponse.EmptyResponse(isValid ? null : ErrorState.APP.LICENSE_EXPIRED);
    }

    getSchoolInformation(_apiParams: ApiParams): Promise<any> {
        return App.setup.schoolInformation();
    }
    
    getStartupInformation(apiParams: ApiParams<any, SQLResultData<SQLRecord<SetupSchoolInfoSQLDocument>>>) {
        const isValid = Service.get(ValidationService).isLicenseValid();
        const err = isValid ? null : ErrorState.APP.LICENSE_EXPIRED;
        const data = apiParams.previousData.data?.document;
        
        return AppResponse.DataResponse(data, apiParams.previousData.error || err);
    }
}
