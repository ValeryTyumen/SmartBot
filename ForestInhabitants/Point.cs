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
}