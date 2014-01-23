using Netc.Tcp;
using Netc.Util;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Netc.Sock
{
	public class SocketServer<T>
	{

		Dictionary<TcpClient, Guid> _clientKeys;
		Dictionary<string, List<Action<Guid, T[]>>> _actions;

		TcpServer _server;
		public SocketServer()
		{
			_clientKeys = new Dictionary<TcpClient, Guid>();
			_actions = new Dictionary<string, List<Action<Guid, T[]>>>();
			_server = new TcpServer();
			_server.OnClientConnectedEvent += _server_OnClientConnectedEvent;
			_server.OnClientDisconnectEvent += _server_OnClientDisconnectEvent;
			_server.OnClientMessageReceiveCompleted += _server_OnClientMessageReceiveCompleted;
		}

		void _server_OnClientMessageReceiveCompleted(TcpClient client, byte[] data)
		{
			var socketClient = _clientKeys[client];

			var obj = Serial.Deserialise<SocketMessage<T>>(data);
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
			if (_clientKeys.ContainsKey(client))
			{
				var socketClient = _clientKeys[client];
				if (_actions.Keys.Contains("disconnect"))
				{
					foreach (var act in _actions["disconnect"])
					{
						act(socketClient, null);
					}
				}
				_clientKeys.Remove(client);
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
		public void On(string message, Action<Guid, T[]> callback)
		{
			if (!_actions.Keys.Contains(message))
			{
				_actions.Add(message, new List<Action<Guid, T[]>>());
			}
			_actions[message].Add(callback);
		}

		public void Emit(Guid clientId, string messageName, params T[] messageContents)
		{
			var client = GetClientsById(clientId).FirstOrDefault();
			if (client == null)
			{
				throw new ArgumentException("Client id your attempting to emit to is invalid");
			}
			SocketMessage<T> sm = new SocketMessage<T>();
			sm.Message = messageName;
			sm.Contents = messageContents;

			var data = Serial.Serialise(sm);
			client.Send(data);
		}

		public IEnumerable<TcpClient> GetClientsById(params Guid[] arr)
		{
			foreach (var v in _clientKeys)
			{
				if (arr.Contains(v.Value))
				{
					yield return v.Key;
				}
			}

		}

		public void Emit(Guid[] clientIds, string messageName, params T[] messageContents)
		{
			foreach (var clientId in clientIds)
			{
				Emit(clientId, messageName, messageContents);
			}
		}
		public void Emit(string messageName, params T[] messageContents)
		{

			foreach (var v in _clientKeys.Values)
			{
				Emit(v, messageName, messageContents);
			}
		}

		public void Shutdown()
		{
			_server.Disconnect();
		}
	}
}
