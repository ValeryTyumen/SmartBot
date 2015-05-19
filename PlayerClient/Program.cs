using System;
using System.Net;
using System.Net.Sockets;
using ForestInhabitants;
using NetworkHelpers;

namespace PlayerClient
{
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
			var nickname = "Spider-man";
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
				Name = nickname
			});
			//new BlindServerTalker().CommunicateWithServer(nickname, socket);
			new SmartServerTalker().CommunicateWithServer(nickname, socket);
		}
	}
}
