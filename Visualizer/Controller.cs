using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using ForestInhabitants;

namespace Visualizer
{
	class Controller
	{
		static void Main(string[] args)
		{
			var forest = ForestLoader.Load("forest.txt");
			var inhabitant1 = new Inhabitant("Spider-man", 2, null);
			var inhabitant2 = new Inhabitant("Batman", 2, null);
			forest.Place(inhabitant1, 11, 7);
			forest.Place(inhabitant2, 11, 6);
			var aim = new Point(14, 14);
			var view = new ForestView(new BasicDrawer());
			//FindPath(forest, inhabitant, view);
			FindPathWithSmartAi(1, forest, new [] {inhabitant1, inhabitant2}, aim, view);
		}

		private static void FindPath(Forest forest, Inhabitant inhabitant, ForestView view)
		{
			var pathFinder = new PathFinder(new BlindAi());
			pathFinder.Moved += () =>
			{
				view.Repaint(forest, new[] { inhabitant });
				Thread.Sleep(20);
				//Console.ReadKey();
			};
			pathFinder.Find(forest, inhabitant, new Point(8, 5));
			Console.ReadKey();
		}

		private static Terrain[][] GetVisibleArea(int warFog, Point location, Inhabitant[] inhabitants, Forest forest)
		{
			var forestDimensions = new Point(forest.Area[0].Length, forest.Area.Length);
			var result = new Terrain[warFog * 2 + 1][];
			for (var i = 0; i < warFog * 2 + 1; i++)
				result[i] = new Terrain[warFog * 2 + 1];
			var yMin = Math.Max(0, location.Y - warFog);
			var yMax = Math.Min(forestDimensions.Y - 1, location.Y + warFog);
			var xMin = Math.Max(0, location.X - warFog);
			var xMax = Math.Min(forestDimensions.X - 1, location.X + warFog);
			for (var y = yMin; y <= yMax; y++)
				for (var x = xMin; x <= xMax; x++)
				{
					result[warFog + y - location.Y][warFog + x - location.X] = forest.Area[y][x];
					foreach (var inhabitant in inhabitants)
						if (inhabitant.Location.Equals(new Point(x, y)) 
								&& ! inhabitant.Location.Equals(location))
							result[warFog + y - location.Y][warFog + x - location.X] = new PathOrTrap();
				}
			return result;
		}

		private static bool SomeoneReachedAim(Inhabitant[] inhabitants, Point aim)
		{
			foreach (var inhabitant in inhabitants)
				if (inhabitant.Location.Equals(aim))
					return true;
			return false;
		}

		private static void FindPathWithSmartAi(int warFog, Forest forest, Inhabitant[] inhabitants, Point aim, ForestView view)
		{
			var ais = inhabitants
				.Select(z => new SmartAi(
					z, 
					aim,
					new Point(forest.Area[0].Length, forest.Area.Length)))
				.ToArray();
			var visibleAreas = inhabitants
				.Select(z =>GetVisibleArea(warFog, z.Location, inhabitants, forest))
				.ToArray();
			for (var i = 0; i < inhabitants.Length; i++)
				ais[i].ReceiveMoveResult(visibleAreas[i]);
			while (! SomeoneReachedAim(inhabitants, aim))
			{
				for (var i = 0; i < inhabitants.Length; i++)
					if (inhabitants[i].Health >= 0)
					{
						view.Repaint(forest, inhabitants);
						Thread.Sleep(20);
						var direction = ais[i].MakeStep();
						forest.Move(inhabitants[i], direction);
						if (inhabitants[0].Location.Equals(inhabitants[1].Location))
						{
						}
						var visibleArea = GetVisibleArea(warFog, inhabitants[i].Location, inhabitants, forest);
						ais[i].ReceiveMoveResult(visibleArea);
					}
			}
			Console.ReadKey();
		}
	}
}
