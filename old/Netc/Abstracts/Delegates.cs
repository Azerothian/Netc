using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Netc.Abstracts
{
	public delegate void OnConnectingDelegate<T>(T client) where T : NetworkAbstractStream<T>;
	public delegate void OnConnectedDelegate<T>(T client) where T : NetworkAbstractStream<T>;
	public delegate void OnDisconnectingDelegate<T>(T client) where T : NetworkAbstractStream<T>;
	public delegate void OnDisconnectedDelegate<T>(T client) where T : NetworkAbstractStream<T>;
	public delegate void OnDisconnectDelegate<T>(T client) where T : NetworkAbstractStream<T>;
	public delegate void OnMessageReceivedDelegate<T>(T client, int bytesReceived) where T : NetworkAbstractStream<T>;
	public delegate void OnMessageSentDelegate<T>(T client, int bytesSent) where T : NetworkAbstractStream<T>;
}
