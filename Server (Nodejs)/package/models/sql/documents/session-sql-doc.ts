import { AccountType } from '../../types';

export interface SessionSQLDocument {
    account_id: string;
    started_at: number;
    last_active: number;
    ownedFeatures: Array<string>;
    ownedTags: string[]
    permissions: string[]
    role: AccountType;
}