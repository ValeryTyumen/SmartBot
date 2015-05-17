using System;
using System.Collections.Generic;
using System.Linq;
using ForestInhabitants;

namespace Visualizer
{
    public abstract class ForestDrawer
    {
        public void Clear()
        {
            Console.SetCursorPosition(0, 0);
        }

        protected abstract void DrawCell(string cellName);

        public void DrawArea(string[][] areaNames)
        {
            Console.WriteLine();
            foreach (var line in areaNames)
            {
                Console.Write(" ");
                foreach (var name in line)
                    DrawCell(name);
                Console.WriteLine();
            }
        }

        public abstract void DrawLabels(Inhabitant[] inhabitants);
    }

    public class BasicDrawer : ForestDrawer
    {
        private readonly Dictionary<string, char> _signs = new Dictionary<string, char>()
        {
            {"Path", ' '},
            {"Bush", '█'},
            {"Trap", 'X'},
            {"Life", '♥'},
            {"Inhabitant", '☺'}
        };

        protected override void DrawCell(string cellName)
        {
            Console.Write(_signs[cellName]);
        }

        public override void DrawLabels(Inhabitant[] inhabitants)
        {
            Console.WriteLine();
            foreach (var terrainLabel in _signs)
            {
                Console.WriteLine("  " + terrainLabel.Value + " - " + terrainLabel.Key);
            }
			foreach(var inhabitant in inhabitants)
				Console.WriteLine("  " + _signs["Inhabitant"] + " - " + inhabitant.Name + " (" + inhabitant.Health + " lives)");
            Console.WriteLine("  Use [Arrows] to control");
            Console.WriteLine("  Press [ESC] to quit");
        }
    }

    public class ForestView
    {
        private readonly BasicDrawer _drawer;

        public ForestView(BasicDrawer drawer)
        {
            _drawer = drawer;
        }

        private static string[][] GetAreaNames(Terrain[][] area, Inhabitant[] inhabitants)
        {
            var names = area
                .Select(z => z
                    .Select(x => x.Name)
                    .ToArray())
                .ToArray();
			foreach(var inhabitant in inhabitants)
				names[inhabitant.Location.Y][inhabitant.Location.X] = "Inhabitant";
            return names;
        }

        public void Repaint(Forest forest, Inhabitant[] inhabitants)
        {
            _drawer.Clear();
            _drawer.DrawArea(GetAreaNames(forest.Area, inhabitants));
            _drawer.DrawLabels(inhabitants);
        }
    }
}
