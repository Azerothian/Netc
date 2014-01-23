using Netc.Tcp;
using Netc.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Netc.TcpTest
{
	class Program
	{
		const string IP = "127.0.0.1";
		const int PORT = 6662;

		static TcpServer _server;
		//static List<TcpStream> _clients;
		static byte[] obj;
		static byte[] crc;


		static void Main(string[] args)
		{
			LogManager.OnLog += LogManager_OnLog;
			GenerateObject();

			InitServer();

			for(var i = 0; i < 5; i++)
			{
				InitClient();

			}
			Console.ReadLine();

		}

		#region Client

		static void InitClient()
		{
			LogManager.Info("[CLIENT] Init Client");
			TcpClient client = new TcpClient();
			client.OnConnectedEvent += client_OnConnectedEvent;
			client.OnMessageReceiveCompleted += client_OnMessageReceiveCompleted;
			client.Connect(IP, PORT);
		}

		static void client_OnMessageReceiveCompleted(TcpClient stream, byte[] data)
		{
			if (CompareObject(data))
			{
				LogManager.Info("[CLIENT] SUCCESS COMPARE");
				Thread.Sleep(10);
				stream.Send(obj);
				//stream.Send(obj);
			}
			else
			{
				LogManager.Info("[CLIENT] FAILED COMPARE");
			}


		}

		static void client_OnConnectedEvent(TcpClient stream)
		{
			LogManager.Info("[CLIENT] Connected to Server - Sending Object");
			stream.Send(obj);
			//stream.Send(obj);
		}

		#endregion


		#region Server
		static void InitServer()
		{			
			
			_server = new TcpServer();
			_server.OnClientConnectedEvent += _server_OnClientConnectedEvent;
			_server.OnClientMessageReceiveCompleted += _server_OnClientMessageReceiveCompleted;
			_server.StartListening(PORT);


		}

		static void _server_OnClientMessageReceiveCompleted(TcpClient stream, byte[] data)
		{
			if(CompareObject(data))
			{
				LogManager.Info("[SERVER] SUCCESS COMPARE");
				stream.Send(obj);
			}
			else
			{
				LogManager.Info("[SERVER] FAILED COMPARE");
			}


		}

		static void _server_OnClientConnectedEvent(TcpClient stream)
		{
			LogManager.Info("[SERVER] A client connected to Server");
		}
		#endregion

		static void GenerateObject()
		{
			IEnumerable<int> _ii = Enumerable.Range(1, 10).ToArray();
			obj = Bytes.ObjectToByteArray(_ii);
			crc = Util.CRC.CalculateCRC(obj);
		}

		static bool CompareObject(byte[] data)
		{
			LogManager.Info("array size {0}", data.Length);
			var newCrc = Util.CRC.CalculateCRC(data);
			return newCrc.SequenceEqual(crc);

		}
		static void LogManager_OnLog(LogManager.LogType type, string message, params object[] objects)
		{
			Console.WriteLine("[" + type.ToString() + "] " + message, objects);
		}
	}
}
