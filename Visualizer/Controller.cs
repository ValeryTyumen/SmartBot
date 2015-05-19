using System;
using System.Threading;
using System.Collections.Generic;
using ForestInhabitants;

namespace Visualizer
{
	class Controller
	{
		static void Main(string[] args)
		{
			var forest = ForestLoader.Load("forest.txt");
			var inhabitant = new Inhabitant("Spider-man");
			forest.Place(inhabitant, 1, 1);
			var aim = new Point(28, 26);
			var view = new ForestView(new BasicDrawer());
			//FindPath(forest, inhabitant, view);
			FindPathWithSmartAi(2, forest, inhabitant, aim, view);
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

		private static Terrain[][] GetVisibleArea(int warFog, Point location, Forest forest)
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
					result[warFog + y - location.Y][warFog + x - location.X] = forest.Area[y][x];
			return result;
		}

		private static void FindPathWithSmartAi(int warFog, Forest forest, Inhabitant inhabitant, Point aim, ForestView view)
		{
			var ai = new SmartAi(
				inhabitant, 
				aim,
				new Point(forest.Area[0].Length, forest.Area.Length));
			var visibleArea = GetVisibleArea(warFog, inhabitant.Location, forest);
			ai.ReceiveMoveResult(visibleArea);
			while (! inhabitant.Location.Equals(aim))
			{
				view.Repaint(forest, new[] { inhabitant });
				Thread.Sleep(20);
				var direction = ai.MakeStep();
				forest.Move(inhabitant, direction);
				visibleArea = GetVisibleArea(warFog, inhabitant.Location, forest);
				ai.ReceiveMoveResult(visibleArea);
				if (inhabitant.Health <= 0)
				{
					Console.WriteLine("Dead!");
					break;
				}
			}
			Console.ReadKey();
		}

		static void ControlLoop(Forest forest, Inhabitant inhabitant, ForestView view)
		{
			var loop = true;
			var actions = new Dictionary<ConsoleKey, Action>()
            {
                { ConsoleKey.UpArrow, () => forest.Move(inhabitant, Direction.Up) },
                { ConsoleKey.DownArrow, () => forest.Move(inhabitant, Direction.Down) },
                { ConsoleKey.LeftArrow, () => forest.Move(inhabitant, Direction.Left) },
                { ConsoleKey.RightArrow, () => forest.Move(inhabitant, Direction.Right) },
                { ConsoleKey.Escape, () => { loop = false; } }
            };
			while (loop)
			{
				view.Repaint(forest, new[] { inhabitant });
				var key = Console.ReadKey().Key;
				actions[key]();
			}
		}
	}
}
