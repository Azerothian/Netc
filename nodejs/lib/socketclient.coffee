
Events = require 'events';
StringDecoder = require('string_decoder').StringDecoder;

class SocketClient
  constructor: (@tcpClient) ->
    @events = new Events.EventEmitter();
    @tcpClient.on "data", @onClientData;

  onClientData: (data) =>
    decoder = new StringDecoder('utf8');
    result =  decoder.write(data);
    obj = JSON.parse(result);
    @events.emit "#{obj.Message}", obj.Contents, @
  on: (event, func) =>
    @events.on event, func;
  emit: () =>
    obj = {};
    obj.Message = arguments[0];
    obj.Contents = arguments[i] for i in [1...arguments.length];
    str = JSON.stringify(obj);
    buffer = new Buffer(str, "utf8");
    @tcpClient.sendData(buffer);

module.exports = SocketClient;