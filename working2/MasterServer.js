///////////////////////////////////////////////////////////////////////////
import lib from "./lib";

///////////////////////////////////////////////////////////////////////////
var ip = "127.0.0.1";
var port = 3333;
var wsServer = require('ws').Server;
var wss = new wsServer({ port: port });

var fs = require('fs');
///////////////////////////////////////////////////////////////////////////
// Read config file.
fs.readFile('config.txt', 'utf8', (err, data) => {
    if (err) {
        console.error(err);
        return;
    }



    console.log(data);
});
///////////////////////////////////////////////////////////////////////////
// Start server.













var lobbies = {};

console.log('Server opened on port %d.', port);

wss.on('connection', function connection(socket) {
    console.log("Client connected");

    socket.on('message', (message) => {
        let jObject = JSON.parse(message);

    });

    socket.on('close', () => {
        //OnSocketDisconnected(socket);
    });
});