using Netc.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Netc.PacketTest
{
	class Program
	{
		const string IP = "127.0.0.1";
		const int PORT = 6662;
		static PacketServer _server;

		static byte[] obj;
		static byte[] crc;



		static void Main(string[] args)
		{
			LogManager.OnLog += LogManager_OnLog;
			GenerateObject();

			InitServer();

			for (var i = 0; i < 1; i++)
			{
				InitClient();

			}
			Console.ReadLine();

		}



		#region Client

		static void InitClient()
		{
			LogManager.Info("[CLIENT] Init Client");
			PacketClient client = new PacketClient();
			client.OnConnectedEvent += client_OnConnectedEvent;
			client.OnMessageReceiveCompleted += client_OnMessageReceiveCompleted;
			client.Connect(IP, PORT);
		}

		static void client_OnMessageReceiveCompleted(PacketClient client, byte[] data)
		{
			if (CompareObject(data))
			{
				LogManager.Info("[CLIENT] SUCCESS COMPARE");
				client.Send(obj);

			}
			else
			{
				LogManager.Info("[CLIENT] FAILED COMPARE");
			}


		}

		static void client_OnConnectedEvent(PacketClient client)
		{
			LogManager.Info("[CLIENT] Connected to Server - Sending Object");
			client.Send(obj);

		}

		#endregion


		#region Server
		static void InitServer()
		{

			_server = new PacketServer();
			_server.OnClientConnectedEvent += _server_OnClientConnectedEvent;
			_server.OnClientMessageReceiveCompleted += _server_OnClientMessageReceiveCompleted;
			_server.StartListening(PORT);


		}

		static void _server_OnClientMessageReceiveCompleted(PacketClient client, byte[] data)
		{
			if (CompareObject(data))
			{
				LogManager.Info("[SERVER] SUCCESS COMPARE");
				client.Send(obj);
				//Thread.Sleep(10);
				//client.Send(obj);
			}
			else
			{
				LogManager.Info("[SERVER] FAILED COMPARE");
			}


		}

		static void _server_OnClientConnectedEvent(PacketClient stream)
		{
			LogManager.Info("[SERVER] A client connected to Server");
		}
		#endregion

		static void GenerateObject()
		{
			IEnumerable<int> _ii = Enumerable.Range(1, 1000).ToArray();
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
