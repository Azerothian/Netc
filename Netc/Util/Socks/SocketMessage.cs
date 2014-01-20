using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netc.Util.Socks
{
	[Serializable]
	public class SocketMessage
	{
		public string MessageName;
		public object MessageContents;
	}
}
