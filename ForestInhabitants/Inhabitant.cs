using System;

namespace ForestInhabitants
{
    public class Inhabitant : ICell
    {
        private const int StartHealth = 10;
        public string Name { get; set; }
        public int Health { get; set; }
	    private Point location;
		public Point Location
		{ get; set; }

        public Inhabitant(string name)
        {
            Name = name;
            Health = StartHealth;
	        Location = new Point(0, 0);
        }
    }
}
