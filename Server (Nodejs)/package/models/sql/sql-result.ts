import { IRecordSet, IResult } from "mssql";
import { ErrorState } from "../error";

export type SQLMaps<RecordSets> = { [key in keyof RecordSets]: (recordSet: IRecordSet<any>) => RecordSets[key] }

export type SQLMap<Record> = (recordSet: IRecordSet<any>) => Record;

export class SQLResultSets<RecordSets = any[]> {

    public static QueryError<Type>(message?:string) {
        return new SQLResultSets<Type>(null, null, ErrorState.DATABASE.QUERY_FAILED.addMessage(message));
    }

    sets: { [key in keyof RecordSets]: RecordSets[key] }
    
    constructor(public response: IResult<any>, mapping?: SQLMaps<RecordSets>, public error?: ErrorState) {
        if (!this.error) {
            this.sets = <any>[];
            if (mapping) {
                const sets: [] = <any>mapping;

                sets.forEach((map: (recordSet: IRecordSet<any>) => any, index) => {
                    (<any>this.sets).push(map ? map(this.response.recordsets[index]) : null);
                });
            }
        }
    }
    
}

export class SQLResultData<Data> {

    public static QueryError<Type>(message?:string) {
        return new SQLResultData<Type>(null, null, ErrorState.DATABASE.QUERY_FAILED.addMessage(message));
    }

    data: Data;
    
    constructor(public response: IResult<any>, mapping?: SQLMap<Data>, public error?: ErrorState) {
        if (!this.error) {
            if (mapping) {
                this.data = mapping(this.response.recordset);
            } else {
                this.data = <any>this.response.recordset;
            }
        }
    }
    
}