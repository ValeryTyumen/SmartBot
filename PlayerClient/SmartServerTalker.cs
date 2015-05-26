using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ForestInhabitants;
using NetworkHelpers;

namespace PlayerClient
{
	class SmartServerTalker : IServerTalker
	{
		private static readonly Dictionary<TerrainType, TerrainFactory> factories = new Dictionary<TerrainType, TerrainFactory>()
		{
			{TerrainType.Path, new TerrainFactory<Path>()},
			{TerrainType.Bush, new TerrainFactory<Bush>()},
			{TerrainType.Trap, new TerrainFactory<Trap>()},
			{TerrainType.Life, new TerrainFactory<Life>()},
			{TerrainType.PathOrTrap, new TerrainFactory<PathOrTrap>()}
		};

		private Terrain[][] Convert(int[,] visibleMap)
		{
			var result = new Terrain[visibleMap.GetLength(0)][];
			for (var i = 0; i < visibleMap.GetLength(0); i++)
			{
				result[i] = new Terrain[visibleMap.GetLength(1)];
				for (var j = 0; j < visibleMap.GetLength(1); j++)
					if (visibleMap[i, j] == (int)TerrainType.None)
						result[i][j] = null;
					else
						result[i][j] = factories[(TerrainType)visibleMap[i, j]].Create();
			}
			return result;
		}

		public void CommunicateWithServer(string nickname, Socket socket)
		{
			var clientInfo = Json.Read<ClientInfo>(socket);
			Console.WriteLine("Got client info.");
			var inhabitant = new Inhabitant(nickname, clientInfo.Hp, clientInfo.StartPosition);
			IAi ai = new SmartAi(inhabitant, clientInfo.Target, clientInfo.MapSize);
			ai.ReceiveMoveResult(Convert(clientInfo.VisibleMap));
			while (true)
			{
				var direction = ai.MakeStep();
				Json.Write(socket, new Move
				{
					Direction = direction
				});
				var moveResultInfo = Json.Read<MoveResultInfo>(socket);
				if (moveResultInfo.Result == 0)
					inhabitant.Location = inhabitant.Location.Add(Forest.Movings[direction]);
				if (moveResultInfo.Result == 2)
					break;
				Console.WriteLine("Went to {0}, result: {1}", direction, moveResultInfo.Result);
			}
		}
	}
}
