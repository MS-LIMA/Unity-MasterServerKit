import {sc} from './lib';

///////////////////////////////////////////////////////////////////////////
var ip = "127.0.0.1";
var port = 3333;
var wsServer = require('ws').Server;
var wss = new wsServer({ port: port });

var fs = require('fs');

///////////////////////////////////////////////////////////////////////////
// Read config file.
fs.readFile('config.json', 'utf8', (err, data) => {
    if (err) {
        console.error(err);
        return;
    }

    console.log(data);
    startServer();
});

///////////////////////////////////////////////////////////////////////////
// Start server.

const startServer = () => {
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

