using Netc.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;


namespace Netc.Abstracts
{

	public abstract class NetworkAbstractStream<T> : Stream
		where T : NetworkAbstractStream<T>
	{
    private MemoryManager _incomingMemoryStream = new MemoryManager();
    private MemoryManager _outgoingMemoryStream = new MemoryManager();

		//public MemoryStream IncomingMemoryStream
		//{
		//	get
		//	{
		//		lock(_incomingLock)
		//		{
		//			return _incomingMemoryStream;
		//		}
		//	}
		//	set
		//	{
		//		lock (_incomingLock)
		//		{
		//			_incomingMemoryStream = value;
		//		}

		//	}
		//}
		//public MemoryStream OutgoingMemoryStream
		//{
		//	get
		//	{
		//		lock (_outgoingLock)
		//		{
		//			return _outgoingMemoryStream;
		//		}
		//	}
		//	set
		//	{
		//		lock (_outgoingLock)
		//		{
		//			_outgoingMemoryStream = value;
		//		}
		//	}
		//}
    //protected object _incomingLock = 1;
    //protected object _outgoingLock = 1;


		/// <summary>
		/// Occurs when [on connecting event].
		/// </summary>
		public abstract event OnConnectingDelegate<T> OnConnectingEvent;
		/// <summary>
		/// Occurs when [on connected event].
		/// </summary>
		public abstract event OnConnectedDelegate<T> OnConnectedEvent;

		/// <summary>
		/// Occurs when [on disconnecting event].
		/// </summary>
		public abstract event OnDisconnectingDelegate<T> OnDisconnectingEvent;
		/// <summary>
		/// Occurs when [on disconnected event].
		/// </summary>
		public abstract event OnDisconnectedDelegate<T> OnDisconnectedEvent;
		/// <summary>
		/// Occurs when [on message received event].
		/// </summary>
		public abstract event OnMessageReceivedDelegate<T> OnMessageReceivedEvent;
				/// <summary>
		/// Occurs when [on message sent event].
		/// </summary>
		public abstract event OnMessageSentDelegate<T> OnMessageSentEvent;

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
		/// </summary>
		/// <returns>true if the stream supports reading; otherwise, false.</returns>
		public override bool CanRead
		{
			get { return true; }
		}
		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
		/// </summary>
		/// <returns>true if the stream supports seeking; otherwise, false.</returns>
		public override bool CanSeek
		{
			get { return false; }
		}
		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
		/// </summary>
		/// <returns>true if the stream supports writing; otherwise, false.</returns>
		public override bool CanWrite
		{
			get { return true; }
		}
		/// <summary>
		/// When overridden in a derived class, sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
		/// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
		/// <returns>
		/// The new position within the current stream.
		/// </returns>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
		///   
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}
		/// <summary>
		/// When overridden in a derived class, sets the length of the current stream.
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
		///   
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}
		/// <summary>
		/// When overridden in a derived class, gets or sets the position within the current stream.
		/// </summary>
		/// <returns>The current position within the stream.</returns>
		///   
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
		///   
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}


		public abstract void CreateFromSocket(Socket s);

		//public abstract long Length;
		/// <summary>
		/// Connects the specified ip address.
		/// </summary>
		/// <param name="IpAddress">The ip address.</param>
		/// <param name="Port">The port.</param>
		public abstract void Connect(string IpAddress, int Port);
		/// <summary>
		/// Disconnects this instance.
		/// </summary>
		public abstract void Disconnect();
		/// <summary>
		/// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		/// </summary>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		public override void Flush()
		{

			byte[] data = null;

      lock (_outgoingMemoryStream)
			{

				if (_outgoingMemoryStream.Length > 0)
				{
					data = new byte[_outgoingMemoryStream.Length];
					_outgoingMemoryStream.Position = 0;
					_outgoingMemoryStream.Read(data, 0, (int)_outgoingMemoryStream.Length);
          _outgoingMemoryStream.Clear();
				}
			}
			if(data != null)
			{
				BeginSend(data);
			}

		}

		protected abstract void BeginSend(byte[] data);
		/// <summary>
		/// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>
		/// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. </exception>
		///   
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="buffer"/> is null. </exception>
		///   
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
		///   
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
		///   
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		public override int Read(byte[] buffer, int offset, int count)
		{
      lock (_incomingMemoryStream)
			{
				_incomingMemoryStream.Position = 0;
        var cnt =_incomingMemoryStream.Read(buffer, 0, count);
        _incomingMemoryStream.Remove(0, count);
				return cnt;
			}
		}
		/// <summary>
		/// Peeks the specified buffer.
		/// </summary>
		/// <param name="buffer">The buffer.</param>
		/// <param name="offset">The offset.</param>
		/// <param name="count">The count.</param>
		/// <returns></returns>
		public int Peek(byte[] buffer, int offset, int count)
		{
      lock (_incomingMemoryStream)
			{
				var cnt = _incomingMemoryStream.Read(buffer, offset, count);
				_incomingMemoryStream.Position = 0;
				return cnt;
			}
		}
		/// <summary>
		/// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		/// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length. </exception>
		///   
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="buffer"/> is null. </exception>
		///   
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
		///   
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
		///   
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		public override void Write(byte[] buffer, int offset, int count)
		{
      lock (_outgoingMemoryStream)
			{
				_outgoingMemoryStream.Write(buffer, offset, count);
			}
		}


		public void WriteToIncomingStream(byte[] buffer, int offset, int count)
		{
      lock (_incomingMemoryStream)
			{
				_incomingMemoryStream.Write(buffer, offset, count);
			}
		}
    public void WriteToIncomingStream(byte buffer)
    {
      lock (_incomingMemoryStream)
      {
        _incomingMemoryStream.WriteByte(buffer);
      }
    }
		/// <summary>
		/// When overridden in a derived class, gets the length in bytes of the stream.
		/// </summary>
		/// <returns>A long value representing the length of the stream in bytes.</returns>
		///   
		/// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
		///   
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		public override long Length
		{
			get
			{
        lock (_incomingMemoryStream)
				{ 
          return _incomingMemoryStream.Length; 
        }
			}
		}

	}
}
