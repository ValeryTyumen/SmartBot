using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ForestInhabitants;
using NetworkHelpers;

namespace GameServer
{
	public class Program
	{
		private static Forest _forest;
		private static Point _aim;
		private static bool GameOver = false;
		private static readonly int WarFog = 2;
		private static readonly int MaxInhabitantCount = 2;
		private static object _locker = new object();

		private static List<CellChange> _cellChanges;
		private static List<PlayerStateChange> _playerStateChanges;

		private static List<RemotePlayer> _players;

		private static Socket _visualizerSocket;

		public static readonly Dictionary<string, TerrainType> TerrainCode = new Dictionary<string, TerrainType>
		{
			{ "Path", TerrainType.Path },
			{ "Bush", TerrainType.Bush },
			{ "Trap", TerrainType.Trap },
			{ "Life", TerrainType.Life }
		};

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
			_cellChanges = new List<CellChange>();
			_playerStateChanges = new List<PlayerStateChange>();
			var config = ForestConfigLoader.Load();
			_forest = ForestLoader.Load(config.ForestSource);
			_aim = config.Aim;

			_players = new List<RemotePlayer>();
			_visualizerSocket = null;

			TcpListener listener;
			var addressTuple = ParseArgs(args);
			try
			{
				listener = new TcpListener(addressTuple.Item1, addressTuple.Item2);
				listener.Start();
				Console.WriteLine("Server started on {0}:{1}", addressTuple.Item1, addressTuple.Item2);
				RunListener(listener);
			}
			catch (SocketException e)
			{
				Console.WriteLine("Connection error");
			}
		}

		#region Communication methods


		private static void WriteWorldInfo(Socket socket)
		{
			var map = new TerrainType[_forest.Area.Length, _forest.Area[0].Length];
			for (var i = 0; i < _forest.Area.Length; i++)
				for (var j = 0; j < _forest.Area[0].Length; j++)
					map[i, j] = TerrainCode[_forest.Area[i][j].Name];
			var worldInfo = new WorldInfo
			{
				Players = Enumerable
					.Range(0, _players.Count)
					.Select(z => new Player(
						z,
						_players[z].Inhabitant.Name,
						_players[z].Inhabitant.Location,
						_aim,
						_players[z].Inhabitant.Health))
					.ToArray(),
				Map = map
			};
			Bson.Write(socket, worldInfo);
		}

		#endregion

		private static Point FindPlace()
		{
			for (var i = 0; i < _forest.Area.Length; i++)
				for (var j = 0; j < _forest.Area[0].Length; j++)
					if (_forest.Area[i][j].Name != "Bush" && _forest.Area[i][j].Name != "Trap")
					{
						var empty = true;
						foreach (var player in _players)
							if (player.Inhabitant.Location.Equals(new Point(j, i)))
								empty = false;
						if (empty)
							return new Point(j, i);
					}
			return new Point(1, 1);
		}

		private static void AddPlayer(string name, Socket socket)
		{
			Point location;
			location = FindPlace();
			int id;
			id = _players.Count;
			_players.Add(new RemotePlayer(new Inhabitant(name), socket));
			_forest.Place(_players[id].Inhabitant, location.X, location.Y);
		}

		private static void AddVisualizer(Socket socket)
		{
			_visualizerSocket = socket;
		}

		private static void OnGameStart()
		{
			var tasks = new List<Task>();
			WriteWorldInfo(_visualizerSocket);
			var visualizerCommunication = new Task(() => 
				CommunicateWithVisualizer(_visualizerSocket), TaskCreationOptions.LongRunning);
			tasks.Add(visualizerCommunication);
			visualizerCommunication.Start();
			for (var i = 0; i < _players.Count; i++)
			{
				var iLocal = i;
				var playerCommunication = new Task(() => 
					new PlayerTalker(
						_forest, 
						_aim, 
						WarFog, 
						_players, 
						_locker, 
						_cellChanges, 
						_playerStateChanges, 
						OnGameOver
					).CommunicateWithPlayer(iLocal), TaskCreationOptions.LongRunning);
				tasks.Add(playerCommunication);
				playerCommunication.Start();
			}
			Task.WaitAll(tasks.ToArray());
		}

		private static void OnGameOver()
		{
			_visualizerSocket.Close();
			foreach (var player in _players)
			{
				Bson.Write(player.Socket, new MoveResultInfo { Result = 2 });
				player.Socket.Close();
			}
		}

		private static void CommunicateWithVisualizer(Socket socket)
		{
			while (!GameOver)
			{
				Answer answer;
				try
				{
					answer = Bson.Read<Answer>(socket);
				} catch { break; }
				if (answer.AnswerCode == 0)
				{
					LastMoveInfo lastMoveInfo;
					lock (_locker)
					{
						lastMoveInfo = new LastMoveInfo
						{
							GameOver = GameOver,
							CellChanges = _cellChanges.ToArray(),
							PlayerStateChanges = _playerStateChanges.ToArray()
						};
						_cellChanges.Clear();
						_playerStateChanges.Clear();
					}
					if (GameOver)
						OnGameOver();
					try
					{
						Bson.Write(socket, lastMoveInfo);
					} catch { break; }
				}
				else
				{
					Console.WriteLine("Visualizer is broken.");
					socket.Close();
					break;
				}
			}
		}

		private static void RunListener(TcpListener listener)
		{
			while (_visualizerSocket == null || _players.Count < MaxInhabitantCount)
			{
				var socket = listener.AcceptSocket();
				var hello = Bson.Read<Hello>(socket);
				Console.WriteLine("New connection: {0}, isVisualizer: {1}", socket.RemoteEndPoint, hello.IsVisualizator);
				if (hello.IsVisualizator)
					AddVisualizer(socket);
				else
				{
					if (_players.Count < MaxInhabitantCount)
						AddPlayer(hello.Name, socket);
				}
			}
			OnGameStart();
		}
	}
}
