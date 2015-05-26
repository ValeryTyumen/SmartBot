using NUnit.Framework;
using ForestInhabitants;

namespace TestForestInhabitants
{
	[TestFixture]
	class PathFinder_should
	{
		public void FindAim(string filename, Point inhabitantLocation, Point aim)
		{
			var forest = ForestLoader.Load(filename);
			var inhabitant = new Inhabitant("Spider-man");
			forest.Place(inhabitant, inhabitantLocation.X, inhabitantLocation.Y);
			var finder = new PathFinder(new BlindAi());
			finder.Find(forest, inhabitant, aim);
			Assert.AreEqual(aim, inhabitant.Location);
		}

		[Test]
		public void find_aim1()
		{
			FindAim("forest.txt", new Point(2, 1), new Point(3, 1));
		}

		[Test]
		public void find_aim2()
		{
			FindAim("forest1.txt", new Point(2, 1), new Point(8, 5));
		}

		[Test]
		public void find_aim3()
		{
			FindAim("forest2.txt", new Point(2, 1), new Point(1, 4));
		}
	}
}
