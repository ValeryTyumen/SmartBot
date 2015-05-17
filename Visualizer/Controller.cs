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
			forest.Place(inhabitant, 2, 1);
			var view = new ForestView(new BasicDrawer());
	        FindPath(forest, inhabitant, view);
        }

	    private static void FindPath(Forest forest, Inhabitant inhabitant, ForestView view)
	    {
		    var pathFinder = new PathFinder(new Ai());
			pathFinder.Moved += () => { 
				view.Repaint(forest, new [] { inhabitant });
				Thread.Sleep(20);
				//Console.ReadKey();
			};
		    pathFinder.Find(forest, inhabitant, new Point(8, 5));
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
