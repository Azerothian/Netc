using System;
namespace Netc.Abstracts
{
	public interface INetworkAbstract
	{
		void Connect(string IpAddress, int Port);
		void CreateFromSocket(System.Net.Sockets.Socket s);
		void Disconnect();
		void Send(byte[] data);
		event GenericDelegate<INetworkAbstract> OnConnectedEvent;
		event GenericDelegate<INetworkAbstract> OnConnectingEvent;
		event GenericDelegate<INetworkAbstract> OnDisconnectedEvent;
		event GenericDelegate<INetworkAbstract> OnDisconnectingEvent;
		event GenericDelegate<INetworkAbstract, byte[], int> OnMessageReceivedEvent;
		event GenericDelegate<INetworkAbstract, int> OnMessageSentEvent;
	}
}
