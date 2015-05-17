using ForestInhabitants;

namespace NetworkHelpers
{
	public class ClientInfo
	{
		public Point MapSize; // x - height, y - width
		public int Hp;
		public Point StartPosition;
		public Point Target;
		public TerrainType[,] VisibleMap; // видимая часть карты в начале игры.
	}
}