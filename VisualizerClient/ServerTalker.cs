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
					Bson.Write(socket, new Answer { AnswerCode = 0 });
				} catch { break; }
				var lastMoveInfo = Bson.Read<LastMoveInfo>(socket);
				foreach (var change in lastMoveInfo.CellChanges)
					_forest.Area[change.Location.Y][change.Location.X] = _factories[change.Type].Create();
				foreach (var change in lastMoveInfo.PlayerStateChanges)
				{
					_inhabitants[change.Id].Health = change.Hp;
					_inhabitants[change.Id].Location = change.Location;
				}
				if (lastMoveInfo.GameOver)
					break;
			}
			Console.ReadKey();
		}
	}
}