using System;
using ForestInhabitants;

namespace NetworkHelpers
{
	public class LastMoveInfo
	{
        public bool GameOver;
        public Tuple<Point, int>[] ChangedCells;
        public Tuple<int, Point, int>[] PlayersChangedPosition; // <id, new position, new hp>
    }
}