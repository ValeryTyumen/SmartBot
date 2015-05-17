using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ForestInhabitants;
using Visualizer;
using NetworkHelpers;

namespace VisualizerClient
{
	internal class ServerTalker
	{
		private ForestView _view;
		private Forest _forest;
		private List<Inhabitant> _inhabitants;
		private Dictionary<TerrainType, TerrainFactory> _factories;

		public ServerTalker(ForestView view, Forest forest, List<Inhabitant> inhabitants, 
			Dictionary<TerrainType, TerrainFactory> factories)
		{
			_view = view;
			_forest = forest;
			_inhabitants = inhabitants;
			_factories = factories;
		}

		public void CommunicateWithServer(Socket socket)
		{
			while (true)
			{
				_view.Repaint(_forest, _inhabitants.ToArray());
				Thread.Sleep(50);
				try
				{
					Bson.Write(socket, new Answer { AnswerCode = 0 });
				} catch { break; }
				var lastMoveInfo = Bson.Read<LastMoveInfo>(socket);
				foreach (var change in lastMoveInfo.CellChanges)
					_forest.Area[change.Location.Y][change.Location.X] = _factories[change.Type].Create();
				foreach (var change in lastMoveInfo.PlayerStateChanges)
				{
					_inhabitants[change.Id].Health = change.Hp;
					_inhabitants[change.Id].Location = change.Location;
				}
				if (lastMoveInfo.GameOver)
					break;
			}
			Console.ReadKey();
		}
	}

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
			Bson.Write(socket, new Hello {IsVisualizator = true});
			var worldInfo = Bson.Read<WorldInfo>(socket);
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
