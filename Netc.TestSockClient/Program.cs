﻿using Netc.Util;
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
    static List<ClientTester> _clients;
		static void Main(string[] args)
		{
			LogManager.OnLog += LogManager_OnLog;
			_server = new SocketServer(6667);
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
      _client = new SocketClient("127.0.0.1", 6667);
      _client.On("loginConfirm", OnClientLoginConfirm);
      
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
