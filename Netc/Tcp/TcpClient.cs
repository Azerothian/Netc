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
	public class TcpClient
	{
		private byte[] eof = BitConverter.GetBytes(-1);


		private const int receiveBufferSize = 512;
		private Socket _socket;
		private bool _isSending = false;
		Stack<byte[]> _queue = new Stack<byte[]>();
		byte[] _current = null;
		int _currentSent = 0;
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


		#region Events
		/// <summary>
		/// Occurs when [on connected event].
		/// </summary>
		public event GenericVoidDelegate<TcpClient> OnConnectedEvent;

		/// <summary>
		/// Occurs when [on message received event].
		/// </summary>
		public event GenericVoidDelegate<TcpClient, byte[], int> OnMessageReceivedEvent;

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
			if (OnDisconnectingEvent != null)
			{
				OnDisconnectingEvent(this);
			}
			_socket.Shutdown(SocketShutdown.Both);
			_socket.Close();
			if (OnDisconnectedEvent != null)
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
			_socket.BeginConnect(ipep,
				new AsyncCallback(EndConnect), _socket);
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
			lock (_queue)
			{
				_queue.Push(data);
			}
			if (!_isSending)
			{
				BeginSend();
			}

		}

		private void BeginSend()
		{
			lock (_queue)
			{
				if(_queue.Count() > 0)
				{
					_current = _queue.Pop();
				} else {
					_current = null;
				}
			}
			if (!_isSending && _current != null)
				{
					
					_currentSent = 0;
					_isSending = true;
					// Convert the string data to byte data using ASCII encoding.
					// Begin sending the data to the remote device.
					if (!_socket.Connected) //puts in a delay.. though really should verify if in connecting state...
					{
						do
						{
							Thread.Sleep(10);
						} while (!_socket.Connected);
					}

					var array = new byte[_current.Length + eof.Length];
					Buffer.BlockCopy(_current, 0, array, 0, _current.Length);
					Buffer.BlockCopy(eof, 0, array, _current.Length, eof.Length);
					_socket.BeginSend(array, 0, array.Length, 0, EndSend, _socket);
				}
			
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

				_currentSent += bytesSent;

				if(_currentSent == _current.Length + eof.Length)
				{
					if (OnMessageSentEvent != null)
					{
						OnMessageSentEvent(this, _currentSent);
					}
					_isSending = false;
					if (_queue.Count > 0)
					{
						//Thread.Sleep(100);//let the buffer clear?
						BeginSend();
					}
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
			try
			{
				// Retrieve the state object and the client socket 
				// from the asynchronous state object.
				StateObject state = (StateObject)ar.AsyncState;
				Socket client = state.ServerSocket;

				// Read data from the remote device.
				int bytesRead = -1;
				bytesRead = client.EndReceive(ar);
				//LogManager.Info("EndReceive {0}", bytesRead);


				state.Memory.Write(state.Buffer, 0, bytesRead);

				// Get the rest of the data.
				if (OnMessageReceivedEvent != null)
				{
					//Fire Recieve Event Here
					byte[] data = new byte[bytesRead];
					Buffer.BlockCopy(state.Buffer, 0, data, 0, bytesRead);
					OnMessageReceivedEvent(this, data, bytesRead);
				}
				//	if(bytesRead < receiveBufferSize)
				//	{
				//var eofCheck = state.Buffer.Skip(bytesRead - eof.Length).Ta;

				var eofCheck = new byte[eof.Length];
				Buffer.BlockCopy(state.Buffer, bytesRead - eof.Length, eofCheck, 0, eof.Length);
				if (eofCheck.SequenceEqual(eof))
				{
					if (OnMessageReceiveCompleted != null)
					{
						//TODO can i d
						var resultArr = state.Memory.GetBuffer();
						var resultData = new byte[(int)state.Memory.Length - eof.Length];
						state.Memory.Clear();
						Buffer.BlockCopy(resultArr, 0, resultData, 0, resultData.Length);
						OnMessageReceiveCompleted(this, resultData);
					}
					//	}

				}

				client.BeginReceive(state.Buffer, 0, receiveBufferSize, 0, EndReceive, state);

			}
			catch (Exception e)
			{

				LogManager.Critical(e.ToString());
			}
		}
		#endregion
		private class StateObject
		{
			public MemoryManager Memory;
			public StateObject(int bufferSize)
			{
				Buffer = new byte[bufferSize];
				Memory = new MemoryManager();

			}
			public Socket ServerSocket;
			public byte[] Buffer;
		}


	}
}
