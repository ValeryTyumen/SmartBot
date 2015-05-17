namespace ForestInhabitants
{
	public class MovementResult
	{
		public Terrain Change { get; private set; }
		public Direction Direction { get; private set; }

		public MovementResult(Terrain change, Direction direction)
		{
			Change = change;
			Direction = direction;
		}
	}
}