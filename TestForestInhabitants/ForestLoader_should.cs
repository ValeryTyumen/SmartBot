using ForestInhabitants;
using NUnit.Framework;

namespace TestForestInhabitants
{
    [TestFixture]
    class ForestLoader_should
    {
        [Test]
        public void load_forest()
        {
            var forest = ForestLoader.Load("forest.txt");

            var area = forest.Area;
            for (var i = 0; i < 5; i++)
            {
                Assert.IsInstanceOf<Bush>(area[0][i]);
                Assert.IsInstanceOf<Bush>(area[2][i]);
            }
            Assert.IsInstanceOf<Bush>(area[1][0]);
            Assert.IsInstanceOf<Trap>(area[1][1]);
            Assert.IsInstanceOf<Path>(area[1][2]);
            Assert.IsInstanceOf<Life>(area[1][3]);
            Assert.IsInstanceOf<Bush>(area[1][4]);
        }
    }
}
