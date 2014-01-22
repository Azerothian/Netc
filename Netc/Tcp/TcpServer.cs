using Netc.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Netc.Tcp
{
	public class TcpServer
	{
		private Dictionary<string, TcpClient> _clients = new Dictionary<string, TcpClient>();
		private Socket server;

		public void Disconnect()
		{
			foreach (var v in _clients.Keys)
			{
				_clients[v].Disconnect();
			}
			if (server.Connected)
			{
				server.Shutdown(SocketShutdown.Both);
			}
			server.Close();
		}

		public void StartListening(int Port)
		{
			IPEndPoint ipep = new IPEndPoint(IPAddress.Any, Port);

			server = new Socket(AddressFamily.InterNetwork,
							SocketType.Stream, ProtocolType.Tcp);
			server.NoDelay = true;
			server.Bind(ipep);
			server.Listen(10);
			server.BeginAccept(new AsyncCallback(AcceptConnection), server);
		}

		public event GenericVoidDelegate<TcpClient> OnClientConnectedEvent;
		public event GenericVoidDelegate<TcpClient> OnClientDisconnectEvent;
		public event GenericVoidDelegate<TcpClient, byte[], int> OnClientMessageReceivedEvent;
		public event GenericVoidDelegate<TcpClient, int> OnClientMessageSentEvent;
		public event GenericVoidDelegate<TcpClient, byte[]> OnClientMessageReceiveCompleted;
		
		private void AcceptConnection(IAsyncResult iar)
		{
			try
			{
				Socket oldserver = (Socket)iar.AsyncState;
				Socket client = oldserver.EndAccept(iar);
				client.NoDelay = true;
				var stream = new TcpClient();
				stream.CreateFromSocket(client);
				_clients.Add(String.Format("{0}", client.RemoteEndPoint), stream);
				var key = client.RemoteEndPoint.ToString();
				if (OnClientConnectedEvent != null)
				{
					OnClientConnectedEvent(_clients[key]);
				}

				_clients[key].OnConnectedEvent += TcpServer_OnConnectedEvent;
				_clients[key].OnDisconnectedEvent += TcpServer_OnDisconnectedEvent;

				_clients[key].OnMessageReceivedEvent += TcpServer_OnMessageReceivedEvent;
				_clients[key].OnMessageReceiveCompleted += TcpServer_OnMessageReceiveCompleted;
				_clients[key].OnMessageSentEvent += TcpServer_OnMessageSentEvent;

				//Log.WriteLine(LogLevel.Information, "Client {0} Connected.", client.RemoteEndPoint);
			}
			catch
			{
				LogManager.Critical("Failure Accepting connections", this);
			}
			finally
			{
				try
				{
					server.BeginAccept(new AsyncCallback(AcceptConnection), server);
				}
				catch (ObjectDisposedException)
				{
					//is shutting down!!
				}
			}
		}

		void TcpServer_OnMessageReceiveCompleted(TcpClient stream, byte[] data)
		{
			if (OnClientMessageReceiveCompleted != null)
			{
				OnClientMessageReceiveCompleted(stream, data);
			}
		}

		void TcpServer_OnMessageSentEvent(TcpClient client, int bytesSent)
		{
			if (OnClientMessageSentEvent != null)
			{
				OnClientMessageSentEvent(client, bytesSent);
			}
		}

		void TcpServer_OnMessageReceivedEvent(TcpClient client, byte[] data, int bytesReceived)
		{
			if (OnClientMessageReceivedEvent != null)
			{
				OnClientMessageReceivedEvent(client, data, bytesReceived);
			}
		}

		void TcpServer_OnDisconnectedEvent(TcpClient client)
		{
			if (OnClientDisconnectEvent != null)
			{
				OnClientDisconnectEvent(client);
			}
		}

		void TcpServer_OnConnectedEvent(TcpClient client)
		{
			if (OnClientConnectedEvent != null)
			{
				OnClientConnectedEvent(client);
			}
		}
	}
}
