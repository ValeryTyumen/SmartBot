using System;
using System.Collections.Generic;
using System.Net.Sockets;
using ForestInhabitants;
using NetworkHelpers;

namespace GameServer
{
	internal class PlayerTalker
	{
		private readonly Forest _forest;
		private readonly Point _aim;
		private readonly int _warFog;
		private readonly List<RemotePlayer> _players;
		private readonly object _locker;
		private readonly List<CellChange> _cellChanges;
		private readonly List<PlayerStateChange> _playerStateChanges;
		private readonly Action _onGameOver;

		public PlayerTalker(
			Forest forest,
			Point aim,
			int warFog,
			List<RemotePlayer> players,
			object locker,
			List<CellChange> cellChanges,
			List<PlayerStateChange> playerStateChanges,
			Action onGameOver
			)
		{
			_forest = forest;
			_aim = aim;
			_warFog = warFog;
			_players = players;
			_locker = locker;
			_cellChanges = cellChanges;
			_playerStateChanges = playerStateChanges;
			_onGameOver = onGameOver;
		}

		private TerrainType[,] GetVisibleArea(Point location)
		{
			var area = new TerrainType[_warFog * 2 + 1, _warFog * 2 + 1];
			var yMin = location.Y - _warFog;
			var yMax = location.Y + _warFog;
			var xMin = location.X - _warFog;
			var xMax = location.X + _warFog;
			for (var i = yMin; i <= yMax; i++)
				for (var j = xMin; j <= xMax; j++)
				{
					var code = TerrainType.None;
					if (i > 0 && j > 0 && i < _forest.Area.Length && j < _forest.Area[0].Length)
						code = Program.TerrainCode[_forest.Area[i][j].Name];
					area[i - yMin, j - xMin] = code;
				}
			foreach (var player in _players)
			{
				if (player.Inhabitant.Location.InNeighborhood(location, _warFog)
				    && ! player.Inhabitant.Location.Equals(location))
				{
					var enemy = player.Inhabitant.Location;
					area[enemy.Y - yMin, enemy.X - xMin] = TerrainType.PathOrTrap;
				}
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
}