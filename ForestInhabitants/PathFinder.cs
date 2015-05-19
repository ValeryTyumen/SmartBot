using System;

namespace ForestInhabitants
{
	public class PathFinder
	{
		public event Action Moved;
		private readonly IBlindAi _blindAi;

		public PathFinder(IBlindAi blindAi)
		{
			_blindAi = blindAi;
			Moved = () => { };
		}

		public void Find(Forest forest, Inhabitant inhabitant, Point aim)
		{
			var dimensions = new Point(forest.Area[0].Length, forest.Area.Length);
			foreach (var direction in _blindAi.Find(inhabitant, aim, dimensions))
			{
				forest.Move(inhabitant, direction);
				Moved();
			}
		}
	}
}