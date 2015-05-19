using System;

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

		public bool InNeighborhood(Point point, int radius)
		{
			return Math.Abs(X - point.X) <= radius && Math.Abs(Y - point.Y) <= radius;
		}

		public static int GetSquaredDistance(Point point1, Point point2)
		{
			return (point1.X - point2.X) * (point1.X - point2.X) + (point1.Y - point2.Y) * (point1.Y - point2.Y);
		}

		public static int GetManhattanDistance(Point point1, Point point2)
		{
			return Math.Abs(point1.X - point2.X) + Math.Abs(point1.Y - point2.Y);
		}
	}
}