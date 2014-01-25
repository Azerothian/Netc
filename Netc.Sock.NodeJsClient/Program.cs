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


		static SocketClient<DateTime> _client;
		static void Main(string[] args)
		{
      LogManager.OnLog += LogManager_OnLog;
      Console.WriteLine("Waiting to send... Press the enter key to continue -- type 'quit' to quit..");
      string input = Console.ReadLine();
      if (input == "quit")
        return;
      do
      {

        _client = new SocketClient<DateTime>();
        _client.On("response", OnResponse);
        _client.Connect("127.0.0.1", 6112);

        _client.Emit("response", DateTime.Now);
        
        Console.WriteLine("Waiting to send... Press the enter key to continue -- type 'quit' to quit..");
        input = Console.ReadLine();
        _client.Disconnect();
      } while (input != "quit");
		}
    static void OnResponse(DateTime[] message)
		{

      var span = DateTime.Now - message[0];


      LogManager.Info("OnResponse : {0}", span.TotalMilliseconds);
      _client.Emit("response", DateTime.Now);
		}

		static void LogManager_OnLog(LogManager.LogType type, string message, params object[] objects)
		{
			Console.WriteLine("[" + type.ToString() + "] " + message, objects);
		}
	}

}
