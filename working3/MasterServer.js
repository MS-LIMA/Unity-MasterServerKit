import {sc} from './lib';

///////////////////////////////////////////////////////////////////////////
var ip = "127.0.0.1";
var port = 3333;

var fs = require('fs');

///////////////////////////////////////////////////////////////////////////
// Read config file.
fs.readFile('config.json', 'utf8', (err, data) => {
    if (err) {
        console.error(err);
        return;
    }

    const json = JSON.parse(data);
    ip = json.ip;
    port = json.port;

    console.log(json);

    startServer(json);
});

///////////////////////////////////////////////////////////////////////////
// Start server.

const startServer = (config) => {
    var wsServer = require('ws').Server;
    var wss = new wsServer({ port: port });

    sc.init(config);

    console.log('Server opened on port %d.', port);

    wss.on('connection', function connection(socket) {    
        console.log("Client connected");
        
        socket.on('message', (message) => {
            sc.onSocketMessage(socket, message);
        });
    
        socket.on('close', () => {
            sc.onSocketClose(socket);
        });
    });
}

