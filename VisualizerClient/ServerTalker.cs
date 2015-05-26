using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using ForestInhabitants;
using NetworkHelpers;
using Visualizer;

namespace VisualizerClient
{
	internal class ServerTalker
	{
		private ForestView _view;
		private Forest _forest;
		private List<Inhabitant> _inhabitants;
		private Dictionary<TerrainType, TerrainFactory> _factories;

		public ServerTalker(ForestView view, Forest forest, List<Inhabitant> inhabitants, 
			Dictionary<TerrainType, TerrainFactory> factories)
		{
			_view = view;
			_forest = forest;
			_inhabitants = inhabitants;
			_factories = factories;
		}

		public void CommunicateWithServer(Socket socket)
		{
			while (true)
			{
				_view.Repaint(_forest, _inhabitants.ToArray());
				Thread.Sleep(50);
				try
				{
					Json.Write(socket, new Answer { AnswerCode = 0 });
				} catch { break; }
				var lastMoveInfo = Json.Read<LastMoveInfo>(socket);
				foreach (var change in lastMoveInfo.ChangedCells)
					_forest.Area[change.Item1.Y][change.Item1.X] = _factories[(TerrainType)change.Item2].Create();
				foreach (var change in lastMoveInfo.PlayersChangedPosition)
				{
					_inhabitants[change.Item1].Health = change.Item3;
					_inhabitants[change.Item1].Location = change.Item2;
				}
				if (lastMoveInfo.GameOver)
					break;
			}
			Console.ReadKey();
		}
	}
}