using System;
using ForestInhabitants;

namespace Visualizer
{
	public abstract class ForestDrawer
	{
		public void Clear()
		{
			Console.SetCursorPosition(0, 0);
		}

		protected abstract void DrawCell(string cellName);

		public void DrawArea(string[][] areaNames)
		{
			Console.WriteLine();
			foreach (var line in areaNames)
			{
				Console.Write(" ");
				foreach (var name in line)
					DrawCell(name);
				Console.WriteLine();
			}
		}

		public abstract void DrawLabels(Inhabitant[] inhabitants);
	}
}