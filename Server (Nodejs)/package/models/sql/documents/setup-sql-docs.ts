import { ItemTranslation, Address } from "../../info-models";

export interface SetupValidationSQLDocument {
    remain_time: number
}

export interface SetupSchoolInfoSQLDocument {
    name : ItemTranslation,
    address: Address,
    phone_number: string,
    logo: string
}