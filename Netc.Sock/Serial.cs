using Netc.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
			string json = JsonConvert.SerializeObject(data);
			return System.Text.Encoding.UTF8.GetBytes(json);
      //return Bytes.ObjectToByteArray(data);
      //using (MemoryStream stream = new MemoryStream())
      //{
      //  Serializer.Serialize(stream, data);// Bytes.ObjectToByteArray(sm);
      //  return stream.ToArray();
      //}
		}
		public static T Deserialise<T>(byte[] data)
		{

			string result = System.Text.Encoding.UTF8.GetString(data);
			//JObject person = JObject.Parse(result);

			return JsonConvert.DeserializeObject<T>(result);
      //return (T)Bytes.ByteArrayToObject(data);
      //using (MemoryStream stream = new MemoryStream(data))
      //{
      //  return Serializer.Deserialize<T>(stream);// Bytes.ObjectToByteArray(sm);
      //}

		}

	}
}
