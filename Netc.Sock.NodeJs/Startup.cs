using Netc.Sock;
using Netc.Util;
using System;
using System.Threading.Tasks;

namespace Netc.Sock.NodeJs
{
	public class Startup
	{
		public async Task<object> Invoke(object input)
		{
			LogManager.OnLog += LogManager_OnLog;
			return new SocketServerAsync<string>();
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
	public class SocketServerAsync<T>
	{

		public Func<object, Task<object>> StartListening;

		SocketServer<T> _socketServer;
		public SocketServerAsync()
		{
			_socketServer = new SocketServer<T>();
			_socketServer.On("response", OnResponse);

			StartListening = (Func<object, Task<object>>)(async (i) =>
			{
				LogManager.Info("Calling Start Listening");
				var port = (int)i;
				_socketServer.StartListening(port);
				return i;
			});

		}
		void OnResponse(Guid clientId, T[] message)
		{
			LogManager.Info("OnResponse :  {0} {1}",clientId, message[0]);
			_socketServer.Emit(clientId, "response", message[0]);

		}

	}
}