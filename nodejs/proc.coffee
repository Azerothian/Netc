# Load the TCP Library
Net = require 'net';
#Buffer = require 'buffer';
Events = require 'events';
StringDecoder = require('string_decoder').StringDecoder;

class TcpServer
  constructor: () ->
    @clients = []
  startListening: (port) =>
    @server = Net.createServer(@onCreateSocket)
    @server.listen(port)
  onCreateSocket: (socket) =>
    socket.name = socket.remoteAddress + ":" + socket.remotePort 
    client = new TcpClient(socket, @);
    @clients.push client
    client.on 'data', @onClientData
    socket.on 'end', () =>
      #TODO fire disconnect
      delete @clients[socket];
  onClientData: (client, data) =>
    decoder = new StringDecoder('utf8');
    result =  decoder.write(data);
    console.log("length: #{data.length}, result: #{result}");
    obj = JSON.parse(result);
    client.sendData(obj);
    #console.log "CLIENT DATA", data;
    #@emit 'data', client, data;


class TcpClient extends Events.EventEmitter
  constructor: (@socket) ->
    @endOfFile = [0xff,0xff,0xff,0xff]
    @startOfFile = [0xfe,0xff,0xff,0xff]
    @endOfHeader = [0xfd,0xff,0xff,0xff]
    @memory = new MemoryManager();
    @packets = [];
    @socket.on 'data', @onData

  onData: (data) =>
    @memory.write(data);
    @scanForPackets();

  sendData: (data) =>
    str = JSON.stringify(data);
    buf = new Buffer(str, 'utf8');
    bufPack = new MemoryManager();
    packetSize = @startOfFile.length + 4 + @endOfHeader.length + buf.length + @endOfFile.length;
    console.log("[SEND] packetSize #{packetSize}");
    bufPack.setLength(packetSize);
    index = 0;
    bufPack.writeBytes(@startOfFile, index);
    index += @startOfFile.length;
    bufPack.buffer.writeInt32LE(buf.length, index);
    index += 4
    bufPack.writeBytes(@endOfHeader, index);
    index += @endOfHeader.length;
    bufPack.writeBytes(buf, index);
    index += buf.length;
    bufPack.writeBytes(@endOfFile, index);
    @socket.write(bufPack.buffer);


  scanForPackets: () =>
    buffer = @memory.getBuffer();
    for i in [0...@memory.length]
      index = i;
      if index + @startOfFile.length + 4 + @endOfHeader.length > buffer.length
        console.log  "To Short";
        break;

      if not @memory.compare(index, @startOfFile)
        #console.log  "Start Index not found";
        continue;
      #console.log  "Start Index found";
      index += @startOfFile.length;

      packetSize = buffer.readInt32LE(index);

      #console.log "packetSize #{packetSize}";
      index += 4;
      #console.log  "Checking for End of Header";
      if not @memory.compare(index, @endOfHeader)
        #console.log  "End of Header not found";
        continue;

      index += @endOfHeader.length;
      if (@memory.length < index + packetSize + @endOfFile.Length) # stream not finished yet;
        #console.log  "Stream not finished yet";
        break;
      #console.log  "checking for end of file";
      if not @memory.compare index + packetSize, @endOfFile
        #console.log  "End of file not found";
        continue;
      #console.log "slice #{index}  #{packetSize} #{packetSize - index}"
      data = buffer.slice(index, index+packetSize);
      totalPacketSize = @startOfFile.Length + 4 + @endOfHeader.Length + packetSize + @endOfFile.Length;
      @memory.remove 0, i + totalPacketSize
      #console.log  "complete";
      @emit 'data', @, data;

      break;



class MemoryManager #: Events.EventEmitter
  length: 0
  constructor: () ->

  writeBytes: (array, index) =>
    for i in [index...index + array.length]
      bufferIndex = i;
      arrayIndex = i - index;
      #console.log "writeBytes buffer[#{bufferIndex}] = #{array[arrayIndex]}[#{arrayIndex}]"
      @buffer[bufferIndex] = array[arrayIndex];
  setLength: (length) =>
    @buffer = new Buffer(length);
    @length = length;
  write: (source) => 
    if not @buffer?
      @buffer = new Buffer(source.length);
      source.copy(@buffer);
    else
      @buffer.concat(source)
    @length = @buffer.length;
  remove: (index, count) =>
    newBuffer = new Buffer(1);
    if index > 0
      newBuffer = @buffer.slice(0, index)
    if index + count < @buffer.length
      end = @buffer.slice(index +count)
      newBuffer.concat(end);
    @buffer = newBuffer;
    @length = @buffer.length;
  clear: () =>
    @buffer = null;
    @length = 0;

  compare: (index, array) =>
    for i in [index...index+array.length]
      bufferIndex = i;
      arrayIndex = i - index;

      console.log " compare: #{@buffer[bufferIndex]}[#{bufferIndex}] ==  #{array[arrayIndex]}[#{arrayIndex}] Length: #{@buffer.length} ";

      if @buffer[bufferIndex] != array[arrayIndex]
        return false;
    return true;

  getBuffer: () =>
    return @buffer;

server = new TcpServer()
server.startListening(6112);


console.log("Chat server running at port 6112\n");