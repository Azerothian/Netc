using Netc.Tcp;
using Netc.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Netc.Sock
{
	public class SocketClient<T>
	{

		Dictionary<string, List<Action<T[]>>> _actions;
		TcpClient _client;
		public SocketClient()
		{
			_actions = new Dictionary<string, List<Action<T[]>>>();
			_client = new TcpClient();
			_client.OnMessageReceiveCompleted += _client_OnMessageReceiveCompleted;
		}

		void _client_OnMessageReceiveCompleted(TcpClient client, byte[] data)
		{


			var obj = Serial.Deserialise<SocketMessage<T>>(data);// (SocketMessage)Bytes.ByteArrayToObject(data);
			if (obj != null)
			{
				if (_actions.Keys.Contains(obj.Message))
				{
					foreach (var act in _actions[obj.Message])
					{
						act(obj.Contents);
					}
				}
			}
			else
			{
				LogManager.Critical("Client - Invalid Data Recieved");
			}
		}
		public void Connect(string ip, int port)
		{
			_client.Connect(ip, port);
		}
		public void On(string message, Action<T[]> callback)
		{
			if (!_actions.Keys.Contains(message))
			{
				_actions.Add(message, new List<Action<T[]>>());
			}
			_actions[message].Add(callback);
		}
		public void Emit<T>(string messageName, params T[] messageContents)
		{
			SocketMessage<T> sm = new SocketMessage<T>();
			sm.Message = messageName;
			sm.Contents = messageContents;

			var data = Serial.Serialise(sm);
			_client.Send(data);
		}

		public void Disconnect()
		{
			_client.Disconnect();
		}
	}
}
