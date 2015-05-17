using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace NetworkHelpers
{
	public class Bson
	{
		public static void Write<T>(Socket socket, T obj)
		{
			var stream = new NetworkStream(socket);
			using (var writer = new BsonWriter(stream))
			{
				var serializer = new JsonSerializer();
				serializer.Serialize(writer, obj);
			}
			Thread.Sleep(50);
		}

		public static T Read<T>(Socket socket)
		{
			var stream = new NetworkStream(socket);
			while (!stream.DataAvailable) { }
			using (var reader = new BsonReader(stream))
			{
				var serializer = new JsonSerializer();
				return serializer.Deserialize<T>(reader);
			}
		}
	}
}
