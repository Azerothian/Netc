SocketClient = require './socketclient';
TcpServer = require './tcpserver'
Events = require 'events';

class SocketServer
  constructor: () ->
    @clients = [];
    @events = new Events.EventEmitter();
    @server = new TcpServer();
    @server.on "connected", @onClientConnect;
    @server.on "disconnect", @onClientDisconnect;

  startListening: (port) =>
    @server.startListening(port);

  onClientConnect: (tcpClient) =>
    @clients[tcpClient] = new SocketClient(tcpClient);
    @events.emit "connected", @clients[tcpClient];

  onClientDisconnect: (tcpClient) =>
    @events.emit "disconnect", @clients[tcpClient];
    delete @clients[tcpClient]
  on: (event, func) =>
    @events.on event, func;
  all: (event) =>
    args = arguments[i] for i in [1...arguments.length];
    for client in @clients
      client.emit event, args;
  exclude: (clientsToExclude, event) =>
    args = arguments[i] for i in [2...arguments.length];
    cli = c for c in @clients when c not in clientsToExclude;
    for client in cli
      client.emit event, args;

module.exports = SocketServer;