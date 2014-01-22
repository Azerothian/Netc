using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Netc.Util
{
	public class Bytes
	{
		public static byte[] ObjectToByteArray(Object obj)
		{
			if (obj.GetType().IsSerializable)
			{
				MemoryStream fs = new MemoryStream();
				BinaryFormatter formatter = new BinaryFormatter();
				try
				{
					formatter.Serialize(fs, obj);
					return fs.ToArray();
				}
				catch (SerializationException ex)
				{
					LogManager.Critical("ObjectToByteArray Failed", ex, obj);
				}
				finally
				{
					fs.Close();
				}
			}
			return null;
		}
		public static object ByteArrayToObject(Byte[] Buffer)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream fs = new MemoryStream(Buffer);
			try
			{
				return formatter.Deserialize(fs);
			}
			catch (SerializationException ex)
			{
				LogManager.Log(Netc.Util.LogManager.LogType.Critical, "ByteArrayToObject Failed", ex, Buffer);
				return null;
			}
			finally
			{
				fs.Close();
			}
		}

	}
}
