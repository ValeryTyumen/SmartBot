using System;
using System.Net.Sockets;
using ForestInhabitants;
using NetworkHelpers;

namespace PlayerClient
{
	internal class BlindServerTalker : IServerTalker
	{
		public void CommunicateWithServer(string nickname, Socket socket)
		{
			var clientInfo = Bson.Read<ClientInfo>(socket);
			Console.WriteLine("Got client info.");
			var inhabitant = new Inhabitant(nickname, clientInfo.Hp, clientInfo.StartPosition);
			IBlindAi blindAi = new BlindAi();
			foreach (var direction in blindAi.Find(inhabitant, clientInfo.Target, clientInfo.MapSize))
			{
				Bson.Write(socket, new Move
				{
					Direction = direction
				});
				var moveResultInfo = Bson.Read<MoveResultInfo>(socket);
				if (moveResultInfo.Result == 0)
				{
					inhabitant.Location = inhabitant.Location.Add(Forest.Movings[direction]);
				}
				if (moveResultInfo.Result == 2)
					break;
				Console.WriteLine("Went to {0}, result: {1}", direction, moveResultInfo.Result);
			}
		}
	}
}