namespace ForestInhabitants
{
	public abstract class TerrainFactory
	{
		abstract public Terrain Create();
	}

	public class TerrainFactory<T> : TerrainFactory where T : Terrain, new()
	{
		public override Terrain Create()
		{
			return new T();
		}
	}
}