using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestInhabitants
{
	public class Ai : IAi
	{
		private static readonly Dictionary<Point, Direction> Directions = new Dictionary<Point, Direction>()
		{
			{ new Point(0, -1), Direction.Up },
			{ new Point(0, 1), Direction.Down },
			{ new Point(-1, 0), Direction.Left },
			{ new Point(1, 0), Direction.Right }
		};

		private IEnumerable<Direction> ScanNeighbours(Terrain[][] myArea, Inhabitant inhabitant)
		{
			foreach (var direction in Directions.Keys)
			{
				var newPoint = inhabitant.Location.Add(direction);
				if (myArea[newPoint.Y][newPoint.X].Name == "Unknown")
				{
					var lastHealth = inhabitant.Health;
					var lastLocation = inhabitant.Location;
					yield return Directions[direction];
					if (inhabitant.Location.Equals(lastLocation))
						myArea[newPoint.Y][newPoint.X] = new Bush();
					else
					{
						if (inhabitant.Health < lastHealth)
							myArea[newPoint.Y][newPoint.X] = new Trap();
						else
							myArea[newPoint.Y][newPoint.X] = new Path();
						yield return Directions[direction.Negative()];
					}
				}
			}
		}

		private IEnumerable<Point> GetMovesToStart(Dictionary<Point, Point> parents, Point point)
		{
			var current = point;
			while (parents.ContainsKey(current))
			{
				yield return parents[current].Substract(current);
				current = parents[current];
			}
		}

		private IEnumerable<Point> GetMovesFromStart(Dictionary<Point, Point> parents, Point point)
		{
			return GetMovesToStart(parents, point)
				.Select(z => z.Negative())
				.Reverse();
		}

		private IEnumerable<Point> GetPath(Dictionary<Point, Point> parents, Point point1, Point point2)
		{
			var ancestors = new HashSet<Point>();
			var current = point1;
			ancestors.Add(current);
			while (parents.ContainsKey(current))
			{
				current = parents[current];
				ancestors.Add(current);
			}
			var pathToPoint2 = new List<Point>();
			current = point2;
			while (! ancestors.Contains(current))
			{
				pathToPoint2.Add(current.Substract(parents[current]));
				current = parents[current];
			}
			var pathToPoint1 = new List<Point>();
			var commonParent = current;
			current = point1;
			while (! current.Equals(commonParent))
			{
				pathToPoint1.Add(parents[current].Substract(current));
				current = parents[current];
			}
			return pathToPoint1
				.Concat(pathToPoint2
					.Select(z => z)
					.Reverse());
		}

		private IEnumerable<Direction> ScanForest(Terrain[][] myArea, Inhabitant inhabitant, Point aim)
		{
			var front = new List<Point> { inhabitant.Location };
			var parents = new Dictionary<Point, Point>();
			var loop = true;
			while (loop)
			{
				var newFront = new List<Point>();
				for (var i = 0; i < front.Count; i++)
				{
					foreach (var direction in ScanNeighbours(myArea, inhabitant))
					{
						yield return direction;
						if (inhabitant.Location.Equals(aim))
						{
							loop = false;
							break;
						}
						if (! inhabitant.Location.Equals(front[i]))
						{
							newFront.Add(inhabitant.Location);
							parents[inhabitant.Location] = front[i];
						}
					}
					if (!loop) break;
					if (i == front.Count - 1)
					{
						if (newFront.Count != 0)
							foreach (var move in GetPath(parents, front[i], newFront[0]))
								yield return Directions[move];
					}
					else
						foreach (var move in GetPath(parents, front[i], front[i + 1]))
							yield return Directions[move];
				}
				front = newFront;
				if (front.Count == 0) 
					break;
			}
		}

		public IEnumerable<Direction> Find(Inhabitant inhabitant, Point aim, Point forestDimensions)
		{
			var myArea = Enumerable
				.Range(0, forestDimensions.Y)
				.Select(z => new Terrain[forestDimensions.X])
				.ToArray();
			for(var i = 0; i < forestDimensions.Y; i++)
				for (var j = 0; j < forestDimensions.X; j++)
					myArea[i][j] = new Unknown();
			myArea[inhabitant.Location.Y][inhabitant.Location.X] = new Path();
			return ScanForest(myArea, inhabitant, aim);
		}
	}
}
