using System;
using System.Collections.Generic;
using System.Linq;


namespace ForestInhabitants
{
	public class Forest
	{
		public static readonly Dictionary<Direction, Point> Movings =
			new Dictionary<Direction, Point>()
			{
				{ Direction.Stay, new Point(0, 0) },
				{ Direction.Up, new Point(0, -1)},
				{ Direction.Down, new Point(0, 1) },
				{ Direction.Left, new Point(-1, 0) },
				{ Direction.Right, new Point(1, 0) }
			};

		public static readonly Dictionary<Point, Direction> Directions = new Dictionary<Point, Direction>()
		{
			{ new Point(0, -1), Direction.Up },
			{ new Point(0, 1), Direction.Down },
			{ new Point(-1, 0), Direction.Left },
			{ new Point(1, 0), Direction.Right }
		};

		private static readonly Dictionary<char, TerrainFactory> factories = new Dictionary<char, TerrainFactory>()
		{
			{'0', new TerrainFactory<Path>()},
			{'1', new TerrainFactory<Bush>()},
			{'K', new TerrainFactory<Trap>()},
			{'L', new TerrainFactory<Life>()}
		};

		public Terrain[][] Area { get; set; }

		public Forest(IEnumerable<string> lines)
			: this(lines
				.Select(l => l
				.Select(c => factories[c].Create())
				.ToArray())
				.ToArray())
		{
		}

		public Forest(Terrain[][] area)
		{
			Area = area;
		}

		public void Place(Inhabitant inhabitant, int x, int y)
		{
			inhabitant.Location = new Point(x, y);
		}

		public MovementResult Move(Inhabitant inhabitant, Direction direction)
		{
			var intention = Movings[direction];
			var aim = Area[inhabitant.Location.Y + intention.Y][inhabitant.Location.X + intention.X];
			var result = aim.Interact(inhabitant, direction);
			Area[inhabitant.Location.Y + intention.Y][inhabitant.Location.X + intention.X] = result.Change;
			var moving = Movings[result.Direction];
			inhabitant.Location = inhabitant.Location.Add(moving);
			return result;
		}
	}
}
