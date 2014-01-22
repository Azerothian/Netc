using Netc.Tcp;
using Netc.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Netc.Packets
{
	public class PacketManager : ThreadHelper
	{

		public const int PacketSize = 2000;
		public event GenericVoidDelegate<TcpClient, byte[]> OnDataReceivedEvent;

		List<PacketList> _sent = new List<PacketList>();
		List<PacketList> _recieved = new List<PacketList>();

		TcpClient _stream;
		MemoryManager _buffer = new MemoryManager();
		short packetListIncrement = 0;
		bool sendComplete = true;

		public PacketManager(TcpClient stream)
		{
			_stream = stream;
			stream.OnMessageReceiveCompleted += stream_OnMessageReceiveCompleted;
			ThreadSleep = 10;
			Start();

		}

		void stream_OnMessageReceiveCompleted(TcpClient stream, byte[] data)
		{
			lock (_buffer)
			{
				_buffer.Write(data);
			}
		}



		public override void ThreadWorker(TimeSpan timeDiff)
		{
			if (_buffer.Length > Packet.HeaderSize)
			{
				ProcessBuffer();
				CancelNextSleep();
			}

			if (sendComplete && _sent.Count > 0)
			{
				processSendQueue();
				CancelNextSleep();
			}
		}
		public void ProcessBuffer()
		{
			IEnumerable<Packet> packets = null;
			lock(_buffer)
			{
				packets = Packet.ScanForPackets(_buffer).ToArray();
			}
			if (packets != null && packets.Count() > 0)
			{
				lock (_recieved)
				{
					foreach (var p in packets)
					{
						var list = (from v in _recieved where v.PacketListId == p.PacketListId select v).FirstOrDefault();
						if (list == null)
						{
							list = new PacketList(p.PacketListId, PacketSize, p.TotalPackets);
							list.PacketListCompleteEvent += newList_PacketListCompleteEvent;
							_recieved.Add(list);
						}
						list.AddPacket(p);
					}
				}
			}
		}


		void newList_PacketListCompleteEvent(PacketList list, byte[] data)
		{
			lock (_recieved)
			{
				list.PacketListCompleteEvent -= newList_PacketListCompleteEvent;
				_recieved.Remove(list);
			}

			if (OnDataReceivedEvent != null)
			{
				OnDataReceivedEvent(_stream, data);
			}
		}



		public void Send(byte[] arr)
		{
			//lock (_sent)
			//{
			// new packet list;
			// send packets
			if (packetListIncrement == short.MaxValue)
			{
				packetListIncrement = 0;
			}
			packetListIncrement++;

			var list = new PacketList(packetListIncrement, PacketSize);
			list.CreatePacketList(arr);

			lock (_sent)
			{
				_sent.Add(list);
			}

			//}
		}

		private void processSendQueue()
		{
			lock (_sent)
			{
				foreach (var k in _sent)
				{
					var p = k.Packets.Where(i => !i.Sent).FirstOrDefault();

					if (p != null)
					{
						LogManager.Info("Index: {0}, ListId: {1}, Sent: {2}", p.PacketIndex, p.PacketListId, p.Sent);
						lock (_stream)
						{
							var data = p.CreatePacket();
							p.Sent = true;
							_stream.Send(data);
						}
					}

				}

				var r = (from v in _sent where v.Packets.Count() == v.Packets.Where(t => t.Sent).Count() select v).ToArray();
				for (var ii = 0; ii < r.Length; ii++ )
				{
					_sent.Remove(r[ii]);
				}
			}
			//LogManager.Info("processSendQueue - finished ");

		}


	}
}
