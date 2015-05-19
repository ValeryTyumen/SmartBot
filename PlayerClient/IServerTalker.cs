using System.Net.Sockets;
using ForestInhabitants;

namespace PlayerClient
{
	interface IServerTalker
	{
		void CommunicateWithServer(string nickname, Socket socket);
	}
}
