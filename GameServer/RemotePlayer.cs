using System.Net.Sockets;
using ForestInhabitants;

namespace GameServer
{
	internal class RemotePlayer
	{
		public Inhabitant Inhabitant { get; private set; }
		public Socket Socket { get; private set; }

		public RemotePlayer(Inhabitant inhabitant, Socket socket)
		{
			Inhabitant = inhabitant;
			Socket = socket;
		}
	}
}