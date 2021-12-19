import { GuadrianType } from "../../../types";

export interface StudentGuardianSQLDocument {
    studentId: string
    guardianId: string
    type: GuadrianType
}