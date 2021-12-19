import { StudentSQLCollections } from "../models/sql/collections/student-sql-collections";
import { TagSQLCollections } from "../models/sql/collections/tag-sql-collections";
import { AccountsSQLTable, SessionSQLTable, TagsSQLTable, PermissionsSQLTable, StudentsSQLTable, GuardiansSQLTable } from "../models/sql/sql-tables";

export class SQLTables {

    /*************** Methods ***************/
    public static build() {
        return {
            accounts: new AccountsSQLTable('accounts'),
            sessions: new SessionSQLTable('sessions'),
            tags: new TagsSQLTable('tags', { collections: new TagSQLCollections() }),
            permissions: new PermissionsSQLTable('permissions'),
            students: new StudentsSQLTable('students', { collections: new StudentSQLCollections() }),
            guardians: new GuardiansSQLTable('guardians')
        }
    }

    /*************** Constructor ***************/
    private constructor() {}
}