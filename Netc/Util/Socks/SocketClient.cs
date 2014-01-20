using Netc.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netc.Util.Socks
{
	public class SocketClient
	{
		Client<TcpStream> _client;
		Dictionary<string, List<Action<object[]>>> _actions;
		public SocketClient(string ip, int port)
		{
			_actions = new Dictionary<string, List<Action<object[]>>>();
			_client = new Client<TcpStream>();
			_client.OnDataReceivedEvent += _client_OnDataReceivedEvent;
			_client.OnClientConnectedEvent += _client_OnClientConnectedEvent;
			_client.Connect(ip, port);
		}

		void _client_OnClientConnectedEvent()
		{
			
		}

		void _client_OnDataReceivedEvent(byte[] data)
		{
			var obj = (SocketMessage)Bytes.ByteArrayToObject(data);
      if (obj != null)
      {
        if (_actions.Keys.Contains(obj.MessageName))
        {
          foreach (var act in _actions[obj.MessageName])
          {
            act(obj.MessageContents);
          }
        }
      }
      else
      {
        LogManager.Critical("Client - Invalid Data Recieved");

      }
		}
		public void On(string message, Action<object[]> callback)
		{
			if (!_actions.Keys.Contains(message))
			{
				_actions.Add(message, new List<Action<object[]>>());
			}
			_actions[message].Add(callback);
		}
		public void Emit<T>(string messageName,  params T[] messageContents)
		{
			SocketMessage sm = new SocketMessage();
			sm.MessageName = messageName;
      sm.MessageContents = messageContents.Cast<object>().ToArray();

			var data = Bytes.ObjectToByteArray(sm);
			_client.Send(data);
		}

	}
}
