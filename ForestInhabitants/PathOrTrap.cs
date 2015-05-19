using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestInhabitants
{
	public class PathOrTrap : Terrain
	{
		public PathOrTrap()
		{
			Name = "PathOrTrap";
		}

		public override MovementResult Interact(Inhabitant inhabitant, Direction direction)
		{
			throw new NotImplementedException();
		}
	}
}
