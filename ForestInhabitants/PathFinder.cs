using System;

namespace ForestInhabitants
{
	public class PathFinder
	{
		public event Action Moved;
		private readonly IAi _ai;

		public PathFinder(IAi ai)
		{
			_ai = ai;
			Moved = () => { };
		}

		public void Find(Forest forest, Inhabitant inhabitant, Point aim)
		{
			var dimensions = new Point(forest.Area[0].Length, forest.Area.Length);
			foreach (var direction in _ai.Find(inhabitant, aim, dimensions))
			{
				forest.Move(inhabitant, direction);
				Moved();
			}
		}
	}
}