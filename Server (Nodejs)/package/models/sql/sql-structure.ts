import { isNullOrUndefined } from '../../utils/methods';
import { SQLCollectionQuery, SQLQuery } from './sql-query';

interface SQLTableConfiguration<CollectionsScheme, RowExtend> {
    extraColumns?: { [key in keyof RowExtend]: SQLTableColumn }
    collections?: CollectionsScheme
}

export type SQLTableRefName = 'a' | 'b' | 'c';

export interface SQLBuilder {
    SQLText: string
}

export interface SQLRow<Doc = any> {
    _id?: string
    document: Doc
}

export type SQLGroup<RowExtend = any> = SQLRecord<{ value: string | number, count: number }, RowExtend>[];
export type SQLCount = { count: number };
export type SQLDoc<T> = T & { _id: string };
export type SQLRecord<Doc, RowExtend = {}> = SQLRow<Doc> & RowExtend & { collection?: string }
type TableCollections<Scheme, RowExtend extends { [key: string]: string | number } = {}> = { [key in keyof Scheme]: SQLTableCollection<Scheme[key], RowExtend> };
export type SQLGroupValue = string | SQLJson.Value;
export type SQLGroupedValues = [ SQLGroupValue | SQLJson.QueryValue, (SQLGroupValue | SQLJson.QueryValue)? ];
export interface SQLGroupSelection {
    by?: { 1: string, 2?: string },
    grouped?: { 1: string, 2?: string }
    count?: string | 1
}
export type SQLGroupOptions = SQLQueryOptions & { 
    groupedValues?: [SQLGroupValue, SQLGroupValue?],
    selection: SQLGroupSelection & { by: { 1: string, 2?: string } },
    parseOutside?: string
};

export interface SQLQueryOptions {
    offset?: SQLOffset
    andDelete?: boolean
}

export interface SQLJoinSource { 
    on: SQLOperation[],
    source?: SQLTable | SQLTableCollection
}

export class SQLSelectAs {
    static build(...map: [string | SQLJson.Value | SQLJson.QueryValue | SQLJson.Modify, string][]) {
        return new SQLSelectAs(new Map(<any>map));
    }

    private constructor(public map: Map<string | SQLJson.Value | SQLJson.QueryValue | SQLJson.Modify, string>) {}
}

export interface SQLJoinOptions {
    selection?: SQLSelectAs
    sources: [SQLJoinSource, SQLJoinSource?, SQLJoinSource?]
    fromSQL?: string
    offset?: SQLOffset
}

export interface SQLOffset {
    limit: number
    skip: number
    orderBy: string | SQLJson.Value
    orderDirection?: 'DESC' | 'ASC'
}

export class SQLDataType implements SQLBuilder {
    
    /*************** Variables ***************/
    public SQLText: string;
    public isRequired: boolean = true;

    /*************** Constructor ***************/
    private constructor(name: string, _value?: string) {
        const template = '[$name]($value)';
        this.SQLText = template
            .replace('$name', name)
            .replace(_value ? '$value' : '($value)', _value || '') + ' NOT NULL'; 
    }

    /*************** Methods ***************/
    public static get INT() {
        return new SQLDataType('int');
    }

    public static get BIG_INT() {
        return new SQLDataType('bigint');
    }

    public static NVARCHAR(length: number | 'MAX') {
        return new SQLDataType('nvarchar', length + '');
    }

    public static get SMALL_NVARCHAR() {
        return SQLDataType.NVARCHAR(20);
    }

    public static get MEDIUM_NVARCHAR() {
        return SQLDataType.NVARCHAR(60);
    }
    
    public static get LARGE_NVARCHAR() {
        return SQLDataType.NVARCHAR(200);
    }

    public static get MAX_NVARCHAR() {
        return SQLDataType.NVARCHAR('MAX');
    }

    public primaryKey() {
        const value = ' PRIMARY KEY';
        if(!this.SQLText.includes(value)) {
            this.SQLText += value;
        }
        this.isRequired = true;
        return this;
    }

    public Nullable() {
        this.SQLText = this.SQLText.replace(' NOT NULL', '');
        
        if(!this.SQLText.includes(' PRIMARY KEY')) {
            this.isRequired = false;
        }

        return this;
    }

}

export class SQLTableColumn implements SQLBuilder {
    
    /*************** Variables ***************/
    public SQLText: string;

    /*************** Constructor ***************/
    constructor(public name: string, private type: SQLDataType) {
        this.SQLText = '[' + this.name + '] ' + this.type.SQLText;
    }
}

export class SQLCollectionScheme<Doc = any> {
    public value: Doc;
}

export class SQLTableCollection<Doc = any, RowExtend extends { [key: string]: string | number } = {}> {
    /*************** Constructor ***************/
    constructor(public name: string, public sqlTable: SQLTable<Doc, any, RowExtend>) {}

    /*************** Methods ***************/
    query(): SQLCollectionQuery<Doc, RowExtend> {
       return new SQLCollectionQuery<Doc, RowExtend>(this.sqlTable, this.name);
    }
}

export class SQLTable<Doc = any, CollectionsScheme = any, RowExtend extends { [key: string]: string | number } = {}> {

    /*************** Variables ***************/
    public columns: SQLTableColumn[];
    public collections?: TableCollections<CollectionsScheme, RowExtend>;

    /*************** Constructor ***************/
    constructor(public name: string, config?: SQLTableConfiguration<CollectionsScheme, RowExtend>) {
        this.columns = [
            new SQLTableColumn('_id', SQLDataType.NVARCHAR(400).primaryKey()),
            new SQLTableColumn('document', SQLDataType.MAX_NVARCHAR)
        ];
        if (config?.extraColumns) {
            Object.keys(config.extraColumns).forEach((key) => this.columns.push(config.extraColumns[key]));
        }
        this.columns.push(new SQLTableColumn('collection', SQLDataType.MEDIUM_NVARCHAR.Nullable()));
    
        if (config?.collections) {
            this.collections =<any>{};
            Object.keys(config.collections).forEach(key => {
                this.collections[key] = new SQLTableCollection(key, this);
            });
        }
    }

    /*************** Methods ***************/
    query(): SQLQuery<Doc & RowExtend> {
        return new SQLQuery<Doc & RowExtend>(this);
    }

}

export namespace SQLJson {
    
    export class Value implements SQLBuilder {
        /*************** Variables ***************/
        private compilerVersion = '1.0';
        public SQLText: string;
    
        /*************** Constructor ***************/
        constructor(name: string, tableRefName?: SQLTableRefName, from: string = 'document') {
            this.SQLText = "JSON_VALUE(" + (tableRefName ? tableRefName + '.' : '') + from + ", '$." + name + "')";
            this.clone = (t) => new Value(name, t);
        }
        
        /*************** Methods ***************/
        clone: (tableRefName?: SQLTableRefName) => Value;

        toString() {
            return this.SQLText;
        }
    }
    
    export class QueryValue implements SQLBuilder {
        /*************** Variables ***************/
        private compilerVersion = '1.0';
        public SQLText: string;
    
        /*************** Constructor ***************/
        constructor(name: string, tableRefName?: SQLTableRefName, from: string = 'document') {
            this.SQLText = `JSON_QUERY(${ (tableRefName ? tableRefName + '.' : '') + from }${ name ? ", '$." + name + "'" : '' })`;
            this.clone = (t) => new QueryValue(name, t); 
        }
    
        /*************** Methods ***************/
        clone: (tableRefName?: SQLTableRefName) => QueryValue;

        toString() {
            return this.SQLText;
        }
    }
    
    export class Modify<Type = any> implements SQLBuilder {
        
        /*************** Variables ***************/
        public SQLText: string;
    
        /*************** Constructor ***************/
        constructor(private value: Type, private ignoreQuote?: boolean) {}
        
        /*************** Methods ***************/
        modify(path: string, on: string = 'document') {
            const value = this.value;
            const type = typeof(value);
            const isString = type === 'string';
            const isObject = type === 'object' && !(value instanceof SQLJson.Value);
            
            const quote = (isString || isObject) && !this.ignoreQuote ? "'" : '';
            const sqlValue = (quote ? 'N'+ quote : '')  + (isObject ? JSON.stringify(value) : value) + quote;
            this.SQLText = `JSON_MODIFY(${on}, '$.${path}', ${ isObject ? 'JSON_QUERY(' + sqlValue + ')' : sqlValue})`
            
            return this;
        }

        toString() {
            return this.SQLText;
        }
    }   
    
    export function VALUE(key: string, tableRefName?: SQLTableRefName) {
        return new Value(key, tableRefName);
    }

    export function COMBINED_VALUES(...values: (string | SQLJson.Value)[]) {
        const combined = new Value('');
        combined.SQLText = values.join(" + ' ' + ");
        return combined;
    }

    export function MODIFY<Type = any>(value: Type) {
        return new Modify<Type>(value);
    }

    export function SELECT_MODIFY(name: string, key: string, value: string | SQLJson.QueryValue | SQLJson.Value) {
        return new Modify(value, true).modify(key, name);
    }

    export function QUERY_VALUE(name: string, tableRefName?: SQLTableRefName) {
        return new QueryValue(name, tableRefName);
    }
    
    export function SELECT_QUERY_VALUE(name: string) {
        return new QueryValue(null, null, name);
    }

    export function SELECT_VALUE(name: string, key: string) {
        return new Value(key, null, name);
    }
}

export class SQLOperation implements SQLBuilder {

    /*************** Methods ***************/
    public static LESS_THAN(a: SQLJson.Value | string, b: number, orEqual?: boolean): SQLOperation {
        return this.Middle('<' + (orEqual ? '=' : ''), a, b);
    };
    
    public static GREATER_THAN(a: SQLJson.Value | string, b: number, orEqual?: boolean): SQLOperation {
        return this.Middle('>' + (orEqual ? '=' : ''), a, b);
    };

    public static EQUALS(a: SQLJson.Value | string, b: SQLJson.Value | string | number, isColumnValue?: boolean) {
        return this.Middle('=', a, b, !isColumnValue);
    }
    
    public static NOT_EQUALS(a: SQLJson.Value | string, b: string | number, isColumnValue?: boolean) {
        return this.Middle('!=', a, b, !isColumnValue);
    }

    public static BETWEEN_RANGE(a: SQLJson.Value | string, b: number, c: number) {
        return this.Middle('BETWEEN', a, b + ' AND ' + c);
    }

    public static BETWEEN_STRINGS(a: SQLJson.Value | string, b: string, c: string) {
        return this.Middle('BETWEEN', a, "'"+ b + "' AND '" + c + "'");
    }

    public static START_WITH(a: SQLJson.Value | string, b: string) {
        return this.Middle('LIKE', a, "'"+ b + "%'");
    }
    
    public static END_WITH(a: SQLJson.Value | string, b: string) { 
        return this.Middle('LIKE', a, "'%" + b + "'");
    }

    public static CONTAINS(a: SQLJson.Value | string, b: string) {
        return this.Middle('LIKE', a, "'%" + b + "%'");
    }

    public static CONTAINED(a: SQLJson.Value | string, b: string, isColumnValue?: boolean) {
        const value = a instanceof SQLJson.Value ? "'%' + " + a + " + '%'" : "'%" + a + "%'";
        return this.Middle('LIKE', (isColumnValue ? '' : "'") + b + (isColumnValue ? '' : "'"), value);
    }

    public static EXISTS(a: SQLJson.Value | string | SQLJson.QueryValue) {
        return this.Right('is not null', a);
    }

    public static NOT_EXISTS(a: SQLJson.Value | string | SQLJson.QueryValue) {
        return this.Right('is null', a);
    }

    public static ARRAY_CONTAINS(a: SQLJson.QueryValue, b: string | number, isColumnValue?: boolean) {
        const cases = ['\\[n\\]' , ',n,' , '\\[n,' , ',n\\]'];
        const operations = cases.map(c => {
            const value = c.replace('n', typeof(b) === 'string' ? '"' + b + '"' : b.toString());
            return a.SQLText + ` LIKE '%${ (isColumnValue ? "' + " : '') + value + (isColumnValue ? " + '" : '')}%' ESCAPE '\\'`;
        }).join(' ' + SQLOperation.OR.SQLText + ' ');

        return new SQLOperation(operations);
    }

    public static IN(a: SQLJson.Value | string, values: Array<string> | Array<number>) {
        const isString = values.length && typeof (values[0]) == 'string';
        const array = (isString ? values.map(v => "'" + v + "'") : values).join(', ');
        return this.Middle('IN', a, array);
    }

    public static GROUP(operations: SQLOperation[]) {
        const value = '(' + operations.map((op) => op.SQLText).join(' ') + ')';
        return new SQLOperation(value);
    }

    public static CLONE(operations: SQLOperation[], tableRefName?: SQLTableRefName) {
        const refDoc = (tableRefName ? tableRefName + '.' : '') + 'document';
        const ops: SQLOperation[] = [];
        const refs = ['a', 'b', 'c'];
        operations.forEach(op => {
            let sqlText = tableRefName ? op.SQLText.replace('JSON_VALUE(document', 'JSON_VALUE(' + refDoc) : '';
            
            refs.forEach(ref => {
                if (ref !== tableRefName) {
                    sqlText = sqlText.replace('JSON_VALUE(' + ref + '.document', 'JSON_VALUE(' + refDoc);
                }
            });

            return new SQLOperation(sqlText);
        });
        return ops;
    }

    private static Right(operation: string, value: string | number | SQLJson.Value | SQLJson.QueryValue) {
        return new SQLOperation(operation, 'right', value);
    }

    private static Middle(operation: string, a: string | number | SQLJson.Value | SQLJson.QueryValue, b: string | number | SQLJson.Value | SQLJson.QueryValue, addQuotes?: boolean) {
        return new SQLOperation(operation, 'middle', a, b, addQuotes);
    }

    /*************** Variables ***************/
    public static AND = new SQLOperation('AND');
    public static OR = new SQLOperation('OR');

    public SQLText: string;

    /*************** Constructor ***************/
    private constructor(
        public operation: string,
        public type?: 'right' | 'middle',
        public a?: string | number | SQLJson.Value | SQLJson.QueryValue,
        public b?: string | number | SQLJson.Value | SQLJson.QueryValue,
        public addQuotes?: boolean
    ) {
        const A = this.a?.toString() || '';
        const B = this.b?.toString() || '';

        switch (this.type) {
            case 'right':
                this.SQLText = A + ' ' + this.operation;
                break;
            case 'middle':
                this.SQLText = A + ' ' + this.operation + ' ' + (addQuotes && typeof(b) == 'string' ?  "'" + B + "'" : B);
                break;
            default:
                this.SQLText = this.operation;
        }
    }

    /*************** Methods ***************/
    toString() {
        return this.SQLText;
    }
}