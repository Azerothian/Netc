using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netc.Sock
{
	[ProtoContract]
	public class SocketMessage<T>
	{
		[ProtoMember(1)]
		public string Message { get; set; }
		[ProtoMember(2)]
		public T[] Contents { get; set; }
	}
}
