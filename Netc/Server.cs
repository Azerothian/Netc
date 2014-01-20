using Netc.Abstracts;
using Netc.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netc
{
	public class Server<T, T1> 
		where T : NetworkAbstractServer<T1>, new()
		where T1 : NetworkAbstractStream<T1>
	{

		public delegate void OnDataReceivedDelegate(Guid clientId, byte[] data);
		public event OnDataReceivedDelegate OnDataReceivedEvent;

		public delegate void OnClientConnectedDelegate(Guid clientId);
		public event OnClientConnectedDelegate OnClientConnectedEvent;

		public delegate void OnClientDisconnectDelegate(Guid clientId);
		public event OnClientDisconnectDelegate OnClientDisconnectEvent;

		Dictionary<Guid, T1> _keys;
		Dictionary<T1, PacketManager<T1>> _data;

		T _server;
		public Server()
		{
			_server = new T();
			_keys = new Dictionary<Guid, T1>();
			_data = new Dictionary<T1, PacketManager<T1>>();
			_server.OnClientConnectedEvent += _server_OnClientConnectedEvent;
			_server.OnClientDisconnectEvent += _server_OnClientDisconnectEvent;
		}

		void _server_OnClientDisconnectEvent(T1 client)
		{
			var id = GetStreamId(client);
			lock (_data)
			{
				lock (_keys)
				{
					_keys.Remove(id);
					_data.Remove(client);
				}
			}
			if (OnClientDisconnectEvent != null)
			{
				OnClientDisconnectEvent(id);
			}
		}

		void _server_OnClientConnectedEvent(T1 client)
		{
			if (!_data.ContainsKey(client))
			{
				var pacMan = new PacketManager<T1>(client);
				pacMan.OnDataReceivedEvent += pacMan_DataReceivedEvent;
				_data.Add(client, pacMan);

				var n = Guid.NewGuid();
				_keys.Add(n, client);
				if (OnClientConnectedEvent != null)
					OnClientConnectedEvent(n);

			}
		}
		void pacMan_DataReceivedEvent(T1 c, byte[] data)
		{
			//LogManager.Info(String.Format("[Server] PacMan Recieved Data {0}", data.Length));
			if (OnDataReceivedEvent != null)
				OnDataReceivedEvent(GetStreamId(c), data);
		}
		private Guid GetStreamId(T1 stream)
		{
			var k = _keys.Where(p => p.Value == stream).FirstOrDefault();
			return k.Key;
		}
		public void Disconnect()
		{
			_server.Disconnect();
		}

		public void Send(Guid c, byte[] data)
		{
			_data[_keys[c]].Send(data);
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
