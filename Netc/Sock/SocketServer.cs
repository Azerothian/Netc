using Netc.Tcp;
using Netc.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netc.Sock
{
	public class SocketServer
	{
		Dictionary<TcpClient, Guid> _clientKeys;
		Dictionary<string, List<Action<Guid, object[]>>> _actions;

		TcpServer _server;
		public SocketServer()
		{
			_clientKeys = new Dictionary<TcpClient, Guid>();
			_actions = new Dictionary<string, List<Action<Guid, object[]>>>();
			_server = new TcpServer();
			_server.OnClientConnectedEvent += _server_OnClientConnectedEvent;
			_server.OnClientDisconnectEvent += _server_OnClientDisconnectEvent;
			_server.OnClientMessageReceiveCompleted += _server_OnClientMessageReceiveCompleted;
		}

		void _server_OnClientMessageReceiveCompleted(TcpClient client, byte[] data)
		{
			var socketClient = _clientKeys[client];
			var obj = (SocketMessage)Bytes.ByteArrayToObject(data);
			if (obj != null)
			{
				if (_actions.Keys.Contains(obj.Message))
				{
					foreach (var act in _actions[obj.Message])
					{
						act(socketClient, obj.Contents);
					}
				}
			}
			else
			{
				LogManager.Critical("Server - Invalid Data Recieved");

			}
		}

		void _server_OnClientDisconnectEvent(TcpClient client)
		{
			var socketClient = _clientKeys[client];
			if (_actions.Keys.Contains("disconnect"))
			{
				foreach (var act in _actions["disconnect"])
				{
					act(socketClient, null);
				}
			}
		}

		void _server_OnClientConnectedEvent(TcpClient client)
		{
			var socketClient = Guid.NewGuid();
			_clientKeys.Add(client, socketClient);
			if (_actions.Keys.Contains("connect"))
			{
				foreach (var act in _actions["connect"])
				{
					act(socketClient, null);
				}
			}
		}
		public void StartListening(int port)
		{
			_server.StartListening(port);

		}
		public void On(string message, Action<Guid, object[]> callback)
		{
			if (!_actions.Keys.Contains(message))
			{
				_actions.Add(message, new List<Action<Guid, object[]>>());
			}
			_actions[message].Add(callback);
		}

		public void Emit(Guid clientId, string messageName, params object[] messageContents)
		{
			var client = GetClientsById(clientId).FirstOrDefault();
			//var client = (from v in _clientKeys where v.Value == clientId select v.Key).FirstOrDefault();
			if(client == null)
			{
				throw new ArgumentException("Client id your attempting to emit to is invalid");
			}


			SocketMessage sm = new SocketMessage();
			sm.Message = messageName;
			sm.Contents = messageContents;

			var data = Bytes.ObjectToByteArray(sm);
			client.Send(data);
		}

		public IEnumerable<TcpClient> GetClientsById(params Guid[] arr)
		{
			foreach(var v in _clientKeys)
			{
				if(arr.Contains(v.Value))
				{
					yield return v.Key;
				}
			}

		}

		public void Emit(Guid[] clientIds, string messageName, params object[] messageContents)
		{

			var clients = GetClientsById(clientIds);
			if (clients.Count() == 0)
			{
				throw new ArgumentException("Client ids your attempting to emit to is invalid");
			}
			SocketMessage sm = new SocketMessage();
			sm.Message = messageName;
			sm.Contents = messageContents;

			var data = Bytes.ObjectToByteArray(sm);
			foreach(var c in clients)
			{
				c.Send(data);
			}
		}
		public void Emit(string messageName, params object[] messageContents)
		{
			SocketMessage sm = new SocketMessage();
			sm.Message = messageName;
			sm.Contents = messageContents;
			var data = Bytes.ObjectToByteArray(sm);
			var clients = _clientKeys.Keys;
			foreach (var c in clients)
			{
				c.Send(data);
			}
		}
	}
}
