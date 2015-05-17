using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForestInhabitants;

namespace GameServer
{
	public class Hello
	{
		public bool IsVisualizator;
		public string Name;
	}

	#region Работа с игроком

	public enum TerrainType
	{
		None,
		Path,
		Bush,
		Trap,
		Life
	}

	public class ClientInfo
	{
		public Point MapSize; // x - height, y - width
		public int Hp;
		public Point StartPosition;
		public Point Target;
		public TerrainType[,] VisibleMap; // видимая часть карты в начале игры.
	}

	public class Move
	{
		public Direction Direction;
	}

	public class MoveResultInfo
	{
		public int Result; // 2 -- GameOver.
		public TerrainType[,] VisibleMap;
	}

	#endregion

	#region Работа с визуализатором


	public class WorldInfo
	{
		public Player[] Players;
		public TerrainType[,] Map;
	}

	public class Answer
	{
		public int AnswerCode;
	}

	public class CellChange
	{
		public Point Location;
		public TerrainType Type;
	}

	public class PlayerStateChange
	{
		public int Id;
		public Point Location;
		public int Hp;
	}

	public class LastMoveInfo
	{
		public bool GameOver;
		public CellChange[] CellChanges;
		public PlayerStateChange[] PlayerStateChanges; // <id, new position, new hp>
	}


	#endregion


	/*	public class Point
	{
		public Point(int x, int y)
		{
			X = x;
			Y = y;
		}

		public int X;
		public int Y;
	}
*/
	public class Player
	{
		public Player(int id, string nick, Point startPos, Point target, int hp)
		{
			Id = id;
			Nick = nick;
			StartPosition = startPos;
			Target = target;
			Hp = hp;
		}

		public int Id;
		public string Nick;
		public int Hp;
		public Point StartPosition;
		public Point Target;
	}
}
