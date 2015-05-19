using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestInhabitants
{
	public abstract class Terrain : ICell
    {
        public string Name;
        public abstract MovementResult Interact(Inhabitant inhabitant, Direction direction);
    }
}
