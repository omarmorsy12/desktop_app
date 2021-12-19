import { SQLTable } from "./sql-structure";
import { SessionSQLDocument } from "./documents/session-sql-doc";
import { StudentSQLDocument } from "./documents/student/student-sql-doc";
import { StudentSQLCollections } from "./collections/student-sql-collections";
import { AccountSQLDocument } from "./documents/account-sql-doc";
import { TagSQLCollections } from "./collections/tag-sql-collections";
import { TagGroupSQLDocument } from "./documents/tag/tag-group-sql-doc";
import { PermissionsSQLDocument } from "./documents/permissions-sql-doc";
import { GuardianSQLDocument } from "./documents/guardian-sql-doc";

// Database Tables
export class SessionSQLTable extends SQLTable<SessionSQLDocument> {}
export class StudentsSQLTable extends SQLTable<StudentSQLDocument, StudentSQLCollections> {}
export class AccountsSQLTable extends SQLTable<AccountSQLDocument> {}
export class TagsSQLTable extends SQLTable<TagGroupSQLDocument, TagSQLCollections> {}
export class PermissionsSQLTable extends SQLTable<PermissionsSQLDocument> {}
export class GuardiansSQLTable extends SQLTable<GuardianSQLDocument> {}
