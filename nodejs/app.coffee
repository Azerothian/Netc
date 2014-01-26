Net = require 'net';

SocketServer = require './lib/socketserver';


server = new SocketServer()

server.on "connected", (client) =>
  console.log "client connected", client;
  client.on "response", (data) =>
    console.log "client emitting response", data;
    client.emit "response", data;

server.startListening(6112);


console.log("Chat server running at port 6112\n");