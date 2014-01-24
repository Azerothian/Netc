using Netc.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Netc.Tcp
{
	public class TcpClient : ThreadHelper
	{
		private byte[] endOfFile = new byte[] { 255, 255, 255, 255 };// BitConverter.GetBytes(-1);
		private byte[] startOfFile = new byte[] { 254, 255, 255, 255 };//BitConverter.GetBytes(-2);
		private byte[] endOfHeader = new byte[] { 253, 255, 255, 255 };//BitConverter.GetBytes(-3);

		private bool IsShuttingDown = false;

		private MemoryManager _incomingStream = new MemoryManager();
		private const int receiveBufferSize = 1024;
		private Socket _socket;
		public string Key = "";
		public bool KeepAlive
		{
			get
			{
				return (bool)_socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive);
			}
			set
			{
				_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, value);
			}
		}

		public TcpClient()
		{
			ThreadSleep = 1;
			Start();
			OnDisconnectedEvent +=TcpClient_OnDisconnectedEvent;
		}

		private void TcpClient_OnDisconnectedEvent(TcpClient obj1)
		{
			Stop();
		}
		#region Events
		/// <summary>
		/// Occurs when [on connected event].
		/// </summary>
		public event GenericVoidDelegate<TcpClient> OnConnectedEvent;

		/// <summary>
		/// Occurs when [on message received event].
		/// </summary>
		//public event GenericVoidDelegate<TcpClient, byte[], int> OnMessageReceivedEvent;

		public event GenericVoidDelegate<TcpClient, byte[]> OnMessageReceiveCompleted;

		/// <summary>
		/// Occurs when [on message sent event].
		/// </summary>
		public event GenericVoidDelegate<TcpClient, int> OnMessageSentEvent;

		/// <summary>
		/// Occurs when [on connecting event].
		/// </summary>
		//	public event GenericVoidDelegate<TcpStream> OnConnectingEvent;

		/// <summary>
		/// Occurs when [on disconnecting event].
		/// </summary>
		public event GenericVoidDelegate<TcpClient> OnDisconnectingEvent;

		/// <summary>
		/// Occurs when [on disconnected event].
		/// </summary>
		public event GenericVoidDelegate<TcpClient> OnDisconnectedEvent;
		#endregion
		public void CreateFromSocket(Socket s)
		{
			_socket = s;
			BeginReceive();
		}

		#region Connect Functions
		public void Disconnect()
		{
			IsShuttingDown = true;
			if (_socket.Connected)
			{
				if (OnDisconnectingEvent != null)
				{
					OnDisconnectingEvent(this);
				}
				_socket.Shutdown(SocketShutdown.Both);
				_socket.Close();
			}
			if (OnDisconnectedEvent != null && !DisconnectedEvent)
			{
				OnDisconnectedEvent(this);
			}
		}

		/// <summary>
		/// Connects the specified ip address.
		/// </summary>
		/// <param name="IpAddress">The ip address.</param>
		/// <param name="Port">The port.</param>
		public void Connect(string IpAddress, int Port)
		{
			IPEndPoint ipep = new IPEndPoint(
						IPAddress.Parse(IpAddress), Port);

			_socket = new Socket(AddressFamily.InterNetwork,
							 SocketType.Stream, ProtocolType.Tcp);
			_socket.NoDelay = true;
			_socket.BeginConnect(ipep, EndConnect, _socket);
	
		}
		private void EndConnect(IAsyncResult ar)
		{
			try
			{
				// Retrieve the socket from the state object.
				Socket server = (Socket)ar.AsyncState;

				// Complete the connection.
				server.EndConnect(ar);

				Key = server.RemoteEndPoint.ToString();
				//Log.WriteLine(LogLevel.Information, "Socket connected to {0}", Key);

				// Signal that the connection has been made.
				if (OnConnectedEvent != null)
				{
					OnConnectedEvent(this);
				}
				BeginReceive();
			}
			catch (Exception e)
			{
				LogManager.Critical(e.ToString(), e, this);
			}
		}
		#endregion
		#region Send Functions
		public void Send(byte[] data)
		{
			BeginSend(data);

		}

		private void BeginSend(byte[] data)
		{

			// Convert the string data to byte data using ASCII encoding.
			// Begin sending the data to the remote device.
			if (!_socket.Connected) //TODO: puts in a delay.. though really should verify if in connecting state...
			{
				do
				{
					Thread.Sleep(10);
				} while (!_socket.Connected);
			}
			var sizeOfPacket = BitConverter.GetBytes(data.Length);
			var totalPacketSize = startOfFile.Length + sizeOfPacket.Length + endOfHeader.Length + data.Length + endOfFile.Length;
			var array = new byte[totalPacketSize];

			var index = 0;
			Buffer.BlockCopy(startOfFile, 0, array, 0, startOfFile.Length);
			index += startOfFile.Length;
			Buffer.BlockCopy(sizeOfPacket, 0, array, index, sizeOfPacket.Length);
			index += sizeOfPacket.Length;
			Buffer.BlockCopy(endOfHeader, 0, array, index, endOfHeader.Length);
			index += endOfHeader.Length;
			Buffer.BlockCopy(data, 0, array, index, data.Length);
			index += data.Length;
			Buffer.BlockCopy(endOfFile, 0, array, index, endOfFile.Length);

			_socket.BeginSend(array, 0, array.Length, 0, EndSend, _socket);


		}
		private void EndSend(IAsyncResult ar)
		{
			try
			{
				// Retrieve the socket from the state object.
				Socket client = (Socket)ar.AsyncState;

				// Complete sending the data to the remote device.
				int bytesSent = client.EndSend(ar);
				//Log.WriteLine(LogLevel.Information, "Sent {0} bytes to server.", bytesSent);
				//Fire sent event here
				if (OnMessageSentEvent != null)
				{
					OnMessageSentEvent(this, bytesSent);
				}

			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}
		#endregion
		#region Receive Functions
		private void BeginReceive()
		{
			try
			{
				// Begin receiving the data from the remote device.
				StateObject state = new StateObject(receiveBufferSize);
				state.ServerSocket = _socket;
				SocketError err;
				_socket.BeginReceive(state.Buffer, 0, receiveBufferSize, SocketFlags.None, out err, EndReceive, state);
				//LogManager.Info("SocketError {0}", err);
			}
			catch (Exception e)
			{

				LogManager.Critical(e.ToString(), e, this);

			}
		}
		private void EndReceive(IAsyncResult ar)
		{
		//	if (!IsShuttingDown)
		//	{
				try
				{

					// Retrieve the state object and the client socket 
					// from the asynchronous state object.
					StateObject state = (StateObject)ar.AsyncState;

					if (!state.ServerSocket.Connected)
						return;


					Socket client = state.ServerSocket;

					// Read data from the remote device.
					int bytesRead = -1;
					//	if (client.Connected)
					//	{
					bytesRead = client.EndReceive(ar);

					if (bytesRead > 0)
					{
						//LogManager.Info("EndReceive {0}", bytesRead);

						lock (_incomingStream)
						{
							_incomingStream.Write(state.Buffer, 0, bytesRead);
						}

						client.BeginReceive(state.Buffer, 0, receiveBufferSize, 0, EndReceive, state);
					}
					else
					{
						if (OnDisconnectedEvent != null)
						{
							OnDisconnectedEvent(this);
						}

					}
				}
				catch (SocketException e)
				{
					//LogManager.Critical(e.ToString());
					if (OnDisconnectedEvent != null)
					{
						OnDisconnectedEvent(this);
					}
				}
				catch (ObjectDisposedException e)
				{
					//LogManager.Critical(e.ToString());
					if (OnDisconnectedEvent != null)
					{
						OnDisconnectedEvent(this);
					}
				}

		//	}
		}
		#endregion
		private class StateObject
		{
			//			public MemoryManager Memory;
			public StateObject(int bufferSize)
			{
				Buffer = new byte[bufferSize];
				//Memory = new MemoryManager();

			}
			public Socket ServerSocket;
			public byte[] Buffer;
		}

		bool DisconnectedEvent = false;
		public override void ThreadWorker(TimeSpan timeDiff)
		{
			if (_socket == null)
				return;
			//if (!_socket.Connected && !DisconnectedEvent)
			//{
			//	if (OnDisconnectedEvent != null)
			//	{
			//		OnDisconnectedEvent(this);
			//	}
			//	DisconnectedEvent = true;
			//	//Stop();
			//	return;
			//}
			byte[] packet = null;
			lock (_incomingStream)
			{
				if (_incomingStream.Length > startOfFile.Length + sizeof(int) + endOfHeader.Length) // check if buffer is larger then min header size
				{
					//CancelNextSleep();
					var buffer = _incomingStream.GetBuffer();
					for (var i = 0; i < _incomingStream.Length; i++)
					{
						var startCheck = new byte[startOfFile.Length];
						var index = i;
						if (index + startOfFile.Length + sizeof(int) + endOfHeader.Length > _incomingStream.Length)
							break;
						Buffer.BlockCopy(buffer, index, startCheck, 0, startOfFile.Length);
						if (!startCheck.SequenceEqual(startOfFile)) // start not found.. continue
							continue;
						index += startOfFile.Length;

						int packetSize = BitConverter.ToInt32(buffer, index);
						index += sizeof(int);

						var endOfHeaderCheck = new byte[endOfHeader.Length];
						Buffer.BlockCopy(buffer, index, endOfHeaderCheck, 0, endOfHeader.Length);
						if (!endOfHeader.SequenceEqual(endOfHeaderCheck)) // endOfHeader not found.. continue
							continue;

						index += endOfHeader.Length;

						if (_incomingStream.Length < index + packetSize + endOfFile.Length) // stream not finished yet;
							break;

						var endCheck = new byte[endOfFile.Length];
						Buffer.BlockCopy(buffer, index + packetSize, endCheck, 0, endOfFile.Length);
						if (!endCheck.SequenceEqual(endOfFile))
							continue; // we had false hope


						packet = new byte[packetSize];
						Buffer.BlockCopy(buffer, index, packet, 0, packetSize);
						var totalPacketSize = startOfFile.Length + sizeof(int) + endOfHeader.Length + packetSize + endOfFile.Length;
						_incomingStream.Remove(0, i + totalPacketSize);
						break;


					}
				}
			}
			if (packet != null && OnMessageReceiveCompleted != null)
			{
				OnMessageReceiveCompleted(this, packet);

			}
		}
	}
}
