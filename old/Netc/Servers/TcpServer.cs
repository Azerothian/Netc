using Netc.Abstracts;
using Netc.Streams;
using Netc.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Netc.Servers
{
	public class TcpServer : NetworkAbstractServer<TcpStream>
	{
		private Dictionary<string, TcpStream> _clients = new Dictionary<string, TcpStream>();
		private Socket server;

		public override void Disconnect()
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

		public override void StartListening(int Port)
		{
			IPEndPoint ipep = new IPEndPoint(IPAddress.Any, Port);

			server = new Socket(AddressFamily.InterNetwork,
							SocketType.Stream, ProtocolType.Tcp);
			server.NoDelay = true;
			server.Bind(ipep);
			server.Listen(10);
			server.BeginAccept(new AsyncCallback(AcceptConnection), server);
		}

		public override event OnConnectedDelegate<TcpStream> OnClientConnectedEvent;
		public override event OnDisconnectDelegate<TcpStream> OnClientDisconnectEvent;
		public override event OnMessageReceivedDelegate<TcpStream> OnClientMessageReceivedEvent;
		public override event OnMessageSentDelegate<TcpStream> OnClientMessageSentEvent;


		private void AcceptConnection(IAsyncResult iar)
		{
			try
			{
				Socket oldserver = (Socket)iar.AsyncState;
				Socket client = oldserver.EndAccept(iar);
				client.NoDelay = true;
				var stream = new TcpStream();
				stream.CreateFromSocket(client);
				_clients.Add(String.Format("{0}", client.RemoteEndPoint), stream);

				if (OnClientConnectedEvent != null)
				{
					var key = client.RemoteEndPoint.ToString();
					OnClientConnectedEvent(_clients[key]);
					_clients[key].OnConnectedEvent += TcpServer_OnConnectedEvent;
					_clients[key].OnDisconnectedEvent += TcpServer_OnDisconnectedEvent;

					_clients[key].OnMessageReceivedEvent += TcpServer_OnMessageReceivedEvent;
					_clients[key].OnMessageSentEvent += TcpServer_OnMessageSentEvent;
				}
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
				} catch(ObjectDisposedException)
				{
					//is shutting down!!
				}
			}
		}

		void TcpServer_OnMessageSentEvent(TcpStream client, int bytesSent)
		{
			if (OnClientMessageSentEvent != null)
			{
				OnClientMessageSentEvent(client, bytesSent);
			}
		}

		void TcpServer_OnMessageReceivedEvent(TcpStream client, int bytesReceived)
		{
			if (OnClientMessageReceivedEvent != null)
			{
				OnClientMessageReceivedEvent(client, bytesReceived);
			}
		}

		void TcpServer_OnDisconnectedEvent(TcpStream client)
		{
			if (OnClientDisconnectEvent != null)
			{
				OnClientDisconnectEvent(client);
			}
		}

		void TcpServer_OnConnectedEvent(TcpStream client)
		{
			if (OnClientConnectedEvent != null)
			{
				OnClientConnectedEvent(client);
			}
		}


	}
}
