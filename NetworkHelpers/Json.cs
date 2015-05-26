using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace NetworkHelpers
{
	public class Json
	{
        static public void Write<T>(Socket socket, T obj)
        {
            var stream = new NetworkStream(socket);
            var serializer = new JsonSerializer();
            var writer = new JsonTextWriter(new StreamWriter(stream));
            try
            {
                serializer.Serialize(writer, obj);
                writer.Flush();
            }
            catch (Exception e)
            {
                //                Console.WriteLine(e);
                writer.Close();
                throw;
            }
        }

        static public T Read<T>(Socket socket) where T : class
        {
            var stream = new NetworkStream(socket);
            var serializer = new JsonSerializer();
            var reader = new JsonTextReader(new StreamReader(stream));
            T obj = null;
            try
            {
                obj = serializer.Deserialize<T>(reader);
            }
            catch (Exception e)
            {
                //                Console.WriteLine(e);
                reader.Close();
                throw;
            }
            return obj;
        }
    }
}
