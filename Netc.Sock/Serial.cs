﻿using Netc.Util;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Netc.Sock
{
	public static class Serial
	{
		public static byte[] Serialise(object data)
		{

      return Bytes.ObjectToByteArray(data);
      //using (MemoryStream stream = new MemoryStream())
      //{
      //  Serializer.Serialize(stream, data);// Bytes.ObjectToByteArray(sm);
      //  return stream.ToArray();
      //}
		}
		public static T Deserialise<T>(byte[] data)
		{

      return (T)Bytes.ByteArrayToObject(data);
      //using (MemoryStream stream = new MemoryStream(data))
      //{
      //  return Serializer.Deserialize<T>(stream);// Bytes.ObjectToByteArray(sm);
      //}

		}

	}
}
