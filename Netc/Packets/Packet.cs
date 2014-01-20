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
		public const int HeaderSize = 10;
		public short PacketIndex; //2
		public short PacketSize; // 2
		public short PacketListId; //2
		public short TotalPackets; //2
		public byte[] PacketContents;
		public Packet()
		{
		}
		public Packet(short packetListId, short packetIndex, short totalPackets, byte[] packetContents)
		{
			TotalPackets = totalPackets;
			PacketIndex = packetIndex;
			PacketSize = (short)packetContents.Length;
			PacketContents = packetContents;
			PacketListId = packetListId;
		}

		public void WritePacket(Stream c)
		{

			var _packetListId = BitConverter.GetBytes(PacketListId);
			var _packetIndex = BitConverter.GetBytes(PacketIndex);
			var _packetSize = BitConverter.GetBytes(PacketSize);
			var _totalPackets = BitConverter.GetBytes(TotalPackets);

			c.WriteByte(PacketDescriptions.Packet);// 1
			c.Write(_packetListId, 0, sizeof(short)); //2
			c.Write(_packetIndex, 0, sizeof(short));// 2
			c.Write(_packetSize, 0, sizeof(short));// 2
			c.Write(_totalPackets, 0, sizeof(short));//2 
			c.WriteByte(PacketDescriptions.PacketSeparator); //1
			c.Write(PacketContents, 0, PacketSize);
			c.Flush();
			Sent = true;
		}
    public static Packet ReadPacket(byte[] c)
    {
      if (c.Length > HeaderSize)
      {
        byte check = (byte)c[0];
        if (check == PacketDescriptions.Packet)
        {
          //byte[] _header = new byte[9];
          //c.Read(_header, 0, 9);
          //c.Position = 1;
          if (c[9] == PacketDescriptions.PacketSeparator)
          {
            // Process Packet
            var packet = new Packet();
            packet.PacketListId = BitConverter.ToInt16(new byte[] { c[1],c[2] }, 0);

            packet.PacketIndex = BitConverter.ToInt16(new byte[] { c[3], c[4] }, 0);
            packet.PacketSize = BitConverter.ToInt16(new byte[] { c[5], c[6] }, 0);

            packet.TotalPackets = BitConverter.ToInt16(new byte[] { c[7], c[8] }, 0);

             byte[] b = new byte[c.Length - 10];
             Buffer.BlockCopy(c, 10, b, 0, b.Length);
             packet.PacketContents = b;
             return packet;
          }
        }
      }
      return null;

    }



		public static Packet ReadPacket(MemoryStream c)
		{
			if (c.Length > HeaderSize)
			{
				c.Position = 0;
				byte check = (byte)c.ReadByte();
				if (check == PacketDescriptions.Packet)
				{
					byte[] _header = new byte[9];
					c.Read(_header, 0, 9);
					c.Position = 1;
					if (_header[8] == PacketDescriptions.PacketSeparator)
					{
						// Process Packet
						var packet = new Packet();
						byte[] _shortArr = new byte[sizeof(short)];

						c.Read(_shortArr, 0, sizeof(short));
						packet.PacketListId = BitConverter.ToInt16(_shortArr, 0);

						c.Read(_shortArr, 0, sizeof(short));
						packet.PacketIndex = BitConverter.ToInt16(_shortArr, 0);

						c.Read(_shortArr, 0, sizeof(short));
						packet.PacketSize = BitConverter.ToInt16(_shortArr, 0);

						c.Read(_shortArr, 0, sizeof(short));
						packet.TotalPackets = BitConverter.ToInt16(_shortArr, 0);

						c.ReadByte(); // Seperator

						if (c.Length - c.Position >= packet.PacketSize)
						{
							var data = new byte[packet.PacketSize];
							c.Read(data, 0, packet.PacketSize);
							packet.PacketContents = data;

							byte[] b = new byte[c.Length - c.Position];

							c.Read(b, 0, (int)(c.Length - c.Position));
							c.SetLength(0);
							c.Write(b, 0, b.Length);
							return packet;
						}

					}
				}
			}
			return null;

		}
	}
}
