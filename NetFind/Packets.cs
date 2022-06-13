using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace NetFind.Packet
{
	public enum FindType: int
	{
		Find = 1,
		Connect = 2,
		ConnectTCP = 3,
	}
	[Serializable]
	public class UdpFind
	{
		public FindType Type { get; set; } 
		public int PortServer { get; set; }
		public int PortClient { get; set; } // Заполняет тот кто ищет (клиент)
		public IPAddress IPAddressClient { get; set; } // заполняет тот кто ищет (клиент)
		public IPAddress IPAddressServer { get; set; }

		public override string ToString()
		{
			return $"Type: {Type};PortClient: {PortClient}; IPAddressClient: {IPAddressClient}; IPAddressServer: {IPAddressServer}";
		}
		public static byte[] ToByteArray(object obj)
		{
			if (obj == null)
				return null;
			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream ms = new MemoryStream())
			{
				bf.Serialize(ms, obj);
				return ms.ToArray(); 
			}	
		}
		public static UdpFind FromByteArray(byte[] data)
		{
			if (data == null)
				return null;
			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream ms = new MemoryStream(data))
			{
				object obj = bf.Deserialize(ms);
				return (UdpFind)obj;
			}
		}
	}
}
