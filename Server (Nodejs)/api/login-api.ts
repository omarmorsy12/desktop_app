import { Api } from '../api';
import { Service } from '../../service/service';
import { DatabaseService } from '../../service/instances/database-service';
import { AccountSQLDocument } from '../../package/models/sql/documents/account-sql-doc';
import { SessionService } from '../../service/instances/session-service';
import { AppResponse } from '../../package/models/response/app-response';
import { Session } from '../../package/models/session';
import { PermissionsSQLDocument } from '../../package/models/sql/documents/permissions-sql-doc';
import { ErrorState } from '../../package/models/error';
import { ApiParams } from '../../package/models/api-models';
import { SQLJson, SQLOperation, SQLRecord } from '../../package/models/sql/sql-structure';
import { OwnedTagsSQLDocument } from '../../package/models/sql/documents/tag/owned-tags-sql-doc';
import { SQLResultData } from '../../package/models/sql/sql-result';

interface Storage {
    id: string,
    account: AccountSQLDocument,
    ownedTags: SQLRecord<OwnedTagsSQLDocument>[]
    permissions: PermissionsSQLDocument
}

interface LoginRequestBody { 
    id: string; 
    password: string;
    session_only?: boolean;
    role_index?: number;
}

class LoginResponse {

    /*************** Variables ***************/
    public account?: {
        id: string,
        username?: string,
        information: {
            name: { first: string, last: string },
            email: string,
            phone_number: string
        },
        settings: {
            profile_image?: string
        }
    };

    public multipleRoles?: Array<string>;

    /*************** Constructor ***************/
    constructor(id: string, public session: Session, account: AccountSQLDocument) {
        if (account) {
            this.account = { 
                id,
                username: account.authentication ? account.authentication.username : undefined,
                information: account.information,
                settings: account.settings
            };
            if (session && session.require_role_index) {
                this.multipleRoles = account.roles.owned;
            }
        }
    }
}

export class LoginApi extends Api {

    /*************** Variables ***************/
    private dbService: DatabaseService;
    private sessionService: SessionService;

    /*************** Methods ***************/
    initialize(): void {
        this.dbService = Service.get(DatabaseService);
        this.sessionService = Service.get(SessionService);
        this.post('', this.login, [
            this.authenticate,
            this.getAccountData,
            this.createSession
        ]);
    }

    async authenticate(params: ApiParams<LoginRequestBody, void, Storage>) {
        const body = params.req.body;
        const account_table = this.dbService.tables.accounts;
        const isValid = !!body.id && !!body.password;

        const authorized = isValid && await account_table.query().find([
            SQLOperation.GROUP([
                SQLOperation.EQUALS(SQLJson.VALUE('information.email'), body.id),
                SQLOperation.OR,
                SQLOperation.EQUALS(SQLJson.VALUE('authentication.username'), body.id)
            ]),
            SQLOperation.AND,
            SQLOperation.EQUALS(SQLJson.VALUE('authentication.password'), body.password)
        ]).then((value) => {
            const isFound = !!value.data && !!value.data.length;

            if (isFound) {
                params.storage.account = value.data[0].document;
                params.storage.id = value.data[0]._id;
            }

            return isFound;
        });

        if (!authorized) {
            return ErrorState.APP.UNAUTHORIZED;
        }
    }

    async getAccountData(params: ApiParams<LoginRequestBody, void, Storage>) {
        const response = await this.dbService.tables.tags.collections.owned
            .query()
            .transaction
            .find([ SQLOperation.EQUALS(SQLJson.VALUE('ownerId'), params.storage.id) ])
            .concat(this.dbService.tables.permissions.query().transaction.get(params.storage.id))
            .execute<[ SQLRecord<OwnedTagsSQLDocument>[], SQLResultData<SQLRecord<PermissionsSQLDocument>> ]>();
        
        if (response.error) {
            return response.error;
        }
        
        params.storage.ownedTags = response.sets[0];
        params.storage.permissions = response.sets[1]?.data?.document;
    }

    async createSession(params: ApiParams<LoginRequestBody, void, Storage>) {
        const body = params.req.body;
        const { account, permissions, ownedTags, id } = params.storage;
        const session = await this.sessionService.createSession(id, account, ownedTags, permissions, body.role_index);
        
        return session.err || session;
    }

    login(params: ApiParams<LoginRequestBody, Session, Storage>): AppResponse<LoginResponse> {
        const session = params.previousData;
        const response = new LoginResponse(params.storage.id, session, params.req.body.session_only ? null : params.storage.account);
        return AppResponse.DataResponse(response);
    }
}

