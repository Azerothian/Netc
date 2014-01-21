using Netc.Abstracts;
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
  public class PacketManager<T> : ThreadHelper
    where T : NetworkAbstractStream<T>
  {

    public const int PacketSize = 50;
    public delegate void OnDataReceivedDelegate(T c, byte[] data);
    public event OnDataReceivedDelegate OnDataReceivedEvent;

    Dictionary<short, PacketList> _sent = new Dictionary<short, PacketList>();
    Dictionary<short, PacketList> _recieved = new Dictionary<short, PacketList>();

    bool _running = true;
    T _stream;
    //Stack _dataStack = new Stack();
    MemoryManager _buffer = new MemoryManager();
    short packetListIncrement = 0;
    bool sendComplete = true;

    //Thread _processor;
    public PacketManager(T stream)
    {
      _stream = stream;
      stream.OnMessageReceivedEvent += stream_OnMessageReceivedEvent;
      // _processor = new Thread(new ThreadStart(Worker));
      //_processor.Start();
      _stream.OnMessageSentEvent += _stream_OnMessageSentEvent;
      ThreadSleep = 10;
      Start();

    }



    public override void ThreadWorker(TimeSpan timeDiff)
    {
      if (_buffer.Length > Packet.HeaderSize)
      {
        ProcessBuffer();
        CancelNextSleep();
      }

      if (sendComplete && _sent.Keys.Count > 0)
      {
        processSendQueue();
        CancelNextSleep();
      }
    }
    public void Report()
    {
      //LogManager.Info(String.Format("Datastack Size {0}", _dataStack.Count));
      //LogManager.Info(String.Format("Buffer Length {0}", _buffer.Length, _buffer.Position));

      //foreach (var v in _recieved.Keys)
      //{
      //    _recieved[v].Report();
      //}
    }
    void stream_OnMessageReceivedEvent(T c, int bytesReceived)
    {
      lock (_buffer)
      {
        byte[] data = new byte[bytesReceived];
        c.Read(data, 0, bytesReceived);
        _buffer.Write(data, 0, bytesReceived);
      }
    }
    public void ProcessBuffer()
    {

      lock (_buffer)
      {

        IEnumerable<Packet> packets = Packet.ScanForPackets(_buffer);
        foreach (var p in packets)
        {
          lock (_recieved)
          {
            if (!_recieved.ContainsKey(p.PacketListId))
            {
              var newList = new PacketList(p.PacketListId, PacketSize, p.TotalPackets);
              newList.PacketListCompleteEvent += newList_PacketListCompleteEvent;
              _recieved.Add(p.PacketListId, newList);
            }
            _recieved[p.PacketListId].AddPacket(p);
          }


        }
      }
    }


    void newList_PacketListCompleteEvent(PacketList list, byte[] data)
    {
      lock (_recieved)
      {
        var l = _recieved[list.PacketListId];
        l.PacketListCompleteEvent -= newList_PacketListCompleteEvent;
        l = null;
        _recieved.Remove(list.PacketListId);
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
        packetListIncrement = 0;
      packetListIncrement++;
      lock (_sent)
      {
        _sent.Add(packetListIncrement, new PacketList(packetListIncrement, PacketSize));
        _sent[packetListIncrement].CreatePacketList(arr);
      }

      //}
    }

    private void processSendQueue()
    {
      lock (_sent)
      {
				
					foreach (var k in _sent.Keys)
					{
						var p = _sent[k].Packets.Where(i => !i.Sent).FirstOrDefault();
						if (p != null)
						{
							//LogManager.Info(String.Format("Assembling Index {0}, Length {1}, CRC {2}", p.PacketIndex, p.PacketContents.Length, BitConverter.ToString(CRC.CalculateCRC(p.PacketContents))));
							lock (_stream)
							{
								p.WritePacket(_stream);
							}
						}
					
				}


        var e = _sent.Where(u => u.Value.Packets.Where(t => t.Sent).Count() == u.Value.Packets.Count).ToArray();
        foreach (var co in e)
        {
          _sent.Remove(co.Key);
        }
      }
      //LogManager.Info("processSendQueue - finished ");

    }

    private void _stream_OnMessageSentEvent(T client, int bytesSent)
    {
      //processSendQueue();
    }

  }
}
