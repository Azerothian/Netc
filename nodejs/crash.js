var buffer = new Buffer(512);
var slicedArray = buffer.slice(0, 256);

var newBuffer = new Buffer(256);
var output = newBuffer.concat(slicedArray);