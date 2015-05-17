namespace ForestInhabitants
{
	public class Bush : Terrain
	{
		public Bush()
		{
			Name = "Bush";
		}

		public override MovementResult Interact(Inhabitant inhabitant, Direction direction)
		{
			return new MovementResult(this, Direction.Stay);
		}
	}
}