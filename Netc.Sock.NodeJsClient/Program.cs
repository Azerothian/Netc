using Netc.Sock;
using Netc.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netc.SockTest
{
	class Program
	{


		static SocketClient<string> _client;
		static void Main(string[] args)
		{
			LogManager.OnLog += LogManager_OnLog;
			_client = new SocketClient<string>();
			_client.On("response", OnResponse);
			_client.Connect("127.0.0.1", 6112);

			_client.Emit("response", "WEEE");
			Console.ReadLine();
			_client.Disconnect();
		}
		static void OnResponse(string[] message)
		{
			LogManager.Info("OnResponse : {0}",  message[0]);
			_client.Emit("response", "YSYAYS");
		}

		static void LogManager_OnLog(LogManager.LogType type, string message, params object[] objects)
		{
			Console.WriteLine("[" + type.ToString() + "] " + message, objects);
		}
	}

}
