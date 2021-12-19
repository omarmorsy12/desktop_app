export interface GuardianSQLDocument {
    fullname: string
    phone_number: string
    identification: {
        id: string,
        type: 'passport_no' | 'national_id'
    }
    email?: string
    profile_image?: string
}