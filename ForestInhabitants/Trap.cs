namespace ForestInhabitants
{
	public class Trap : Terrain
	{
		public Trap()
		{
			Name = "Trap";
		}

		public override MovementResult Interact(Inhabitant inhabitant, Direction direction)
		{
			var result = new MovementResult(this, direction);
			inhabitant.Health--;
			return result;
		}
	}
}