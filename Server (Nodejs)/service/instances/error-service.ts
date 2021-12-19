import { ErrorState } from '../../package/models/error';
import { Service } from '../service';

export class ErrorService extends Service {
    
    /*************** Methods ***************/
    public log(classReference: { name: string }, err: ErrorState): void {
        console.log('Class name: ' + classReference.name);
        console.log('Error code: ' + err.code);
        if (err.message) {
            console.log('message: ' + err.message);
        }
        console.log('-----------------------------------------');
    }

}