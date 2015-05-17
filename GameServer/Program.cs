using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ForestInhabitants;
using NetworkHelpers;
using Newtonsoft.Json;

namespace GameServer
{
	internal class ForestConfig
	{
		public string ForestSource { get; private set; }
		public Point Aim { get; private set; }

		public ForestConfig(string forestSource, Point aim)
		{
			ForestSource = forestSource;
			Aim = aim;
		}
	}

	internal class ForestConfigLoader
	{
		private const string ConfigSource = "config.txt";

		public static ForestConfig Load()
		{
			var data = File.ReadAllText(ConfigSource);
			return JsonConvert.DeserializeObject<ForestConfig>(data);
		}
	}

	internal class RemotePlayer
	{
		public Inhabitant Inhabitant { get; private set; }
		public Socket Socket { get; private set; }

		public RemotePlayer(Inhabitant inhabitant, Socket socket)
		{
			Inhabitant = inhabitant;
			Socket = socket;
		}
	}

	internal class PlayerTalker
	{
		private readonly Forest _forest;
		private readonly Point _aim;
		private readonly int _maxInhabitantCount;
		private readonly List<RemotePlayer> _players;
		private readonly object _locker;
		private readonly List<CellChange> _cellChanges;
		private readonly List<PlayerStateChange> _playerStateChanges;
		private readonly Action _onGameOver;

		public PlayerTalker(
			Forest forest,
			Point aim,
			int maxInhabitantCount,
			List<RemotePlayer> players,
			object locker,
			List<CellChange> cellChanges,
			List<PlayerStateChange> playerStateChanges,
			Action onGameOver
			)
		{
			_forest = forest;
			_aim = aim;
			_maxInhabitantCount = maxInhabitantCount;
			_players = players;
			_locker = locker;
			_cellChanges = cellChanges;
			_playerStateChanges = playerStateChanges;
			_onGameOver = onGameOver;
		}

		private TerrainType[,] GetVisibleArea(Point location)
		{
			var area = new TerrainType[_maxInhabitantCount * 2 + 1, _maxInhabitantCount * 2 + 1];
			var yMin = location.Y - _maxInhabitantCount;
			var yMax = location.Y + _maxInhabitantCount;
			var xMin = location.X - _maxInhabitantCount;
			var xMax = location.X + _maxInhabitantCount;
			for (var i = yMin; i <= yMax; i++)
				for (var j = xMin; j <= xMax; j++)
				{
					var code = TerrainType.None;
					if (i > 0 && j > 0 && i < _forest.Area.Length && j < _forest.Area[0].Length)
						code = Program.TerrainCode[_forest.Area[i][j].Name];
					area[i - yMin, j - xMin] = code;
				}
			return area;
		}

		private void WriteClientInfo(Socket socket, Inhabitant inhabitant)
		{
			var info = new ClientInfo
			{
				MapSize = new Point(_forest.Area[0].Length, _forest.Area.Length),
				Hp = inhabitant.Health,
				StartPosition = inhabitant.Location,
				Target = _aim,
				VisibleMap = GetVisibleArea(inhabitant.Location)
			};
			Bson.Write(socket, info);
		}

		private void WriteMoveResultInfo(Socket socket, MovementResult result, Inhabitant inhabitant)
		{
			var first = 0;
			if (result.Direction == Direction.Stay)
				first = 1;
			if (inhabitant.Location.Equals(_aim))
				first = 2;
			Bson.Write(socket, new MoveResultInfo
			{
				Result = first,
				VisibleMap = GetVisibleArea(inhabitant.Location)
			});
		}

		public void CommunicateWithPlayer(int id)
		{
			WriteClientInfo(_players[id].Socket, _players[id].Inhabitant);
			var gameOver = false;
			while (!gameOver)
			{
				Move move;
				try
				{
					move = Bson.Read<Move>(_players[id].Socket);
				} catch { break; }
				MovementResult result;
				lock (_locker)
				{
					result = _forest.Move(_players[id].Inhabitant, move.Direction);
					if (result.Direction != Direction.Stay)
						_playerStateChanges.Add(new PlayerStateChange
						{
							Id = id,
							Location = _players[id].Inhabitant.Location,
							Hp = _players[id].Inhabitant.Health
						});
					var location = _players[id].Inhabitant.Location;
					if (_forest.Area[location.Y][location.X].Name
							!= result.Change.Name && result.Direction != Direction.Stay)
						_cellChanges.Add(new CellChange
						{
							Location = _players[id].Inhabitant.Location,
							Type = Program.TerrainCode[result.Change.Name]
						});
					gameOver = _players[id].Inhabitant.Location.Equals(_aim);
					if (gameOver)
						_onGameOver();
				}
				try
				{
					WriteMoveResultInfo(_players[id].Socket, result, _players[id].Inhabitant);
				} catch { break; }
			}
		}
	}

	public class Program
	{
		private static Forest _forest;
		private static Point _aim;
		private static bool GameOver = false;
		private static readonly int WarFog = 1;
		private static readonly int MaxInhabitantCount = 2;
		private static object _locker = new object();
		private static object _logLocker = new object();

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
						MaxInhabitantCount, 
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
