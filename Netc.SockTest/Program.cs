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

		static SocketServer _server;

		static List<ClientTester> _clients;

		static void Main(string[] args)
		{
			LogManager.OnLog += LogManager_OnLog;
			_server = new SocketServer();
			_server.StartListening(6667);

			_clients = new List<ClientTester>();
			_server.On("connect", OnServerConnect);
			_server.On("login", OnServerLogin);
      _clients = new List<ClientTester>();
			

			for (int i = 0; i < 1; i++)
			{
        _clients.Add(new ClientTester());
			}

		}
		static void OnServerConnect(Guid clientId, object[] message)
		{
			LogManager.Info("OnServerConnect : {0} - {1}", clientId, message);

		}
		static void OnServerLogin(Guid clientId, object[] message)
		{
      var dt = (DateTime)message[0];
      var ts = DateTime.Now - dt;
      //LogManager.Info("OnClientLoginConfirm : {0}", message);
      LogManager.Info("OnServerLogin : {0}", ts.TotalMilliseconds);

			_server.Emit(clientId, "loginConfirm", message);
		}

		

		static void LogManager_OnLog(LogManager.LogType type, string message, params object[] objects)
		{
			Console.WriteLine("[" + type.ToString() + "] " + message, objects);
		}
	}
  public class ClientTester
  {
    SocketClient _client;
    public ClientTester()
    {
      _client = new SocketClient();
      _client.On("loginConfirm", OnClientLoginConfirm);
      _client.Connect("127.0.0.1", 6667);
      var data =  new[] { DateTime.Now };
      _client.Emit("login", data);
    }

    void OnClientLoginConfirm(object[] message)
    {
      var dt = (DateTime)message[0];
      var ts = DateTime.Now - dt;
      //LogManager.Info("OnClientLoginConfirm : {0}", message);
      LogManager.Info("OnClientLoginConfirm : {0}", ts.TotalMilliseconds);
      var data = new[] { DateTime.Now };
      _client.Emit("login", data);
    }

  }
	
}
