using Netc.Packets;
using Netc.Tcp;
using Netc.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Netc
{
	public class PacketClient
	{
		public event GenericVoidDelegate<PacketClient, byte[]> OnMessageReceiveCompleted;
		public event GenericVoidDelegate<PacketClient> OnConnectedEvent;
		public event GenericVoidDelegate<PacketClient> OnDisconnectedEvent;

		public Guid Key = Guid.NewGuid();
		PacketManager _data;
		TcpClient _client;

		public PacketClient()
		{
			_client = new TcpClient();
			_data = new PacketManager(_client);
			_data.OnDataReceivedEvent += _data_DataReceivedEvent;
			_client.OnConnectedEvent += client_OnConnectedEvent;
			_client.OnDisconnectedEvent += _client_OnDisconnectedEvent;
		}
		public PacketClient(TcpClient client) 
		{
			_client = client;
			_data = new PacketManager(_client);
			_data.OnDataReceivedEvent += _data_DataReceivedEvent;
			_client.OnConnectedEvent += client_OnConnectedEvent;
			_client.OnDisconnectedEvent += _client_OnDisconnectedEvent;
		}

		void _client_OnDisconnectedEvent(TcpClient client)
		{
			if (OnDisconnectedEvent != null)
			{
				OnDisconnectedEvent(this);
			}
		}


		void client_OnConnectedEvent(TcpClient c)
		{
			//LogManager.Info(String.Format("[Client] Connected to server {0}", c.Key));
			if (OnConnectedEvent != null)
			{
				OnConnectedEvent(this);
			}
		}

		void _data_DataReceivedEvent(TcpClient c, byte[] data)
		{
			//LogManager.Info(String.Format("[Client] Data Received {0}", data.Length));
			if (OnMessageReceiveCompleted != null)
				OnMessageReceiveCompleted(this, data);
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
