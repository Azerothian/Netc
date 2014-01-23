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



		static void Main(string[] args)
		{
			LogManager.OnLog += LogManager_OnLog;
			var _client = new SocketClient<object>();
			_client.On("response", OnResponse);
			_client.Connect("127.0.0.1", 6112);

			_client.Emit("response", "WEEE");
			Console.ReadLine();
			_client.Disconnect();
		}
		static void OnResponse(object[] message)
		{
			LogManager.Info("OnResponse : {0}",  message[0]);

		}

		static void LogManager_OnLog(LogManager.LogType type, string message, params object[] objects)
		{
			Console.WriteLine("[" + type.ToString() + "] " + message, objects);
		}
	}

}
