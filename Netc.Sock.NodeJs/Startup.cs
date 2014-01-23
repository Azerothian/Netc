using Netc.Sock;
using Netc.Util;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Netc.Sock.NodeJs
{
	public class Startup
	{
		public async Task<object> Invoke(object input)
		{
			LogManager.OnLog += LogManager_OnLog;
			return new SocketServerAsync();
			//return new
			//{
			//	StartListening = (Action<int>)(
			//		async (port) =>
			//		{
			//			//socketServer.StartListening(port);
			//			//return await true;
			//		}
			//	)
			//};
		}
		void LogManager_OnLog(LogManager.LogType type, string message, params object[] objects)
		{
			Console.WriteLine("[" + type.ToString() + "] " + message, objects);
		}
	}
	public class SocketServerAsync
	{
    public Func<object, Task<object>> StartListening
    {
      get
      {
        return async (dynamic port) =>
        {
          LogManager.Info("Calling Start Listening");
          await Task.Run(() => { _socketServer.StartListening(port); });
          return Task.FromResult<object>(null);
        };
      }
    }
    public Func<object, Task<object>> On
    {
      get
      {
        return async (dynamic data) =>
        {
          string eventName = data.eventName;
          var action = (Func<object,Task<object>>)data.callback;

          LogManager.Info("On");
          await Task.Run(() =>
          {
            _socketServer.On(eventName, (Guid guid, object[] objs) =>
            {
              action(new
              {
                clientId = guid.ToString(),
                data = objs
              });
            });
          });
          return Task.FromResult<object>(null);
        };
      }
    }
    public Func<object, Task<object>> Emit
    {
      get
      {
        return async (dynamic data) =>
        {
          LogManager.Info("Emit");
          List<Guid> targets = new List<Guid>();
          if(((IDictionary<String, object>)data).ContainsKey("client")) {
            targets.Add(Guid.Parse(data.client));
          }
          if(((IDictionary<String, object>)data).ContainsKey("clients")) {
            foreach(var v in data.clients)
            {
              targets.Add(Guid.Parse(v));
            }
          }
          string eventName = data.eventName;
          await Task.Run(() =>
          {
            if(targets.Count == 0)
            {
              LogManager.Info("Emit to everyone");
              _socketServer.Emit(eventName, data.data);
            } else {
              LogManager.Info("Emit to selected targets {0} {1} {2}",eventName, targets.Count, data.data);
              _socketServer.Emit(targets.ToArray(), eventName, data.data);
            }
          });
          return Task.FromResult<object>(null);
        };
      }
    }

		SocketServer<object> _socketServer;
		public SocketServerAsync()
		{
      _socketServer = new SocketServer<object>();
			//_socketServer.On("response", OnResponse);

		}




    //void OnResponse(Guid clientId, T[] message)
    //{
    //  LogManager.Info("OnResponse :  {0} {1}",clientId, message[0]);
    //  _socketServer.Emit(clientId, "response", message[0]);

    //}

	}
}