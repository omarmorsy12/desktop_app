import { App } from './app';
import { Ports } from './package/models/ports';

new App()
    .initialize()
    .then(App => App.start(Ports.APP, 'Server started'));
