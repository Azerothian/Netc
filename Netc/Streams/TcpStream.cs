using Netc.Abstracts;
using Netc.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace Netc.Streams
{
	public class TcpStream : NetworkAbstractStream<TcpStream>
	{
		private const int receiveBufferSize = 512;
		private Socket _socket;
		/// <summary>
		/// 
		/// </summary>
		public string Key = "";


		private void Process()
		{
			
		}

		#region Client Functions
		/// <summary>
		/// Connects the specified ip address.
		/// </summary>
		/// <param name="IpAddress">The ip address.</param>
		/// <param name="Port">The port.</param>
		public override void Connect(string IpAddress, int Port)
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
		protected override void BeginSend(byte[] data)
		{
			// Convert the string data to byte data using ASCII encoding.
			// Begin sending the data to the remote device.
      if (!_socket.Connected) //puts in a delay.. though really should verify if in connecting state...
      {
        do
        {
          Thread.Sleep(10);
        } while (!_socket.Connected);
      }
			_socket.BeginSend(data, 0, data.Length, 0, EndSend, _socket);

		
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
				_socket.BeginReceive(state.Buffer, 0, receiveBufferSize, 0, EndReceive, state);
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
				try
				{
					bytesRead = client.EndReceive(ar);
				}
				catch { }
				if (bytesRead > 0)
				{
          LogManager.Info("EndReceive {0}", bytesRead);
					WriteToIncomingStream(state.Buffer, 0, bytesRead);
          WriteToIncomingStream((byte)99);
					// Get the rest of the data.
					client.BeginReceive(state.Buffer, 0, receiveBufferSize, 0, EndReceive, state);
					if (OnMessageReceivedEvent != null)
					{
						//Fire Recieve Event Here
						OnMessageReceivedEvent(this, bytesRead);
					}
				}
				else
				{
					if (OnDisconnectedEvent != null)
					{
						//Log.WriteLine(LogLevel.Debugging, "End Receive either returned 0 bytes or crashed.");
						OnDisconnectedEvent(this);
					}

				}
			}
			catch (Exception e)
			{

				LogManager.Critical(e.ToString(), e, this);
			}
		}
		#endregion
		/// <summary>
		/// Disconnects this instance.
		/// </summary>
		public override void Disconnect()
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
		private class StateObject
		{
			public StateObject(int bufferSize)
			{
				Buffer = new byte[bufferSize];
			}
			public Socket ServerSocket;
			public byte[] Buffer;
		}

		/// <summary>
		/// Occurs when [on connected event].
		/// </summary>
		public override event OnConnectedDelegate<TcpStream> OnConnectedEvent;

		/// <summary>
		/// Occurs when [on message received event].
		/// </summary>
		public override event OnMessageReceivedDelegate<TcpStream> OnMessageReceivedEvent;

		/// <summary>
		/// Occurs when [on message sent event].
		/// </summary>
		public override event OnMessageSentDelegate<TcpStream> OnMessageSentEvent;

		/// <summary>
		/// Occurs when [on connecting event].
		/// </summary>
		public override event OnConnectingDelegate<TcpStream> OnConnectingEvent;

		/// <summary>
		/// Occurs when [on disconnecting event].
		/// </summary>
		public override event OnDisconnectingDelegate<TcpStream> OnDisconnectingEvent;

		/// <summary>
		/// Occurs when [on disconnected event].
		/// </summary>
		public override event OnDisconnectedDelegate<TcpStream> OnDisconnectedEvent;

		public override void CreateFromSocket(Socket s)
		{
			_socket = s;
			BeginReceive();
		}
	}
}
