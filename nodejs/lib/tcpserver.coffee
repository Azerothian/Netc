
Net = require 'net';
Events = require 'events';
TcpClient = require './tcpclient';
class TcpServer extends Events.EventEmitter
  constructor: () ->
    @clients = []
  startListening: (port) =>
    @server = Net.createServer(@onCreateSocket)
    @server.listen(port)
  onCreateSocket: (socket) =>
    socket.name = socket.remoteAddress + ":" + socket.remotePort 
    client = new TcpClient(socket, @);
    @clients.push client;
    #client.on 'data', @onClientData;
    client.on 'disconnect', @onClientDisconnect;

    @emit "connected", client;

  onClientDisconnect: (client) =>
    @emit "disconnect", client;
    delete @clients[client.socket];

module.exports = TcpServer;