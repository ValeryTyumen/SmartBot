using System.Collections.Generic;

namespace ForestInhabitants
{
	public interface IAi
	{
		IEnumerable<Direction> Find(Inhabitant inhabitant, Point aim, Point forestDimensions);
	}
}