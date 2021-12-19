import { IRecordSet } from "mssql";
import { DatabaseService } from "../../../service/instances/database-service";
import { getJsonRefs, isNullOrUndefined } from "../../utils/methods";
import { JsonRef } from "../utils-model";
import { SQLMap, SQLMaps, SQLResultData } from "./sql-result";
import { SQLCount, SQLDoc, SQLGroupedValues, SQLGroupOptions, SQLGroupSelection, SQLGroupValue, SQLJoinOptions, SQLJson, SQLOffset, SQLOperation, SQLQueryOptions, SQLRecord, SQLRow, SQLTable, SQLTableCollection, SQLTableRefName } from "./sql-structure";

class SQLDocSource {
    constructor(public key: string, public outsideProject?: boolean) {}
}

type JsonEdit<T> = { [key in keyof T]?: T[key] extends object ? (T[key] extends Array<any> ? SQLJson.Modify<T[key]> : JsonEdit<T[key]>) : SQLJson.Modify<T[key]> }
type Optional<T> = { [key in keyof T]?: T[key] extends object ? (T[key] extends Array<any> ? T[key] : Optional<T[key]> )  : T[key] }
type FieldsSelect<T> = { [key in keyof T]?: T[key] extends object ? (T[key] extends Array<any> ? string | 1 : FieldsSelect<T[key]>) : string | 1 }

export interface SQLEditRows<Doc = any> {
    document?: Doc | JsonEdit<Doc>
}

export interface SQLEditRow<Doc = any> {
    _id: string
    document?: Doc | JsonEdit<Doc>
}

export class SQLStatement {
    constructor(public text: string, public map?: SQLMap<any>) {}

    toString() {
        return this.text;
    }
}

export class SQLTransaction<Doc = any, RowExtend = {}> {

    private statements: SQLStatement[] = [];

    constructor(private sqlTable: SQLTable, private collection?: string) {}

    private commitStatement(statement: string, map?: SQLMap<any>) {
        this.statements.push(new SQLStatement(statement, map));
        return this;
    }

    private _get(_id: string, _fields?: FieldsSelect<SQLRecord<Doc, RowExtend>>, customSelection?: string) {
        let fields: string[];
        
        if (_fields) {
            const fieldsRef = { ..._fields };
            fields = this.getSelectedFields(fieldsRef);
        }

        return `
            select ${fields ? fields.join(', ') : (customSelection || '*')} from [dbo].[${this.sqlTable.name}]
            where _id = '${_id}' ${ this.collection ? "And collection = '" + this.collection + "'" : '' }
            ${ fields ? 'for json path' : '' }
        `;
    }

    public get(_id: string) {
        return this.commitStatement(this._get(_id), (rows) => {
            if (!rows.length) {
                return null;
            }
            if (rows[0].document) {
                rows[0].document = JSON.parse(rows[0].document)
            }
            return rows[0];
        });
    }

    public getDoc(_id: string) {
        return this.commitStatement(
            this._get(_id, null, "JSON_MODIFY(document, '$._id', _id) as document"),
            rows => rows.length ? JSON.parse(rows[0].document) : null
        );
    }

    public getAs(_id: string, fields: FieldsSelect<SQLRecord<Doc, RowExtend>>) {
        return this.commitStatement(this._get(_id, fields), (rows) => {
            if (!rows.length) {
                return null;
            }
            const key = Object.keys(rows[0])[0];
            return JSON.parse(rows[0][key])[0];
        })
    }

    private getSelectedFields(selector: FieldsSelect<SQLRecord<Doc, RowExtend>>) {
        const document = selector.document;
        delete selector.document;
        let _fields = Object.keys(selector).map(key => selector[key] === 1 ? key : key + " as '" + selector[key] + "'");

        if (document) {
            const docFields = getJsonRefs(document).map(ref => SQLJson.VALUE(ref.path) + " as '" + (ref.value === 1 ? 'document.' + ref.path : ref.value) + "'");
            _fields = _fields.concat(docFields);
        }
        return _fields;
    }

    private parseRows(rows: IRecordSet<any>, arrOfDoc?: (key: string, value: any) => any) {
        const key = Object.keys(rows[0])[0];
        const text: string = rows[0][key];

        if (!text) {
            return [];
        }

        return JSON.parse(text
            .replace(/\\r\\n/g, '')
            .replace(/\\/g, '')
            .replace(new RegExp('"\\[', 'g'), '[')
            .replace(new RegExp('\\]"', 'g'), ']')
            .replace(/"{/g, '{')
            .replace(/}"/g, '}'), arrOfDoc);
    }

    private parseOutside(on: string = 'document') {
        let docFlag = false;
        return (key: string, value: any) => {
            docFlag = docFlag || key == on;
            if (docFlag && parseInt(key).toString() == key && value[on]) {
                docFlag = false;
                const content = value[on];
                delete value[on];
                return {
                    ...value,
                    ...content
                };
            }
            return value;
        }
    }
    
    private _find(options: SQLQueryOptions & { 
        where?: SQLOperation[],
        selection?: SQLGroupSelection,
        by?: [SQLGroupValue, SQLGroupValue?],
        groupedValues?: SQLGroupedValues,
        fields?: FieldsSelect<SQLRecord<Doc, RowExtend>>,
        andDelete?: boolean,
        fromSQL?: string,
        customSelect?: string,
        ignoreCollection?: boolean
    }) {
        let fields: string[];

        if (options.fields) {
            const fieldsRef = { ...options.fields };
            fields = this.getSelectedFields(fieldsRef);
        }

        const _where = options.where?.length ? [SQLOperation.GROUP(options.where)] : [];
        
        if (this.collection && !options.ignoreCollection) {
            if (_where.length) {
                _where.push(SQLOperation.AND);
            }
            _where.push(SQLOperation.EQUALS('collection', this.collection));
        }

        const from = options.fromSQL ? '(' + options.fromSQL + ')' : `[dbo].[${this.sqlTable.name}]`;
        const getBy = (tableRef: SQLTableRefName, by: SQLGroupValue) => ((typeof(by) == 'string' ? tableRef + '.' + by : by.clone(tableRef)));

        const aBy = options.by ? getBy('a', options.by[0]) : '';
        const secondAby = options.by?.length == 2 ? getBy('a', options.by[1]) : '';

        const groupSelection: string[] = []; 

        if (options.by && options.selection?.by) {
            [ aBy, secondAby ].forEach((by, index) => {
                if (options.selection.by[index + 1]) {
                    groupSelection.push(by + ' as ' + options.selection.by[index + 1])
                }
            });
        }

        if (options.selection?.count) {
            groupSelection.push(`Count(*) as ${ options.selection.count == 1 ? 'count' : options.selection.count }`);
        }

        let optionGroup = groupSelection.join(', ');

        if (options.groupedValues) {
            const tableRef = ['b', 'c'];
            options.groupedValues.forEach((value, index) => {
                if (!options.selection?.grouped[index + 1]) {
                    return;
                }
                let groupedValue = '';

                if (typeof(value) == 'string') {
                    groupedValue = value;
                } else {
                    groupedValue = value.SQLText;
                }
                
                const quote = `iif(isNumeric(${groupedValue}) = 0, '"', '')`;
                const groupWhere = [SQLOperation.EQUALS(aBy, getBy(<any>tableRef[index], options.by[0]), true)];

                if (options.by.length == 2) {
                    groupWhere.push(SQLOperation.AND);
                    groupWhere.push(SQLOperation.EQUALS(secondAby, getBy(<any>tableRef[index], options.by[1]), true));
                }

                optionGroup += (optionGroup.length ? ', ' : '') + ` ('[' + STUFF((select ',' + ${quote} + ${ groupedValue } + ${quote}
                from ${from} as ${ tableRef[index] }
                ${ groupWhere && groupWhere.length ? 'where ' + groupWhere.join(' ') : '' }
                FOR XML PATH(''),TYPE).value('(./text())[1]','NVARCHAR(MAX)')
                ,1,1,'') + ']') as '${ options.selection.grouped[index + 1] }'`;
            })
        }

        return `
            select ${ optionGroup ? optionGroup : (fields ? fields.join(', ') : options.customSelect || '*') }
            from ${from} ${ optionGroup || options.fromSQL ? 'as a' : '' }
            ${ _where.length ? 'where ' + _where.join(' ') : '' }
            ${ options.by ? 'group by ' + aBy + (secondAby ? ', ' + secondAby : '') : '' }
            ${ options.offset ? 'order by ' + options.offset.orderBy + ' ' + (options.offset.orderDirection || 'ASC') : '' }
            ${ options.offset ? 'OFFSET ' + options.offset.skip + ' ROWS ' : '' }
            ${ options.offset ? 'Fetch next ' + options.offset.limit + ' ROWS ONLY' : '' }
            for json path
            ${ options.andDelete ? this._deleteSQL(options.where) : '' }
        `;
    }

    public find(where: SQLOperation[], options?: SQLQueryOptions) {
        return this.commitStatement(this._find({ ...options, where }), this.parseRows);
    }

    public findDocs(where: SQLOperation[], options?: SQLQueryOptions) {
        return this.commitStatement(
            this._find({ ...options, where, customSelect: "JSON_MODIFY(document, '$._id', _id) as document" }),
            (rows) => this.parseRows(rows,  this.parseOutside)
        );
    }

    public findGroup(where: SQLOperation[], by: [SQLGroupValue, SQLGroupValue?], options?: SQLGroupOptions) {
        return this.commitStatement(this._find({ where, by, ...options }), rows => this.parseRows(rows, options.parseOutside ? this.parseOutside(options.parseOutside) : null));
    }

    public findAs(where: SQLOperation[], fields: FieldsSelect<SQLRecord<Doc, RowExtend>>, options?: SQLQueryOptions) {
        return this.commitStatement(this._find({ where, fields, ...options }), this.parseRows);
    }

    private _findJoin(options: SQLJoinOptions & { tableRefExtend?: string }, where?: SQLOperation[]) {
        const joins: string[] = [];
        const letters = ['b' + (options.tableRefExtend || ''), 'c' + (options.tableRefExtend || ''), 'd' + (options.tableRefExtend || '')];
        let selection: string[] = options.selection ? Array.from(options.selection.map.keys()).map(key => key + ' as ' + options.selection.map.get(key)) : [];

        const replacer = (value: string) => {
            if (!options.tableRefExtend) {
                return value;
            }
            ['a', 'b', 'c', 'd'].forEach(letter => {
                value = value.replace(new RegExp(letter + '\\.', 'g'), letter + options.tableRefExtend + '.');
            })

            return value;
        };

        const rename = (l: string) => col => l + '.' + col.name + ' as ' + col.name + l.toUpperCase();

        if (!options.fromSQL && !selection.length) {
            selection = this.sqlTable.columns.map(rename('a' + (options.tableRefExtend || '')));
        }

        options.sources.forEach((join, index) => {
            const on: SQLOperation[] = [SQLOperation.GROUP(join.on)];
            const source = join.source || this.sqlTable;
            const letter = letters[index];
            
            if (this.collection) {
                on.push(SQLOperation.AND);
                on.push(SQLOperation.EQUALS('a' + (options.tableRefExtend || '') + '.collection', this.collection));
            }
            
            if (source instanceof SQLTableCollection) {
                on.push(SQLOperation.AND);
                on.push(SQLOperation.EQUALS(letter + '.collection', source.name));
            }

            if (!selection.length) {
                selection = selection.concat((source instanceof SQLTableCollection ? source.sqlTable : source).columns.map(rename(letter)));
            }

            const sql = `inner JOIN [dbo].[${ source instanceof SQLTableCollection ? source.sqlTable.name : source.name }] as ${ letters[index] } ON ${ replacer(on.join(' ')) }`
            joins.push(sql);
        });

        const from = options.fromSQL ? '(' + options.fromSQL + ') as' : '[dbo].[' + this.sqlTable.name + '] as';

        return `
            select ${ replacer(selection.join(', ')) }
            from ${ from } a${(options.tableRefExtend || '')}
            ${ joins.join('\n') }
            ${ where && where.length ? 'where ' + where.map(op => replacer(op.SQLText)).join(' ') : '' }
            ${ options.offset ? 'order by ' + options.offset.orderBy + ' ' + (options.offset.orderDirection || 'ASC') : '' }
            ${ options.offset ? 'OFFSET ' + options.offset.skip + ' ROWS ' : '' }
            ${ options.offset ? 'Fetch next ' + options.offset.limit + ' ROWS ONLY' : '' }
            for json path
        `;
    }

    public findJoin(options: SQLJoinOptions & { parseOutside?: string }, where?: SQLOperation[]) {
        const keys: SQLDocSource[] = [];
        
        if (!options.selection) {
            if (!options.fromSQL) {
                keys.push(new SQLDocSource('documentA'));
            } else {
                const letters = ['b', 'c', 'd'];
                options.sources.forEach((src, index) => {
                    keys.push(new SQLDocSource('document' + letters[index].toUpperCase()));
                });
            }
        } else {
            Array.from(options.selection.map.keys()).forEach(selectKey => {
                const isQueryValue = selectKey instanceof SQLJson.QueryValue;
                const isDocument = typeof(selectKey) == 'string' && (selectKey.includes('.document')) || selectKey == 'document'; 
                if (isDocument || isQueryValue) {
                    keys.push(new SQLDocSource(options.selection.map.get(selectKey)));
                }
            });
        }
        return this.commitStatement(this._findJoin(options, where), rows => this.parseRows(rows, options.parseOutside ? this.parseOutside(options.parseOutside) : null));
    }

    public findJoinGroup(options: {
        join: SQLJoinOptions,
        group: {
            selection: SQLGroupSelection,
            by: [SQLGroupValue, SQLGroupValue?],
            groupedValues?: SQLGroupedValues,
            parseOutside?: string
        },
        offset?: SQLOffset
    }, where?: SQLOperation[]) {
        const group = options.group;

        let joinSQL = this._findJoin({...options.join, tableRefExtend: 'J'})
        .replace('for json path', '');

        const sql = this._find({
            ...group,
            offset: options.offset,
            fromSQL: joinSQL,
            where,
            ignoreCollection: true
        });

        return this.commitStatement(sql, rows => this.parseRows(rows, group.parseOutside ? this.parseOutside(group.parseOutside) : null));
    }

    private _deleteSQL(where: SQLOperation[]) {
        return `
            delete from [dbo].[${this.sqlTable.name}]
            where ${ where.join(' ') } ${ this.collection ? "AND collection = '" + this.collection + "'" : ''}
        `;
    }

    public delete(where: SQLOperation[]) {
        return this.commitStatement(this._deleteSQL(where));
    }
        
    private _insertSQL(record: SQLRow<Doc> & RowExtend, multiIndex?: number, lastIndex?: boolean): string {
        const keys = Object.keys(record);
        const autoCreateID = !keys.includes('_id'); 
        
        const names = (autoCreateID ? ['_id', ...keys] : keys);
        const values = keys.map((key) => SQLQuery.SQLValueMap(record, key));

        if (autoCreateID) {
            values.unshift("JSON_VALUE(@ids, '$[" + (multiIndex || 0) + "]')");
        }

        if (this.collection) {
            names.push('collection');
            values.push(SQLQuery.SQLValueMap({ collection: this.collection }, 'collection'));
        }

        const sqlStatement = `
            ${!multiIndex ? "declare @ids nvarchar(max) = JSON_QUERY('[]')" : '' }
            set @ids = JSON_MODIFY(@ids, 'append $', ${autoCreateID ? 'CONVERT(nvarchar(max), NEWID())'  : SQLQuery.SQLValueMap(record, '_id') });
            
            INSERT INTO [dbo].[${this.sqlTable.name}] (${names.join(', ')})
            VALUES (${values.join(', ')});

            ${ lastIndex ? 'select @ids as ids' : '' }
        `;

        return sqlStatement;
    }
    
    public insert(record: SQLRow<Doc> & RowExtend) {
        return this.commitStatement(this._insertSQL(record, 0, true), (rows) => JSON.parse(rows[0].ids)[0]);
    }

    public bulkInsert(records: (SQLRow<Doc> & RowExtend)[]) {
        const statement = records.map((record, index) => this._insertSQL(record, index, index === records.length - 1)).join('; \n');
        return this.commitStatement(statement, (rows) => JSON.parse(rows[0].ids));
    }

    private addConfirmKeys(ref: JsonRef, appendTo: string[]) {
        const pathSplit = ref.path.split('.');
        pathSplit.pop();

        let currentPath = '';
        pathSplit.forEach(key => {
            currentPath += (currentPath.length ? '.' : '') + key;
            const jsonStr = `@json = CASE (ISNULL(JSON_QUERY(@json, '$.${currentPath}'), ''))
            WHEN '' THEN JSON_MODIFY(@json, '$.${currentPath}', JSON_QUERY('{}')) ELSE @json END`
            if (!appendTo.includes(jsonStr)) {
                appendTo.push(jsonStr);
            };
        });
    }

    private _updateSQL(record: SQLEditRows<Doc> & Optional<RowExtend> & { collection?: string }, where?: string, newId?: string): string {
        const hasNewId = !isNullOrUndefined(newId);
        let jsonSQL: string[] = [];
        
        if (record.document) {
            const jsonRefs = getJsonRefs(record.document, [SQLJson.Modify]);
            const isJsonEdit = jsonRefs.length && jsonRefs[0].value instanceof SQLJson.Modify;
            // update json fields
            if (isJsonEdit) {
                jsonSQL.push('@json = document');
                
                const addJsonValue: string[] = [];
                const confirmKeys: string[] = [];

                jsonRefs.forEach((ref) => {
                    this.addConfirmKeys(ref, confirmKeys);
                    const value: SQLJson.Modify = ref.value;
                    addJsonValue.push('@json = ' + value.modify(ref.path, '@json').SQLText);
                });

                jsonSQL = jsonSQL.concat(confirmKeys);
                jsonSQL = jsonSQL.concat(addJsonValue);

                jsonSQL.push('document = @json');
            }
        }

        const keys = Object.keys(record).filter(key => key !== '_id' && (key !== 'document' || !jsonSQL.length));

        let  initialisation = (hasNewId ? [ "_id = '" + newId + "'" ] : []).concat(
            keys
            .map((key) => key + ' = ' + SQLQuery.SQLValueMap(record, key))
        ).join(', ');

        return `
            ${ jsonSQL.length ? 'declare @json nvarchar(max);' : '' }
            update [dbo].[${this.sqlTable.name}]
            set ${ jsonSQL.join(', \n') + (jsonSQL.length && initialisation ? ',' : '') + initialisation }
            ${ where ? 'where ' + where : '' }
        `;
    }

    public update(record: SQLEditRow<Doc> & Optional<RowExtend> & { collection?: string }, newId?: string) {
        const where = [
            SQLOperation.EQUALS('_id', record._id)
        ];

        if (this.collection) {
            where.push(SQLOperation.AND);
            where.push(SQLOperation.EQUALS('collection', this.collection));
        }
        const statement = this._updateSQL(record, where.join(' '), newId);
        return this.commitStatement(statement);
    }

    public updateMany(data: SQLEditRows<Doc> & Optional<RowExtend> & { collection?: string }, where?: SQLOperation[]) {
        const _where = where && where.length ? [SQLOperation.GROUP(where)] : [];
        
        if (this.collection) {
            if (_where.length) {
                _where.push(SQLOperation.AND);
            }
            _where.push(SQLOperation.EQUALS('collection', this.collection));
        }
        const statement = this._updateSQL(data, _where.join(' '));
        return this.commitStatement(statement);
    }

    public createTable() {
        return this.commitStatement(`
            CREATE TABLE [dbo].[${this.sqlTable.name}](${this.sqlTable.columns.map(col => col.SQLText).join(', ') })
        `);
    }

    public count(where?: SQLOperation[]) {
        const _where = where && where.length ? [SQLOperation.GROUP(where)] : [];
        
        if (_where.length) {
            _where.push(SQLOperation.AND);
        }

        if (this.collection) {
            _where.push(SQLOperation.EQUALS('collection', this.collection));
        } else {
            _where.push(SQLOperation.NOT_EXISTS('collection'));
        }

        const whereStatement = _where.join(' ');

        return this.commitStatement(`
            select Count(*) as count from [dbo].[${this.sqlTable.name}]
            ${ whereStatement ? 'where ' + whereStatement : '' }
        `, (rows) => rows[0]);
    }

    public customSQL(statement: string) {
        return this.commitStatement(statement);
    }

    public concat(...transactions: SQLTransaction[]) {
        transactions.forEach(t => this.statements = this.statements.concat(t.statements));
        return this;
    }

    public getStatements(): string {
        return this.statements.map(s => s.toString()).join('\n');
    }

    public executeOne<Result = void>(map?: SQLMap<Result>) {
        const sqlStatement = this.statements.shift();
        return DatabaseService.query<Result>(sqlStatement.text, map || sqlStatement.map);
    }

    public execute<Result = any>(maps?: SQLMaps<Result>) {
        const sqlStatement = this.statements.join(';\n');
        const _defaultMaps = this.statements.map(s => s.map);
        this.statements = [];
        return DatabaseService.querySets<Result>(sqlStatement, maps || <any> _defaultMaps);
    }

}

export class SQLCollectionQuery<Doc = any, RowExtend extends { [key: string]: string | number } = {}> {
    
    public get transaction() {
        return new SQLTransaction<Doc, RowExtend>(this.sqlTable, this.collection);
    }

    constructor(protected sqlTable: SQLTable, private collection?: string) {}

    public get(_id: string): Promise<SQLResultData<SQLRecord<Doc, RowExtend>>> {
        return this.transaction.get(_id).executeOne();
    }

    public getDoc(_id: string) {
        return this.transaction.getDoc(_id).executeOne<SQLDoc<Doc & RowExtend>>();
    }

    public getAs<RecordType>(_id: string, fields: FieldsSelect<SQLRecord<Doc, RowExtend>>): Promise<SQLResultData<SQLRow<RecordType>>> {
        return this.transaction.getAs(_id, fields).executeOne();
    }

    public find(where: SQLOperation[], options?: SQLQueryOptions): Promise<SQLResultData<SQLRecord<Doc, RowExtend>[]>> {
        return this.transaction.find(where, options).executeOne();
    }

    public findDocs(where: SQLOperation[], options?: SQLQueryOptions) {
        return this.transaction.findDocs(where, options).executeOne<SQLDoc<Doc & RowExtend>[]>();
    }

    public findGroup<Result>(where: SQLOperation[], by: [SQLGroupValue, SQLGroupValue?], options?: SQLGroupOptions) {
        return this.transaction.findGroup(where, by, options).executeOne<Result[]>();
    }

    public findAs<RecordType>(where: SQLOperation[], fields: FieldsSelect<SQLRecord<Doc, RowExtend>>, options?: SQLQueryOptions) {
        return this.transaction.findAs(where, fields, options).executeOne<RecordType[]>();
    }

    public findJoin<RecordType>(options: SQLJoinOptions & { parseOutside?: string }, where?: SQLOperation[]) {
        return this.transaction.findJoin(options, where).executeOne<RecordType[]>();
    }

    public findJoinGroup<Result>(options: {
        join: SQLJoinOptions,
        group: {
            selection: SQLGroupSelection,
            by: [SQLGroupValue, SQLGroupValue?],
            groupedValues?: SQLGroupedValues,
            parseOutside?: string
        },
        offset?: SQLOffset
    }, where?: SQLOperation[]) {
        return this.transaction.findJoinGroup(options, where).executeOne<Result[]>();
    }

    public insert(record: SQLRow<Doc> & RowExtend) {
        return this.transaction.insert(record).executeOne<string>();
    }

    public bulkInsert(records: (SQLRow<Doc> & RowExtend)[]) {
        return this.transaction.bulkInsert(records).executeOne<string[]>();
    }

    public update(record: SQLEditRow<Doc> & Optional<RowExtend>, newId?: string) {
        return this.transaction.update(record, newId).executeOne();
    }

    public updateMany(data: SQLEditRows<Doc> & Optional<RowExtend>, where?: SQLOperation[]) {
        return this.transaction.updateMany(data, where).executeOne();
    }

    public delete(_id: string) {
        return this.transaction.delete([ SQLOperation.EQUALS('_id', _id) ]).executeOne();
    }
    
    public deleteMany(where: SQLOperation[]) {
        return this.transaction.delete(where).executeOne();
    }

    public count(where?: SQLOperation[]) {
        return this.transaction
            .count(where)
            .executeOne<SQLCount>();
    }

}

export class SQLQuery<Doc = any, RowExtend extends { [key: string]: string | number } = {}> extends SQLCollectionQuery<Doc, RowExtend> {

    public static SQLValueMap(record: any, key: string) {
        const value = record[key];
        const type = typeof(value);
        const isObject = type === 'object';

        if (isObject || type === 'string') {
            return "N'" + (isObject ? JSON.stringify(value) : value) + "'";
        } else {
            return value;
        }
    }

    public static createTables(tables: SQLTable[]) {
        const transaction = new SQLTransaction(null);
        tables.forEach(table => transaction.concat(table.query().transaction.createTable()));
        return transaction.executeOne();
    }

    public static listTables(): Promise<SQLResultData<string[]>> {
        const sqlStatement = `
            Select TABLE_NAME from blocks.INFORMATION_SCHEMA.TABLES;
        `;

        return DatabaseService.query<string[]>(
            sqlStatement, 
            (rows) => rows.map(col => col.TABLE_NAME)
        );
    }

    public static Pagination(pageNumber: number, numberOfDocsPerPage: number, orderBy: string | SQLJson.Value) {
        return {
            limit: numberOfDocsPerPage,
            skip: (pageNumber - 1) * numberOfDocsPerPage,
            orderBy
        };
    }
    
    public createTable() {
        return this.transaction.createTable().executeOne();
    }

}
