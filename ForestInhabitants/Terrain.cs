using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestInhabitants
{
    public interface ICell
    {
    }

    public enum Direction
    {
        Stay = 0,
        Up,
        Down,
        Left,
        Right
    }

    public class MovementResult
    {
        public Terrain Change { get; private set; }
        public Direction Direction { get; private set; }

        public MovementResult(Terrain change, Direction direction)
        {
            Change = change;
            Direction = direction;
        }
    }

    public abstract class Terrain : ICell
    {
        public string Name;
        public abstract MovementResult Interact(Inhabitant inhabitant, Direction direction);
    }

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

    public class Bush : Terrain
    {
        public Bush()
        {
            Name = "Bush";
        }

        public override MovementResult Interact(Inhabitant inhabitant, Direction direction)
        {
            return new MovementResult(this, Direction.Stay);
        }
    }

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

    public class Life : Terrain
    {
        public Life()
        {
            Name = "Life";
        }

        public override MovementResult Interact(Inhabitant inhabitant, Direction direction)
        {
            var result = new MovementResult(new Path(), direction);
            inhabitant.Health++;
            return result;
        }
    }

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
