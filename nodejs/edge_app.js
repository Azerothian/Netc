
var util = require('util');
var edge = require('edge');




var start = edge.func('Netc.Sock.NodeJs.dll');


start('', function (error, result) {

	//console.log(result);
	result.StartListening(6112);
	result.On({ eventName: "response",
		callback: function(input, callback) {
			console.log("callback");
			console.log(util.inspect(input));
			result.Emit({
				client: input.clientId, 
				eventName: "response", 
				data: input.data 
			});
		}
	});
	//

});







var keypress = require('keypress')
  , tty = require('tty');

// make `process.stdin` begin emitting "keypress" events
keypress(process.stdin);

// listen for the "keypress" event
process.stdin.on('keypress', function (ch, key) {
  console.log('got "keypress"', key);
  if (key && key.ctrl && key.name == 'c') {
    process.stdin.pause();
  }
});

if (typeof process.stdin.setRawMode == 'function') {
  process.stdin.setRawMode(true);
} else {
  tty.setRawMode(true);
}
process.stdin.resume();
