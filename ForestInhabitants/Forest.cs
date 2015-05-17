using System;
using System.Collections.Generic;
using System.Linq;


namespace ForestInhabitants
{
	public class Point
	{
		public int X { get; private set; }
		public int Y { get; private set; }

		public Point(int x, int y)
		{
			X = x;
			Y = y;
		}

		public Point Add(Point other)
		{
			return new Point(X + other.X, Y + other.Y);
		}

		public Point Negative()
		{
			return new Point(-X, -Y);
		}

		public Point Substract(Point other)
		{
			return Add(other.Negative());
		}

		public bool IsZero()
		{
			return Equals(new Point(0, 0));
		}
		
		public override bool Equals(object obj)
		{
			if (obj.GetType() != GetType())
				return false;
			var point = (Point)obj;
			return point.X == X && point.Y == Y;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}
	}

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

        private static readonly Dictionary<char, TerrainFactory> factories = new Dictionary<char, TerrainFactory>()
        {
            {'0', new TerrainFactory<Path>()},
            {'1', new TerrainFactory<Bush>()},
            {'K', new TerrainFactory<Trap>()},
            {'L', new TerrainFactory<Life>()}
        };

        public Terrain[][] Area { get; set; }

        public Forest(IEnumerable<string> lines) : this(lines
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
