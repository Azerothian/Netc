using Netc.Abstracts;
using Netc.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Netc
{
	public class PacketClient<T>
		where T : INetworkAbstract, new()
	{
		public delegate void OnDataReceivedDelegate(byte[] data);
		public event OnDataReceivedDelegate OnDataReceivedEvent;

		public delegate void OnClientConnectedDelegate();
		public event OnClientConnectedDelegate OnClientConnectedEvent;

		public delegate void OnClientDisconnectedDelegate();
		public event OnClientDisconnectedDelegate OnClientDisconnectedEvent;


		PacketManager<T> _data;
		T _client;

		public PacketClient()
		{
			_client = new T();
			_data = new PacketManager<T>(_client);
			_data.OnDataReceivedEvent += _data_DataReceivedEvent;
			_client.OnConnectedEvent += client_OnConnectedEvent;
			_client.OnDisconnectedEvent += _client_OnDisconnectedEvent;
		}

		void _client_OnDisconnectedEvent(INetworkAbstract client)
		{
			if (OnClientDisconnectedEvent != null)
			{
				OnClientDisconnectedEvent();
			}
		}


		void client_OnConnectedEvent(INetworkAbstract c)
		{
			//LogManager.Info(String.Format("[Client] Connected to server {0}", c.Key));
			if (OnClientConnectedEvent != null)
			{
				OnClientConnectedEvent();
			}
		}

		void _data_DataReceivedEvent(T c, byte[] data)
		{
			//LogManager.Info(String.Format("[Client] Data Received {0}", data.Length));
			if (OnDataReceivedEvent != null)
				OnDataReceivedEvent(data);
		}

		public void Report()
		{
			_data.Report();
		}

		public void Connect(string ip, int port)
		{
			_client.Connect(ip, port);
		}

		public void Disconnect()
		{
			_client.Disconnect();
		}

		public void Send(byte[] data)
		{
			_data.Send(data);
		}
	}
}
