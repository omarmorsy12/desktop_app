import { ItemTranslation } from "../../../info-models";
import { AccountType } from "../../../types";

export interface TagGroupSQLDocument {
    name: ItemTranslation
    for: Array<AccountType>
    isPrimary?: boolean
}