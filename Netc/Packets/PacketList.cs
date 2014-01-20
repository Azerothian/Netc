using Netc.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Netc.Packets
{
	public class PacketList
	{
		public delegate void PacketListCompleteDelegate(PacketList list, byte[] data);
		public event PacketListCompleteDelegate PacketListCompleteEvent;



		private byte[] _packetContents = null;
		private List<Packet> _packets;


		public List<Packet> Packets
		{
			get
			{
				return _packets;
			}
		}
		public PacketList()
		{
		}
		public PacketList(short packetListId)
		{
			_packets = new List<Packet>();
			PacketListId = packetListId;
		}

		public PacketList(short packetListId, short packetSize)
		{
			_packets = new List<Packet>();
			PacketListId = packetListId;
			PacketSize = packetSize;
		}
		public PacketList(short packetListId, short packetSize, short totalPackets)
		{
			_packets = new List<Packet>();
			PacketListId = packetListId;
			PacketSize = packetSize;
			TotalPackets = totalPackets;
		}

		public const int HeaderSize = 6;
		public short PacketListId = -1; //2
		public short TotalPackets = -1; // 2
		public short PacketSize = 50; // 2


		public short CurrentTotalPackets
		{
			get
			{
				return (short)_packets.Count;
			}
		}


		public byte[] PacketContents
		{
			get
			{
				return _packetContents;
			}
		}

		#region Receiving Objects
		public bool AddPacket(Packet p)
		{
			var indexCount = (from v in _packets where v.PacketIndex == p.PacketIndex select v).Count() > 0;
			if (indexCount)
			{
				return false;
			}
			_packets.Add(p);
			if (ReceivedAllPackets)
			{
				AssemblePackets();
				return true;
			}
			return false;
		}
		public bool ReceivedAllPackets
		{
			get
			{

				return TotalPackets == _packets.Count;
			}
		}
		public void Report()
		{
			LogManager.Info(String.Format("List ID {0}, Total {1}, Current {2}", PacketListId, TotalPackets, _packets.Count));
		}
		private void AssemblePackets()
		{
			if (!ReceivedAllPackets && _packetContents == null)
				throw new Exception("Not all packets have been recieved yet");
			MemoryStream ms = new MemoryStream();
			lock (_packets)
			{
				foreach (var p in _packets.OrderBy(d => d.PacketIndex))
				{
					//LogManager.Info(String.Format("Assembling Index {0}, Length {1}, CRC {2}", p.PacketIndex, p.PacketContents.Length, BitConverter.ToString(CRC.CalculateCRC(p.PacketContents))));
					ms.Write(p.PacketContents, 0, p.PacketContents.Length);
				}
			}
			ms.Position = 0;
			_packetContents = ms.ToArray();
			if (PacketListCompleteEvent != null)
			{
				PacketListCompleteEvent(this, _packetContents);
			}
		}

		#endregion

		#region Sending Objects
		public void CreatePacketList(byte[] data)
		{
			_packets.Clear();
			int _totalPackets = data.Length / PacketSize;
			if (data.Length % PacketSize > 0)
				_totalPackets++;
			if (_totalPackets == 0 && data.Length > 0)
				_totalPackets = 1;
			if (_totalPackets > short.MaxValue)
				throw new Exception("Too much data to send in one packet, maybe increase your packetsize or decrease the amount of data you are trying to send at once. (3.999877930618823 Gb is the Max amount you can send in one packet list)");
			short index = 0;
			for (int i = 0; i < data.Length; i = i + PacketSize)
			{
				short currentSize = PacketSize;

				short check = (short)(data.Length - (i + PacketSize));
				if (check < 0)
				{
					currentSize = (short)(data.Length - i);
				}

				byte[] _packetContents = new byte[currentSize];
				Buffer.BlockCopy(data, i, _packetContents, 0, currentSize);
				_packets.Add(new Packet(PacketListId, index, (short)_totalPackets, _packetContents));
				index++;
			}
		}
		#endregion

	}
}
