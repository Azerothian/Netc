using Netc.Abstracts;
using Netc.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Netc.Streams
{
	public class UdpStream : NetworkAbstractStream<UdpStream>
	{
		private Socket _socket;
		private IPEndPoint _endPoint;


		public override void Connect(string IpAddress, int Port)
		{
			_endPoint = new IPEndPoint(
						IPAddress.Parse(IpAddress), Port);

			_socket = new Socket(AddressFamily.InterNetwork,
							 SocketType.Dgram, ProtocolType.Udp);
			_socket.NoDelay = true;

		}



		public override event OnConnectingDelegate<UdpStream> OnConnectingEvent;

		public override event OnConnectedDelegate<UdpStream> OnConnectedEvent;

		public override event OnDisconnectingDelegate<UdpStream> OnDisconnectingEvent;

		public override event OnDisconnectedDelegate<UdpStream> OnDisconnectedEvent;

		public override event OnMessageReceivedDelegate<UdpStream> OnMessageReceivedEvent;

		public override event OnMessageSentDelegate<UdpStream> OnMessageSentEvent;

		public override void CreateFromSocket(Socket s)
		{
			throw new NotImplementedException();
		}

		public override void Disconnect()
		{
			throw new NotImplementedException();
		}

		protected override void BeginSend(byte[] data)
		{
			_socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, _endPoint, OnSend, null);
		}
		private void OnSend(IAsyncResult ar)
		{
			try
			{
				_socket.EndSend(ar);
			}
			catch (Exception ex)
			{
				LogManager.Critical("OnSend - Message: {0}", ex.Message);
			}
		}
	}
}
