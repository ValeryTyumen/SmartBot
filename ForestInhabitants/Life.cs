namespace ForestInhabitants
{
	public class Life : Terrain
	{
		public Life()
		{
			Name = "Life";
		}

		public override MovementResult Interact(Inhabitant inhabitant, Direction direction)
		{
			var result = new MovementResult(new Path(), direction);
			inhabitant.IncreaseHealth();
			return result;
		}
	}
}