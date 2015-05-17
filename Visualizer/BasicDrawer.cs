using System;
using System.Collections.Generic;
using ForestInhabitants;

namespace Visualizer
{
	public class BasicDrawer : ForestDrawer
	{
		private readonly Dictionary<string, char> _signs = new Dictionary<string, char>()
		{
			{"Path", ' '},
			{"Bush", '█'},
			{"Trap", 'X'},
			{"Life", '♥'},
			{"Inhabitant", '☺'}
		};

		protected override void DrawCell(string cellName)
		{
			Console.Write(_signs[cellName]);
		}

		public override void DrawLabels(Inhabitant[] inhabitants)
		{
			Console.WriteLine();
			foreach (var terrainLabel in _signs)
			{
				Console.WriteLine("  " + terrainLabel.Value + " - " + terrainLabel.Key);
			}
			foreach(var inhabitant in inhabitants)
				Console.WriteLine("  " + _signs["Inhabitant"] + " - " + inhabitant.Name + " (" + inhabitant.Health + " lives)");
			Console.WriteLine("  Use [Arrows] to control");
			Console.WriteLine("  Press [ESC] to quit");
		}
	}
}