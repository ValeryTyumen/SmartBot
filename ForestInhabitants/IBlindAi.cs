using System.Collections.Generic;

namespace ForestInhabitants
{
	public interface IBlindAi
	{
		IEnumerable<Direction> Find(Inhabitant inhabitant, Point aim, Point forestDimensions);
	}
}