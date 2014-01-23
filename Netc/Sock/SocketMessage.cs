using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netc.Sock
{
	[Serializable]
	public class SocketMessage
	{
		public string Message { get; set; }
		public object[] Contents { get; set; }
	}
}
