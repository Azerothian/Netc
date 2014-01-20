using Netc.Servers;
using Netc.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netc.Util.Socks
{
	public class SocketServer
	{
		private Server<TcpServer, TcpStream> _server;
		Dictionary<string, List<Action<Guid, object[]>>> _actions;

		public Guid[] Clients { 
			get {
				return _server.Clients.Keys.ToArray();
			}
		}

		public SocketServer(int port)
		{
			_server = new Server<TcpServer, TcpStream>();
			_actions = new Dictionary<string, List<Action<Guid, object[]>>>();
			_server.OnDataReceivedEvent +=_server_OnDataReceivedEvent;
			_server.OnClientConnectedEvent += _server_OnClientConnectedEvent;
			_server.OnClientDisconnectEvent += _server_OnClientDisconnectEvent;
			_server.StartListening(port);
		}
		void _server_OnClientDisconnectEvent(Guid clientId)
		{
			if (_actions.Keys.Contains("disconnect"))
			{
				foreach (var act in _actions["disconnect"])
				{
					act(clientId, null);
				}
			}
		}
		void _server_OnClientConnectedEvent(Guid clientId)
		{
			if (_actions.Keys.Contains("connect"))
			{
				foreach (var act in _actions["connect"])
				{
					act(clientId, null);
				}
			}
		}
		void _server_OnDataReceivedEvent(Guid clientId, byte[] data)
		{
			var obj = (SocketMessage)Bytes.ByteArrayToObject(data);
			if (_actions.Keys.Contains(obj.MessageName))
			{
				foreach (var act in _actions[obj.MessageName])
				{
					act(clientId, obj.MessageContents);
				}
			}
		}

		public void On(string message, Action<Guid, object[]> callback)
		{
			if (!_actions.Keys.Contains(message))
			{
				_actions.Add(message, new List<Action<Guid, object[]>>());
			}
			_actions[message].Add(callback);
		}

    public void Emit(Guid client, string messageName, params object[] messageContents)
		{
			SocketMessage sm = new SocketMessage();
			sm.MessageName = messageName;
			sm.MessageContents = messageContents;

			var data = Bytes.ObjectToByteArray(sm);
			_server.Send(client, data);
		}
		public void Emit(Guid[] clients, string messageName, params object[] messageContents)
		{
			SocketMessage sm = new SocketMessage();
			sm.MessageName = messageName;
			sm.MessageContents = messageContents;

			var data = Bytes.ObjectToByteArray(sm);
			_server.Send(clients, data);
		}
    public void Emit(string messageName, params object[] messageContents)
		{
			SocketMessage sm = new SocketMessage();
			sm.MessageName = messageName;
			sm.MessageContents = messageContents;

			var data = Bytes.ObjectToByteArray(sm);
			_server.Send(data);
		}


	}
}
