using Netc.Servers;
using Netc.Streams;
using Netc.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netc.TestClient
{
	class Program
	{
		static Guid _clientId = Guid.Empty;

		static void Main(string[] args)
		{
			LogManager.OnLog += LogManager_OnLog;
			var _server = new Server<TcpServer, TcpStream>();
			_server.OnDataReceivedEvent += _server_OnDataReceivedEvent;
			_server.OnClientConnectedEvent += _server_OnClientConnectedEvent;
			_server.StartListening(6222);

			var _client = new Client<TcpStream>();
			_client.OnDataReceivedEvent += _client_OnDataReceivedEvent;
			_client.OnClientConnectedEvent += _client_OnClientConnectedEvent;
			_client.Connect("127.0.0.1", 6222);

			do
			{
				var message = System.Console.ReadLine();
				if (message == "exit")
					break;
				if (message == "c_send")
				{
					IEnumerable<int> _ii = Enumerable.Range(1, 1000).ToArray();
					var data = Bytes.ObjectToByteArray(_ii);
					_client.Send(data);

				}
				if (message == "s_send")
				{
					IEnumerable<int> _ii = Enumerable.Range(1, 1000).ToArray();
					var data = Bytes.ObjectToByteArray(_ii);
					_server.Send(_clientId, data);
				}
				if (message == "c_report")
				{
					_client.Report();
				}
				if (message == "s_report")
				{
					_server.Report();
				}

			} while (true);

			_server.Disconnect();
			_client.Disconnect();

		}

		static void _client_OnClientConnectedEvent()
		{
			LogManager.Info("[Client] Connected To Server");
		}

		static void _server_OnClientConnectedEvent(Guid clientId)
		{
			LogManager.Info("[Server] Client Connected: {0}", clientId);
			_clientId = clientId;
		}

		static void LogManager_OnLog(LogManager.LogType type, string message, params object[] objects)
		{
			Console.WriteLine("[" + type.ToString() + "] " + message, objects);
		}

		static void _client_OnDataReceivedEvent(byte[] data)
		{
			LogManager.Info(String.Format("Client Received - {0}", data.Length));
		}

		static void _server_OnDataReceivedEvent(Guid clientId, byte[] data)
		{
			LogManager.Info(String.Format("Server Received - {0}", data.Length));
		}
	}
}
