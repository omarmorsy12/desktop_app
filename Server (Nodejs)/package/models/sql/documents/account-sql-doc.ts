import { Name } from '../../info-models';
import { AccountType } from '../../types';

export interface AccountSQLDocument {
    information: {
        name: Name,
        email: string,
        phone_number: string
    }
    authentication?: {
        username?: string
        password: string
    }
    roles: {
        owned: Array<AccountType>,
        features_ids?: Array<string>
    }
    settings?: {
        profile_image?: string
    }
}