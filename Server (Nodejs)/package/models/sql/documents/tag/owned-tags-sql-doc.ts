import { AccountType } from "../../../types";

export interface OwnedTagsSQLDocument {
    ownerId: string
    role: AccountType,
    tagId: string
}