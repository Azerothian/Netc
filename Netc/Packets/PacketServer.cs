using Netc.Abstracts;
using Netc.Packets;
using Netc.Servers;
using Netc.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netc
{
	public class PacketServer
	{

		public delegate void OnDataReceivedDelegate(Guid clientId, byte[] data);
		public event OnDataReceivedDelegate OnDataReceivedEvent;

		public delegate void OnClientConnectedDelegate(Guid clientId);
		public event OnClientConnectedDelegate OnClientConnectedEvent;

		public delegate void OnClientDisconnectDelegate(Guid clientId);
		public event OnClientDisconnectDelegate OnClientDisconnectEvent;

		public Dictionary<Guid, TcpStream> Clients;
		Dictionary<TcpStream, PacketManager<TcpStream>> _data;

		TcpServer _server;
		public PacketServer()
		{
			_server = new TcpServer();
			Clients = new Dictionary<Guid, TcpStream>();
			_data = new Dictionary<TcpStream, PacketManager<TcpStream>>();
			_server.OnClientConnectedEvent += _server_OnClientConnectedEvent;
			_server.OnClientDisconnectEvent += _server_OnClientDisconnectEvent;
		}

		void _server_OnClientDisconnectEvent(TcpStream client)
		{
			var id = GetStreamId(client);
			lock (_data)
			{
				lock (Clients)
				{
					Clients.Remove(id);
					_data.Remove(client);
				}
			}
			if (OnClientDisconnectEvent != null)
			{
				OnClientDisconnectEvent(id);
			}
		}

		void _server_OnClientConnectedEvent(TcpStream client)
		{
			if (!_data.ContainsKey(client))
			{
				var pacMan = new PacketManager<TcpStream>(client);
				pacMan.OnDataReceivedEvent += pacMan_DataReceivedEvent;
				_data.Add(client, pacMan);

				var n = Guid.NewGuid();
        lock (Clients)
        {
          Clients.Add(n, client);
        }
				if (OnClientConnectedEvent != null)
					OnClientConnectedEvent(n);

			}
		}
		void pacMan_DataReceivedEvent(TcpStream c, byte[] data)
		{
			//LogManager.Info(String.Format("[Server] PacMan Recieved Data {0}", data.Length));
			if (OnDataReceivedEvent != null)
				OnDataReceivedEvent(GetStreamId(c), data);
		}
		private Guid GetStreamId(TcpStream stream)
		{
			KeyValuePair<Guid, TcpStream> result;
      lock (Clients)
      {
        result = Clients.Where(p => p.Value.Equals(stream)).FirstOrDefault();
      }
      return result.Key;
		}
		public void Disconnect()
		{
			_server.Disconnect();
		}

		public void Send(Guid c, byte[] data)
		{
			_data[Clients[c]].Send(data);
		}
		public void Send(Guid[] ClientsKeys, byte[] data)
		{
			foreach (var c in ClientsKeys)
			{
				if (_data[Clients[c]] != null)
				{
					_data[Clients[c]].Send(data);
				}
			}
		}
		public void Send(byte[] data)
		{
			foreach (var c in Clients.Keys)
			{
				_data[Clients[c]].Send(data);
			}
		}

		public void Report()
		{
			foreach (var v in _data.Keys)
			{
				_data[v].Report();

			}
		}

		public void StartListening(int Port)
		{
			_server.StartListening(Port);

		}
	}
}
