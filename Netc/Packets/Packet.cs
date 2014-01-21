using Netc.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Netc.Packets
{
	public class Packet
	{
		public bool Sent = false;
		public const int HeaderSize = 16;
		// 0 - Packet Flag
		// 1 - 2 ListId
		// 3 - 4 Index
		// 5 - 6 PacketSize
		// 7 - 8 TotalPackets
		// 9 - 10 TotalSize
		// 11 - 12 - 13 - 14 CRC
		// 15 - Seperator


		public short PacketIndex; //2
		public short PacketSize; // 2
		public short PacketListId; //2
		public short TotalPackets; //2
		public short TotalSize; //2

		public byte[] CRC; // 4
		public byte[] Contents;
		public Packet()
		{
		}
		public Packet(short packetListId, short packetIndex, short totalPackets, short totalSize,byte[] packetContents, byte[] crc)
		{
			TotalPackets = totalPackets;
			PacketIndex = packetIndex;
			PacketSize = (short)packetContents.Length;
			Contents = packetContents;
			PacketListId = packetListId;
			TotalSize = totalSize;
			CRC = crc;

		}

		public void WritePacket(Stream c)
		{
      LogManager.Info("Write Packet ListId {0}, Index {1}, Size {2}", PacketListId, PacketIndex, Contents.Length);
			var _packetListId = BitConverter.GetBytes(PacketListId);
			var _packetIndex = BitConverter.GetBytes(PacketIndex);
      var _packetSize = BitConverter.GetBytes(Contents.Length);
			var _totalPackets = BitConverter.GetBytes(TotalPackets);
			var _totalSize = BitConverter.GetBytes(TotalSize);

			c.WriteByte(PacketDescriptions.Packet);// 1
			c.Write(_packetListId, 0, sizeof(short)); //2
			c.Write(_packetIndex, 0, sizeof(short));// 2
			c.Write(_packetSize, 0, sizeof(short));// 2
			c.Write(_totalPackets, 0, sizeof(short));//2 
			c.Write(_totalSize, 0, 2);//2 
			c.Write(CRC, 0, 4);//4
			c.WriteByte(PacketDescriptions.PacketSeparator); //1
			c.Write(Contents, 0, Contents.Length);
			c.WriteByte(PacketDescriptions.PacketEnd); //1
			c.Flush();
			Sent = true;
		}

    public static IEnumerable<Packet> ScanForPackets(MemoryManager stream)
		{
			if(stream.Length > HeaderSize)
			{
				for(long i = 0; i < stream.Length - HeaderSize; i++)
				{
					stream.Position = i;
					byte[] header = new byte[HeaderSize];
					stream.Read(header, 0, HeaderSize);
					if (header[0] != PacketDescriptions.Packet)
					{
						continue;
					}
					if (header[HeaderSize - 1] != PacketDescriptions.PacketSeparator)
					{
						continue;
					}
          

					var packet = new Packet();
					packet.PacketListId = BitConverter.ToInt16(new byte[] { header[1], header[2] }, 0);
					packet.PacketIndex = BitConverter.ToInt16(new byte[] { header[3], header[4] }, 0);
					packet.PacketSize = BitConverter.ToInt16(new byte[] { header[5], header[6] }, 0);
					packet.TotalPackets = BitConverter.ToInt16(new byte[] { header[7], header[8] }, 0);
					packet.TotalSize = BitConverter.ToInt16(new byte[] { header[9], header[10] }, 0);
					packet.CRC = new byte[] { header[11], header[12], header[13], header[14] };

          if (stream.Length <= i + HeaderSize + packet.PacketSize + 1)
            break;

          stream.Position = i + HeaderSize + packet.PacketSize ;
          var endCheck = stream.ReadByte();
          if (endCheck != PacketDescriptions.PacketEnd)
          {
            continue;
          }


          packet.Contents = new byte[packet.PacketSize];
          stream.Position = i + HeaderSize;
          stream.Read(packet.Contents, 0, packet.PacketSize);

          var end = stream.ReadByte();
          if (end != PacketDescriptions.PacketEnd)
            continue;
          var totalPacketSize = HeaderSize + (int)packet.PacketSize + 1;
          stream.Remove(0, (int)i + totalPacketSize);
          i = 0;

          yield return packet;


				}

			}
		}


    public static Packet ReadPacket(MemoryStream c)
    {
			c.Position = 0;
			byte[] header = new byte[HeaderSize];
			c.Read(header, 0, HeaderSize);
			if (header[0] != PacketDescriptions.Packet)
				throw new Exception("Packet Header is missing");
			if (header[HeaderSize - 1] != PacketDescriptions.PacketSeparator)
				throw new Exception("Packet Separator is missing");



			var packet = new Packet();
			packet.PacketListId = BitConverter.ToInt16(new byte[] { header[1], header[2] }, 0);
			packet.PacketIndex = BitConverter.ToInt16(new byte[] { header[3], header[4] }, 0);
			packet.PacketSize = BitConverter.ToInt16(new byte[] { header[5], header[6] }, 0);
			packet.TotalPackets = BitConverter.ToInt16(new byte[] { header[7], header[8] }, 0);
			packet.TotalSize = BitConverter.ToInt16(new byte[] { header[9], header[10] }, 0);
			packet.CRC = new byte[] { header[11], header[12], header[13], header[14] };

			c.Position = HeaderSize + packet.PacketSize;
			var end = c.ReadByte();

			if (end != PacketDescriptions.PacketEnd)
				return null;
				//throw new Exception("Packet End is missing");

			c.Position = HeaderSize;

			packet.Contents = new byte[packet.PacketSize];

			c.Read(packet.Contents, 0, packet.PacketSize);

			if(c.Length > HeaderSize + packet.PacketSize +1)
			{
				byte[] remainingData = new byte[c.Length - (HeaderSize + packet.PacketSize + 1)];
				c.Position = HeaderSize + packet.PacketSize + 1;
				c.Read(remainingData, 0, remainingData.Length);
				c = new MemoryStream();
				c.Write(remainingData, 0, remainingData.Length);
			}
			return packet;
    }



		//public static Packet ReadPacket(MemoryStream c)
		//{
		//	if (c.Length > HeaderSize)
		//	{
		//		c.Position = 0;
		//		byte check = (byte)c.ReadByte();
		//		if (check == PacketDescriptions.Packet)
		//		{
		//			byte[] _header = new byte[9];
		//			c.Read(_header, 0, 9);
		//			c.Position = 1;
		//			if (_header[8] == PacketDescriptions.PacketSeparator)
		//			{
		//				// Process Packet
		//				var packet = new Packet();
		//				byte[] _shortArr = new byte[sizeof(short)];

		//				c.Read(_shortArr, 0, sizeof(short));
		//				packet.PacketListId = BitConverter.ToInt16(_shortArr, 0);

		//				c.Read(_shortArr, 0, sizeof(short));
		//				packet.PacketIndex = BitConverter.ToInt16(_shortArr, 0);

		//				c.Read(_shortArr, 0, sizeof(short));
		//				packet.PacketSize = BitConverter.ToInt16(_shortArr, 0);

		//				c.Read(_shortArr, 0, sizeof(short));
		//				packet.TotalPackets = BitConverter.ToInt16(_shortArr, 0);

		//				c.ReadByte(); // Seperator

		//				if (c.Length - c.Position >= packet.PacketSize)
		//				{
		//					var data = new byte[packet.PacketSize];
		//					c.Read(data, 0, packet.PacketSize);
		//					packet.PacketContents = data;

		//					byte[] b = new byte[c.Length - c.Position];

		//					c.Read(b, 0, (int)(c.Length - c.Position));
		//					c.SetLength(0);
		//					c.Write(b, 0, b.Length);
		//					return packet;
		//				}

		//			}
		//		}
		//	}
		//	return null;

		//}
	}
}
