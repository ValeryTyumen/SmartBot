using System;
using System.Net.Sockets;
using ForestInhabitants;
using NetworkHelpers;

namespace PlayerClient
{
	internal class ServerTalker
	{
		private Inhabitant _inhabitant;

		public ServerTalker(Inhabitant inhabitant)
		{
			_inhabitant = inhabitant;
		}

		public void CommunicateWithServer(Socket socket)
		{
			var clientInfo = Bson.Read<ClientInfo>(socket);
			Console.WriteLine("Got client info.");

			_inhabitant.Location = clientInfo.StartPosition;
			IAi ai = new Ai();
			foreach (var direction in ai.Find(_inhabitant, clientInfo.Target, clientInfo.MapSize))
			{
				Bson.Write(socket, new Move
				{
					Direction = direction
				});
				var moveResultInfo = Bson.Read<MoveResultInfo>(socket);
				if (moveResultInfo.Result == 0)
				{
					_inhabitant.Location = _inhabitant.Location.Add(Forest.Movings[direction]);
				}
				if (moveResultInfo.Result == 2)
					break;
				Console.WriteLine("Went to {0}, result: {1}", direction, moveResultInfo.Result);
			}
		}
	}
}