using Netc.Tcp;
using Netc.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netc.Sock
{
	public class SocketClient
	{

		Dictionary<string, List<Action<object[]>>> _actions;
		TcpClient _client;
		public SocketClient()
		{
			_actions = new Dictionary<string, List<Action<object[]>>>();
			_client = new TcpClient();
			_client.OnMessageReceiveCompleted += _client_OnMessageReceiveCompleted;
		}

		void _client_OnMessageReceiveCompleted(TcpClient client, byte[] data)
		{
			var obj = (SocketMessage)Bytes.ByteArrayToObject(data);
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
		public void On(string message, Action<object[]> callback)
		{
			if (!_actions.Keys.Contains(message))
			{
				_actions.Add(message, new List<Action<object[]>>());
			}
			_actions[message].Add(callback);
		}
		public void Emit<T>(string messageName, params T[] messageContents)
		{
			SocketMessage sm = new SocketMessage();
			sm.Message = messageName;
			sm.Contents = messageContents.Cast<object>().ToArray();

			var data = Bytes.ObjectToByteArray(sm);
			_client.Send(data);
		}
	}
}
