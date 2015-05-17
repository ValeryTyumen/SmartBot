using ForestInhabitants;

namespace NetworkHelpers
{
	public class Player
	{
		public Player(int id, string nick, Point startPos, Point target, int hp)
		{
			Id = id;
			Nick = nick;
			StartPosition = startPos;
			Target = target;
			Hp = hp;
		}

		public int Id;
		public string Nick;
		public int Hp;
		public Point StartPosition;
		public Point Target;
	}
}