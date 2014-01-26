
class MemoryManager #: Events.EventEmitter
  length: 0
  constructor: () ->

  writeBytes: (array, index) =>
    for i in [index...index + array.length]
      bufferIndex = i;
      arrayIndex = i - index;
      ##console.log "writeBytes buffer[#{bufferIndex}] = #{array[arrayIndex]}[#{arrayIndex}]"
      @buffer[bufferIndex] = array[arrayIndex];
  setLength: (length) =>
    #console.log "[MemoryManager]  SetLength #{length}";
    @buffer = new Buffer(length);
    @length = length;
  write: (source) => 
    #console.log "[MemoryManager]  Write source #{source.length} #{@buffer?}";
    if not @buffer?
      @buffer = new Buffer(source.length);
      source.copy(@buffer);
    else
      @buffer.concat(source)
    @length = @buffer.length;
  remove: (start, end = @buffer.length) =>
    sectionSize = start - end;
    if sectionSize > 0 and end < @buffer.length
      #console.log "[MemoryManager]  Remove #{start} #{end}";
      @newBuffer = new Buffer(@buffer.length - sectionSize);
      if(start > 0)
        @buffer.copy newBuffer, 0, 0, start;
      if end < @buffer.length
        @buffer.copy newBuffer, end, @buffer.length
      @buffer = newBuffer;
    else if start == 0 and end == @buffer.length
      #console.log "[MemoryManager] setting buffer to null";
      @buffer = null;
  slice: (start, end) =>
    newBufferSize = end - start;
    #console.log "[MemoryManager] Slice start: #{start} end: #{end} newSize: #{newBufferSize} buffer: #{@buffer.length}"
    newBuffer = new Buffer(end - start);
    #console.log "[MemoryManager] #{newBuffer.length}";
    @buffer.copy newBuffer, 0, start, end
    #console.log "[MemoryManager] Slice end", newBuffer
    return newBuffer;

  clear: () =>
    #console.log "[MemoryManager]  Clear";
    @buffer = null;
    @length = 0;

  compare: (index, array) =>
    for i in [index...index+array.length]
      bufferIndex = i;
      arrayIndex = i - index;

      ##console.log " compare: #{@buffer[bufferIndex]}[#{bufferIndex}] ==  #{array[arrayIndex]}[#{arrayIndex}] Length: #{@buffer.length} ";

      if @buffer[bufferIndex] != array[arrayIndex]
        return false;
    return true;

  getBuffer: () =>
    return @buffer;
  readInt32: (index) =>
    return @buffer.readInt32LE(index);

module.exports = MemoryManager;