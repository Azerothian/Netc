using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netc.Util.Socks
{
	[Serializable]
	public class SocketMessage
	{
    public string MessageName { get; set; }
    public object[] MessageContents { get; set; }
	}
}
