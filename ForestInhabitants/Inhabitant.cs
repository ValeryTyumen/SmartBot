namespace ForestInhabitants
{
	public class Inhabitant : ICell
	{
		private const int StartHealth = 3;
		public string Name { get; private set; }
		public int Health { get; set; }
		public Point Location
		{ get; set; }

		public Inhabitant(string name)
		{
			Name = name;
			Health = StartHealth;
			Location = new Point(0, 0);
		}

		public Inhabitant(string name, int health, Point location)
		{
			Name = name;
			Health = health;
			Location = location;
		}

		public void Move(Direction direction)
		{
			Location = Location.Add(Forest.Movings[direction]);
		}

		public void IncreaseHealth()
		{
			Health++;
		}

		public void DecreaseHealth()
		{
			Health--;
		}
	}
}
