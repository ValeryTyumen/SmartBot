namespace ForestInhabitants
{
	public class Path : Terrain
	{
		public Path()
		{
			Name = "Path";
		}

		public override MovementResult Interact(Inhabitant inhabitant, Direction direction)
		{
			var result = new MovementResult(this, direction);
			return result;
		}
	}
}