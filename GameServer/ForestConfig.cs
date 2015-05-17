using ForestInhabitants;

namespace GameServer
{
	internal class ForestConfig
	{
		public string ForestSource { get; private set; }
		public Point Aim { get; private set; }

		public ForestConfig(string forestSource, Point aim)
		{
			ForestSource = forestSource;
			Aim = aim;
		}
	}
}