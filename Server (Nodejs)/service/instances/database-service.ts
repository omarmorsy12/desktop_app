import { AsyncComponent } from '../../package/models/aysnc-component';
import { Service } from '../service';
import { ErrorState } from '../../package/models/error';
import { SQLTable } from '../../package/models/sql/sql-structure';
import { ConnectionPool } from 'mssql';
import { Ports } from '../../package/models/ports';
import { SQLQuery } from '../../package/models/sql/sql-query';
import { SQLMaps, SQLResultSets, SQLMap, SQLResultData } from '../../package/models/sql/sql-result';
import { SQLTables } from '../../package/builders/sql-tables';

export class DatabaseService extends Service implements AsyncComponent {

    /*************** Variables ***************/
    public readonly tables = SQLTables.build();
    
    public static Server: ConnectionPool;

    /*************** Methods ***************/
    public static async querySets<T>(sqlStatement: string, mapping?: SQLMaps<T>) {
        try {
            const result = await this.Server.query(sqlStatement);
            return new SQLResultSets<T>(result, mapping);
        } catch(err) {
            console.log(sqlStatement, err);
            return SQLResultSets.QueryError<T>(err);
        }
    }
    
    public static async query<T>(sqlStatement: string, mapping?: SQLMap<T>) {
        try {
            const result = await this.Server.query(sqlStatement);
            return new SQLResultData<T>(result, mapping);
        } catch(err) {
            console.log(sqlStatement, err);
            return SQLResultData.QueryError<T>(err);
        }
    }

    private async connect() {
        const sql = require('mssql');

        DatabaseService.Server = new sql.ConnectionPool({
            user: 'admin',
            password: 'admin',
            database: 'blocks',
            server: 'localhost',
            port: Ports.DATABASE,
            options: {
                trustServerCertificate: true
            }
        });

        await DatabaseService.Server.connect();
    }

    public async asyncInit(): Promise<ErrorState> {
        try {
            await this.connect();
            // const query = new SQLTable<{ name: string }>('test').query();
        } catch(err) {
            return ErrorState.DATABASE.CONNECTION.addMessage(err.message);
        }

        const defaultTables = Object.keys(this.tables);
        const createdTables = await SQLQuery.listTables();
        
        const tables: SQLTable[] = [];

        defaultTables.forEach(key => {
            const table: SQLTable = this.tables[key];
            if (!createdTables.data.includes(table.name)) {
                tables.push(table);
            }
        });

        return tables.length ? (await SQLQuery.createTables(tables)).error : null;
    }

    public getClassRef() {
        return DatabaseService;
    }

}