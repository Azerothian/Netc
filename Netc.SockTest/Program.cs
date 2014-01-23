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

		static SocketServer<string> _server;

		static List<ClientTester> _clients;

		static void Main(string[] args)
		{
			LogManager.OnLog += LogManager_OnLog;
			_server = new SocketServer<string>();
			_server.StartListening(6667);

			_server.On("connect", OnServerConnect);
			_server.On("login", OnServerLogin);
      
			_clients = new List<ClientTester>();
			for (int i = 0; i < 50; i++)
			{
        _clients.Add(new ClientTester());
			}

			Console.ReadLine();
			var _client = new SocketClient<string>();
			//_client.On("loginConfirm", OnClientLoginConfirm);
			_client.Connect("127.0.0.1", 6667);
			var data = new[] { DateTime.Now };
			_client.Emit("login", data);
			Console.ReadLine();
			_client.Disconnect();
			foreach(var c in _clients)
			{
				c.Disconnect();

			}
			_server.Shutdown();
			Console.ReadLine();
		}
		static void OnServerConnect(Guid clientId, object[] message)
		{
			LogManager.Info("OnServerConnect : {0} - {1}", clientId, message);

		}
		static void OnServerLogin(Guid clientId, object[] message)
		{
     // var dt = (DateTime)message[0];
     // var ts = DateTime.Now - dt;
      //LogManager.Info("OnClientLoginConfirm : {0}", message);
			LogManager.Info("OnServerLogin : {0}", clientId);
			_server.Emit("loginConfirm", clientId.ToString());
		}

		

		static void LogManager_OnLog(LogManager.LogType type, string message, params object[] objects)
		{
			Console.WriteLine("[" + type.ToString() + "] " + message, objects);
		}
	}
  public class ClientTester
  {
    SocketClient<string> _client;
    public ClientTester()
    {
      _client = new SocketClient<string>();
      _client.On("loginConfirm", OnClientLoginConfirm);
      _client.Connect("127.0.0.1", 6667);
      //var data =  new[] { DateTime.Now };
     // _client.Emit("login", data);
    }

    void OnClientLoginConfirm(string[] message)
    {
      var clientId = message[0];

			LogManager.Info("OnClientLoginConfirm : {0}", clientId);
			_client.Disconnect();
    }


		internal void Disconnect()
		{
			_client.Disconnect();
		}
	}
	
}
