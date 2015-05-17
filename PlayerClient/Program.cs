using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ForestInhabitants;
using NetworkHelpers;

namespace PlayerClient
{
	internal class ServerTalker
	{
		private Inhabitant _inhabitant;

		public ServerTalker(Inhabitant inhabitant)
		{
			_inhabitant = inhabitant;
		}

		public void CommunicateWithServer(Socket socket)
		{
			var clientInfo = Bson.Read<ClientInfo>(socket);
			Console.WriteLine("Got client info.");

			_inhabitant.Location = clientInfo.StartPosition;
			IAi ai = new Ai();
			foreach (var direction in ai.Find(_inhabitant, clientInfo.Target, clientInfo.MapSize))
			{
				Bson.Write(socket, new Move
				{
					Direction = direction
				});
				var moveResultInfo = Bson.Read<MoveResultInfo>(socket);
				if (moveResultInfo.Result == 0)
				{
					_inhabitant.Location = _inhabitant.Location.Add(Forest.Movings[direction]);
				}
				if (moveResultInfo.Result == 2)
					break;
				Console.WriteLine("Went to {0}, result: {1}", direction, moveResultInfo.Result);
			}
		}
	}


	class Program
	{
		private static Inhabitant _inhabitant;
		private static Forest _forest;
		private static int _warFog;

		private static Tuple<IPAddress, int> ParseArgs(string[] args)
		{
			var address = IPAddress.Parse("127.0.0.1");
			var port = 20000;
			if (args.Length > 0)
				address = IPAddress.Parse(args[0]);
			if (args.Length > 1)
				port = int.Parse(args[1]);
			return Tuple.Create(address, port);
		}

		static void Main(string[] args)
		{
			_inhabitant = new Inhabitant("Spider-man");
			var addressTuple = ParseArgs(args);
			var client = new TcpClient();
			try
			{
				client.Connect(addressTuple.Item1, addressTuple.Item2);
			}
			catch
			{
				Console.WriteLine("Server connection error.");
				return;
			}
			Console.WriteLine("Connected to server.");
			var socket = client.Client;
			Bson.Write(socket, new Hello
			{
				IsVisualizator = false,
				Name = _inhabitant.Name
			});
			new ServerTalker(_inhabitant).CommunicateWithServer(socket);
		}
	}
}
