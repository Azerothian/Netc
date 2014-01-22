using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netc.Util
{
	public delegate void GenericVoidDelegate<T>(T obj1);
	public delegate void GenericVoidDelegate<T, T1>(T obj1, T1 obj2);
	public delegate void GenericVoidDelegate<T, T1, T2>(T obj1, T1 obj2, T2 obj3);
}

