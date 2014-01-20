using Netc.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Netc.Abstracts
{


	public abstract class NetworkAbstractServer<T> where T : NetworkAbstractStream<T>
	{
		public abstract void Disconnect();
		public abstract event OnConnectedDelegate<T> OnClientConnectedEvent;
		public abstract event OnDisconnectDelegate<T> OnClientDisconnectEvent;
		public abstract event OnMessageReceivedDelegate<T> OnClientMessageReceivedEvent;
		public abstract event OnMessageSentDelegate<T> OnClientMessageSentEvent;
		public abstract void StartListening(int Port);

	}
}
