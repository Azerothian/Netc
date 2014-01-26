Net = require 'net';
Events = require 'events';
MemoryManager = require './memorymanager'



class TcpClient extends Events.EventEmitter
  constructor: (@socket) ->
    @endOfFile = [0xff,0xff,0xff,0xff]
    @startOfFile = [0xfe,0xff,0xff,0xff]
    @endOfHeader = [0xfd,0xff,0xff,0xff]
    @memory = new MemoryManager();
    @packets = [];
    @socket.on 'data', @onSocketData
    @socket.on "error", @onSocketError 
    @socket.on "end", @onSocketEnd
  onSocketEnd: (socket) =>
    @emit "disconnect", @;
  onSocketError: (err) =>
    console.log err
  onSocketData: (data) =>
    #console.log "onData", data
    @memory.write(data);
    @scanForPackets();

  sendData: (buf) =>
    bufPack = new MemoryManager();
    packetSize = @startOfFile.length + 4 + @endOfHeader.length + buf.length + @endOfFile.length;
    #console.log("[SEND] packetSize #{packetSize}");
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
    #buffer = @memory.getBuffer();
    for i in [0...@memory.length]
      index = i;
      if index + @startOfFile.length + 4 + @endOfHeader.length > @memory.length
        #console.log  "To Short";
        break;

      if not @memory.compare(index, @startOfFile)
        #console.log  "Start Index not found";
        continue;
      #console.log  "Start Index found";
      index += @startOfFile.length;

      packetSize = @memory.readInt32(index);

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
      data = @memory.slice(index, index+packetSize);
      totalPacketSize = @startOfFile.length + 4 + @endOfHeader.length + packetSize + @endOfFile.length;
      @memory.remove 0, i + totalPacketSize
      #console.log  "complete", data;
      @emit 'data', data;

      break;


module.exports = TcpClient;