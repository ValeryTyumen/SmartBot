using System.Linq;
using ForestInhabitants;

namespace Visualizer
{
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
