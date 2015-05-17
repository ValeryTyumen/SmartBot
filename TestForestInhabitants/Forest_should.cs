using NUnit.Framework;
using ForestInhabitants;

namespace TestForestInhabitants
{
    [TestFixture]
    class Forest_should
    {

        public static void Main()
        {
        }

        private Forest Forest;
        private Inhabitant Inhabitant;

        [SetUp]
        public void CreateForest()
        {
            Forest = new Forest(new [] {"11111", "1K0L1", "11111"});
            Inhabitant = new Inhabitant("Spider-man");
            Forest.Place(Inhabitant, 2, 1);
        }

        [Test]
        public void have_correct_size()
        {
            Assert.AreEqual(3, Forest.Area.Length);
            for (var i = 0; i < 3; i++)
                Assert.AreEqual(5, Forest.Area[i].Length);
        }

        [Test]
        public void create_cells_correctly()
        {
            var area = Forest.Area;
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

        [Test]
        public void move_inhabitant_onto_trap()
        {
            var expected = Inhabitant.Health - 1;
            Forest.Move(Inhabitant, Direction.Left);
            Assert.AreEqual(expected, Inhabitant.Health);
        }

        [Test]
        public void not_move_inhabitant_onto_bush()
        {
            var x = Inhabitant.Location.X;
            var y = Inhabitant.Location.Y;
            Forest.Move(Inhabitant, Direction.Up);
            Assert.AreEqual(x, Inhabitant.Location.X);
            Assert.AreEqual(y, Inhabitant.Location.Y);
        }

        [Test]
        public void move_inhabitant_onto_life()
        {
            var expected = Inhabitant.Health + 1;
            Forest.Move(Inhabitant, Direction.Right);
            Assert.AreEqual(expected, Inhabitant.Health);
            Assert.IsInstanceOf<Path>(Forest.Area[1][3]);
        }
    }
}
