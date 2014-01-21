using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Netc.Util
{
  public class MemoryManager : Stream
  {

    private MemoryStream _memoryStream = new MemoryStream();
    

    public override bool CanRead
    {
      get { return _memoryStream.CanRead; }
    }

    public override bool CanSeek
    {
      get { return _memoryStream.CanSeek; }
    }

    public override bool CanWrite
    {
      get { return _memoryStream.CanWrite; }
    }

    public override void Flush()
    {
      _memoryStream.Flush(); 
    }

    public override long Length
    {
      get { return _memoryStream.Length; }
    }

    public override long Position
    {
      get
      {
        return _memoryStream.Position;
      }
      set
      {
        _memoryStream.Position = value;
      }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      lock (_memoryStream)
      {
        return _memoryStream.Read(buffer, offset, count);
      }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      lock (_memoryStream)
      {
        return _memoryStream.Seek(offset, origin);
      }
    }

    public override void SetLength(long value)
    {
      lock (_memoryStream)
      {
        _memoryStream.SetLength(value);
      }
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      lock (_memoryStream)
      {
        _memoryStream.Write(buffer, offset, count);
      }
    }
    public void Write(byte[] buffer)
    {
      lock (_memoryStream)
      {
        _memoryStream.Write(buffer, 0, buffer.Length);
      }
    }
    public void Remove(int index, int count)
    {
      lock (_memoryStream)
      {
        if (index > _memoryStream.Length)
          throw new ArgumentException("Index is out of range");
        if (index + count > _memoryStream.Length)
          throw new ArgumentException("Total of index and count is out of range");


        var newSize = _memoryStream.Length - count;

        if (newSize > 0)
        {
          byte[] buffer = _memoryStream.GetBuffer();

          byte[] startArray = null;
          byte[] endArray = null;
          //int newLength = index; ;

          if (index > 0)
          {
            startArray = new byte[index];
            Buffer.BlockCopy(buffer, 0, startArray, 0, index);
          }
          int endLength = (int)_memoryStream.Length - (index + count);

          if (endLength > 0)
          {
            //newLength += endLength;
            endArray = new byte[endLength];
            Buffer.BlockCopy(buffer, index + count, endArray, 0, endLength);
          }

          _memoryStream.Position = 0;
          _memoryStream.SetLength(0);
          if (startArray != null)
          {
            Write(startArray);
          }
          if (endArray != null)
          {
            Write(endArray);
          }
        }
        else
        {
          //Clear();
          _memoryStream.Close();
          _memoryStream = new MemoryStream();
        }
      }
    }

    public void Clear()
    {
      lock (_memoryStream)
      {
        _memoryStream.Close();
        _memoryStream = new MemoryStream();
      }
    }
    //public 
  }
}
