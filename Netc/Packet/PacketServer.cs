using Netc.Packets;
using Netc.Tcp;
using Netc.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netc
{
	public class PacketServer
	{
		public event GenericVoidDelegate<PacketClient, byte[]> OnClientMessageReceiveCompleted;
		public event GenericVoidDelegate<PacketClient> OnClientConnectedEvent;
		public event GenericVoidDelegate<PacketClient> OnClientDisconnectEvent;

		private Dictionary<TcpClient, PacketClient> _clients;

		TcpServer _server;
		public PacketServer()
		{
			_server = new TcpServer();
			_clients = new Dictionary<TcpClient, PacketClient>();
			_server.OnClientConnectedEvent += _server_OnClientConnectedEvent;
			_server.OnClientDisconnectEvent += _server_OnClientDisconnectEvent;
		}

		void _server_OnClientDisconnectEvent(TcpClient client)
		{
			PacketClient _event = null;
			lock (_clients)
			{
				_event = _clients[client];
				_clients.Remove(client);
			}
			if (OnClientDisconnectEvent != null)
			{
				OnClientDisconnectEvent(_event);
			}
		}

		void _server_OnClientConnectedEvent(TcpClient client)
		{
			PacketClient _event = null;
			lock (_clients)
			{
				if (!_clients.ContainsKey(client))
				{
					_event = new PacketClient(client);
					_event.OnMessageReceiveCompleted += _clients_OnMessageReceiveCompleted;
					_clients.Add(client, _event);
				}
			}
			if (OnClientConnectedEvent != null)
				OnClientConnectedEvent(_event);

		}

		void _clients_OnMessageReceiveCompleted(PacketClient client, byte[] data)
		{
			if (OnClientMessageReceiveCompleted != null)
			{
				OnClientMessageReceiveCompleted(client, data);
			}
		}

		public void Disconnect()
		{
			_server.Disconnect();
		}

		public void Send(Guid c, byte[] data)
		{
			lock (_clients)
			{
				var target = (from v in _clients.Values where v.Key == c select v).FirstOrDefault();
				if (target != null)
				{
					target.Send(data);
				}
			}

		}
		public void Send(Guid[] ClientsKeys, byte[] data)
		{
			lock (_clients)
			{
				foreach (var c in ClientsKeys)
				{
					var target = (from v in _clients.Values where v.Key == c select v).FirstOrDefault();
					if (target != null)
					{
						target.Send(data);
					}
				}
			}
		}
		public void Send(byte[] data)
		{
			foreach (var c in _clients.Values)
			{
				c.Send(data);
			}
		}

		public void StartListening(int Port)
		{
			_server.StartListening(Port);
		}
	}
}
