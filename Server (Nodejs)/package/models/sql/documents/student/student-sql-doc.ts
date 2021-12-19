import { Address, Name } from "../../../info-models";

export interface StudentSQLDocument {
    name: Name
    gender: 'male' | 'female'
    nationality: string
    religion?: 'muslim' | 'christian'
    address: Address
    date_of_birth: number
    registeration_date: number
    graduation_date?: number
    profile_image?: string
    medical_report?: string
}