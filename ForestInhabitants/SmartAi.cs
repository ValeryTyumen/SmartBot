using System;
using System.Collections.Generic;
using System.Linq;

namespace ForestInhabitants
{
	public interface IAi
	{
		void ReceiveMoveResult(Terrain[][] visibleArea);
		Direction MakeStep();
	}

	public class SmartAi : IAi
	{
		private Inhabitant _inhabitant;
		private Point _aim;
		private Point _enemyLocation;
		private Terrain[][] _area;
		private HashSet<Point> PointsOfInterest;
		private HashSet<Point> PointsOfHighInterest;
		private Point _subAim = null;
		private const int LifeNeighborhood = 3;

		private bool OnMovingToLife = false;
		private Point _lifeAim = null; //life aim can leave
		private List<Point> CurrentPath;
		private int StepIndex;

		public SmartAi(Inhabitant inhabitant, Point aim, Point forestDimensions)
		{
			_inhabitant = inhabitant;
			_aim = aim;
			_area = new Terrain[forestDimensions.Y][];
			for (int i = 0; i < forestDimensions.Y; i++)
			{
				_area[i] = new Terrain[forestDimensions.X];
				for (int j = 0; j < forestDimensions.X; j++)
					_area[i][j] = null;
			}
			PointsOfInterest = new HashSet<Point>();
			PointsOfHighInterest = new HashSet<Point>();
		}

		private bool InsideArea(int x, int y)
		{
			return x > -1 && y > -1 && y < _area.Length && x < _area[0].Length;
		}

		private IEnumerable<Point> GetNeighbors(Point point, Func<Point, bool> pointCondition)
		{
			var dx = new[] { 0, 1, 0, -1 };
			var dy = new[] { 1, 0, -1, 0 };
			for (var i = 0; i < 4; i++)
				if (InsideArea(point.X + dx[i], point.Y + dy[i]))
					if (pointCondition(new Point(point.X + dx[i], point.Y + dy[i])))
						yield return new Point(point.X + dx[i], point.Y + dy[i]);
		}

		private Dictionary<Point, Point> BreadsFirstSearch(Point start, Func<Point, bool> addCondition,
			Func<Point, bool> finishCondition)
		{
			var parents = new Dictionary<Point, Point>();
			var queue = new Queue<Point>();
			queue.Enqueue(start);
			parents[start] = null;
			while (queue.Count != 0)
			{
				var current = queue.Dequeue();
				if (finishCondition(current))
					return parents;
				foreach (var neighbor in GetNeighbors(current, n => addCondition(n) && ! parents.ContainsKey(n)))
				{
					queue.Enqueue(neighbor);
					parents[neighbor] = current;
				}
			}
			return parents;
		}

		private List<Point> GetPathFromParents(Tuple<Point, int> finishState,
			Dictionary<Tuple<Point, int>, Tuple<Point, int>> parents)
		{
			var result = new List<Point>();
			var current = finishState;
			while (current != null)
			{
				result.Add(current.Item1);
				current = parents[current];
			}
			result.Reverse();
			return result;
		}

		private List<Point> GetPathFromParents(Point finish, Dictionary<Point, Point> parents)
		{
			var result = new List<Point>();
			var current = finish;
			while (current != null)
			{
				result.Add(current);
				current = parents[current];
			}
			result.Reverse();
			return result;
		}

		private List<Point> FindShortestPath(Point start, Point finish, int trapsAvailable)
		{
			var parents = new Dictionary<Tuple<Point, int>, Tuple<Point, int>>();
			var queue = new Queue<Tuple<Point, int>>();
			var startState = Tuple.Create(start, 0);
			parents[startState] = null;
			queue.Enqueue(startState);
			while (queue.Count != 0)
			{
				var current = queue.Dequeue();
				if (current.Item1.Equals(finish))
				{
					return GetPathFromParents(current, parents);
				}
				foreach (var neighbor in GetNeighbors(current.Item1, p => 
					_area[p.Y][p.X] != null 
					&& _area[p.Y][p.X].Name != "Bush" 
					//&& _area[p.Y][p.X].Name != "PathOrTrap"
					))
				{
					var trapsPassed = current.Item2;
					if (_area[neighbor.Y][neighbor.X].Name == "Trap")
						trapsPassed++;
					if (trapsPassed > trapsAvailable)
						continue;
					var state = Tuple.Create(neighbor, trapsPassed);
					if (!parents.ContainsKey(state))
					{
						queue.Enqueue(state);
						parents[state] = current;
					}
				}
			}
			return null;
		}

		private List<Point> FindShortestPathWithMinTraps(Point start, Point finish) // if path doesn't exist, infinite loop takes place
		{
			var trapsAvailable = 0;
			while (true)
			{
				var result = FindShortestPath(start, finish, trapsAvailable);
				if (result == null)
					trapsAvailable++;
				else
					return result;
			}
		}

		private List<Point> FindPathToLifeNear()
		{
			var parents = BreadsFirstSearch(_inhabitant.Location, n =>
			{
				var terrain = _area[n.Y][n.X];
				return terrain != null 
				       && terrain.Name != "Bush"
				       && terrain.Name != "Trap"
				       && terrain.Name != "PathOrTrap"
				       && _inhabitant.Location.InNeighborhood(n, LifeNeighborhood);
			}, n => false);
			for (int y = _inhabitant.Location.Y - LifeNeighborhood; y <= _inhabitant.Location.Y + LifeNeighborhood; y++)
				for (int x = _inhabitant.Location.X - LifeNeighborhood; x <= _inhabitant.Location.X + LifeNeighborhood; x++)
					if (InsideArea(x, y) && _area[y][x] != null && _area[y][x].Name == "Life" && parents.ContainsKey(new Point(x, y)))
						return GetPathFromParents(new Point(x, y), parents);
			return null;
		}

		private bool LifeUnreachable()
		{
			for (int i = StepIndex + 1; i < CurrentPath.Count; i++)
				if (_area[CurrentPath[i].Y][CurrentPath[i].X].Name == "PathOrTrap")
					return true;
			return false;
		}

		private Point ChooseSubAim()
		{
			var parents = BreadsFirstSearch(_inhabitant.Location, v =>
			{
				var terrain = _area[v.Y][v.X];
				return terrain != null
				       && terrain.Name != "Bush";
				//&& terrain.Name != "PathOrTrap";
			}, v => false);
			if (PointsOfHighInterest.Count != 0)
				foreach (var point in PointsOfHighInterest.OrderBy(z => Point.GetManhattanDistance(_aim, z)))
					if (parents.ContainsKey(point))
						return point;
			foreach (var point in PointsOfInterest.OrderBy(z => Point.GetManhattanDistance(_aim, z)))
				if (parents.ContainsKey(point))
					return point;
			return null;
		}

		public Direction MakeStep()
		{
			if (_inhabitant.Location.Equals(_aim))
				return Direction.Stay;
			if (CurrentPath != null)
			{
				if (! CurrentPath[StepIndex].Equals(_inhabitant.Location))
				{
					_subAim = null;
				}
			}
			if (_lifeAim != null)
				if (_inhabitant.Location.Equals(_lifeAim) || LifeUnreachable())
					_lifeAim = null;
			if (_lifeAim == null)
			{
				var path = FindPathToLifeNear();
				if (path != null)
				{
					CurrentPath = path;
					StepIndex = 0;
					_lifeAim = path[path.Count - 1];
					_subAim = null;
				}
			}
			if (_lifeAim == null)
			{
				if (_subAim == null || _inhabitant.Location.Equals(_subAim))
				{
					_subAim = ChooseSubAim();
					if (_subAim == null)
						return Direction.Stay;
					CurrentPath = FindShortestPathWithMinTraps(_inhabitant.Location, _subAim); //can create mistake
					StepIndex = 0;
				}
				if (_subAim != null && _area[CurrentPath[StepIndex + 1].Y][CurrentPath[StepIndex + 1].X].Name == "PathOrTrap")
					return Direction.Stay;
			}
			StepIndex++;
			return Forest.Directions[CurrentPath[StepIndex].Substract(CurrentPath[StepIndex - 1])];
		}

		private void UpdateArea(Terrain[][] visibleArea)
		{
			var warFog = visibleArea.Length / 2;
			for (var y = 0; y < visibleArea.Length; y++)
				for (var x = 0; x < visibleArea[0].Length; x++)
				{
					var yOnArea = _inhabitant.Location.Y - warFog + y;
					var xOnArea = _inhabitant.Location.X - warFog + x;
					if (InsideArea(xOnArea, yOnArea))
					{
						_area[yOnArea][xOnArea] = visibleArea[y][x];
						/*if (_area[yOnArea][xOnArea] == null)
							_area[yOnArea][xOnArea] = visibleArea[y][x];
						else if (_area[yOnArea][xOnArea].Name == "PathOrTrap")
							_area[yOnArea][xOnArea] = visibleArea[y][x];
						if (_area[yOnArea][xOnArea].Name == "Life")
							_area[yOnArea][xOnArea] = visibleArea[y][x];*/
					}
				}
		}

		private bool IsBorderPoint(int x, int y)
		{
			return GetNeighbors(new Point(x, y), v => _area[v.Y][v.X] == null).Count() != 0;
		}

		private void UpdatePointsOfInterest(Terrain[][] visibleArea)
		{
			var buffer = PointsOfHighInterest;
			PointsOfHighInterest = new HashSet<Point>();
			PointsOfHighInterest.Add(_aim);
			var warFog = visibleArea.Length / 2;
			var parents = BreadsFirstSearch(_inhabitant.Location, v =>
			{
				var terrain = _area[v.Y][v.X];
				return terrain != null
				       && terrain.Name != "Bush"
				       && terrain.Name != "PathOrTrap"
				       && _inhabitant.Location.InNeighborhood(v, warFog);
			}, v => false);
			for (var y = 0; y < visibleArea.Length; y++)
				for (var x = 0; x < visibleArea[0].Length; x++)
				{
					var yOnArea = _inhabitant.Location.Y - warFog + y;
					var xOnArea = _inhabitant.Location.X - warFog + x;
					if (InsideArea(xOnArea, yOnArea) && parents.ContainsKey(new Point(xOnArea, yOnArea)) &&
							(IsBorderPoint(xOnArea, yOnArea)))
						PointsOfHighInterest.Add(new Point(xOnArea, yOnArea));
				}
			foreach (var point in buffer)
				if (! PointsOfHighInterest.Contains(point))
					PointsOfInterest.Add(point);
			buffer = PointsOfInterest;
			PointsOfInterest = new HashSet<Point>();
			foreach (var point in buffer)
				if (IsBorderPoint(point.X, point.Y))
					PointsOfInterest.Add(point);
		}

		public void ReceiveMoveResult(Terrain[][] visibleArea)
		{
			UpdateArea(visibleArea);
			UpdatePointsOfInterest(visibleArea);
		}
	}
}
