namespace ForestInhabitants
{
	internal class Unknown : Terrain
	{
		public Unknown()
		{
			Name = "Unknown";
		}

		public override MovementResult Interact(Inhabitant inhabitant, Direction direction)
		{
			//doesn't matter
			return new MovementResult(this, Direction.Stay);
		}
	}
}