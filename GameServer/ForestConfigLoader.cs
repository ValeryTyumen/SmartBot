using System.IO;
using Newtonsoft.Json;

namespace GameServer
{
	internal class ForestConfigLoader
	{
		private const string ConfigSource = "config.txt";

		public static ForestConfig Load()
		{
			var data = File.ReadAllText(ConfigSource);
			return JsonConvert.DeserializeObject<ForestConfig>(data);
		}
	}
}