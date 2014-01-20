using Netc.Util;
using Netc.Util.Socks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netc.TestSockClient
{
	class Program
	{
		static SocketServer _server;
		static void Main(string[] args)
		{
			LogManager.OnLog += LogManager_OnLog;
			_server = new SocketServer(6667);
			_server.On("connect", OnServerConnect);
			_server.On("login", OnServerLogin);

			SocketClient _client = new SocketClient("127.0.0.1", 6667);
			_client.On("loginConfirm", OnClientLoginConfirm);

			for (int i = 0; i < 1000; i++)
			{
				_client.Emit("login", DateTime.Now);
			}
		}

		static void OnServerConnect(Guid clientId, object message)
		{
			//LogManager.Info("OnServerConnect : {0} - {1}", clientId, message);

		}
		static void OnServerLogin(Guid clientId, object message)
		{
			//LogManager.Info("OnServerLogin : {0} - {1}", clientId, message);
			_server.Emit(clientId, "loginConfirm", message);
		}

		static void OnClientLoginConfirm(object message)
		{
			var dt = (DateTime)message;
			var ts = DateTime.Now - dt;

			LogManager.Info("OnClientLoginConfirm : {0}", ts.TotalMilliseconds);
		}

		static void LogManager_OnLog(LogManager.LogType type, string message, params object[] objects)
		{
			Console.WriteLine("[" + type.ToString() + "] " + message, objects);
		}
	}
}
