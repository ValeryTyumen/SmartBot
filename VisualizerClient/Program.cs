using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using ForestInhabitants;
using Visualizer;
using NetworkHelpers;

namespace VisualizerClient
{
	class Program
	{
		private static Dictionary<TerrainType, TerrainFactory> _factories = new Dictionary<TerrainType, TerrainFactory>
		{
            { TerrainType.Path, new TerrainFactory<Path>() },
            { TerrainType.Bush, new TerrainFactory<Bush>() },
            { TerrainType.Trap, new TerrainFactory<Trap>() },
            { TerrainType.Life, new TerrainFactory<Life>() }
        };

		private static List<Inhabitant> _inhabitants;
		private static Forest _forest;
		private static ForestView _view;

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
			_view = new ForestView(new BasicDrawer());
			var socket = client.Client;
			Json.Write(socket, new Hello {IsVisualizator = true});
			var worldInfo = Json.Read<WorldInfo>(socket);
			_inhabitants = new List<Inhabitant>();
			foreach (var player in worldInfo.Players)
			{
				_inhabitants.Add(new Inhabitant(player.Nick));
				_inhabitants.Last().Location = player.StartPosition;
				_inhabitants.Last().Health = player.Hp;
			}
			var area = Enumerable
				.Range(0, worldInfo.Map.GetLength(0))
				.Select(i => Enumerable
					.Range(0, worldInfo.Map.GetLength(1))
					.Select(j => _factories[worldInfo.Map[i, j]].Create())
					.ToArray())
				.ToArray();
			_forest = new Forest(area);
			new ServerTalker(_view, _forest, _inhabitants, _factories).CommunicateWithServer(socket);
		}
	}
}
