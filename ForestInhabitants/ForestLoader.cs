using System.Collections.Generic;

namespace ForestInhabitants
{
    public class ForestLoader
    {
        public static Forest Load(string file)
        {
            using (var reader = new System.IO.StreamReader(file))
            {
                var lines = new List<string>();
                while (! reader.EndOfStream)
                {
                    lines.Add(reader.ReadLine());
                }
                return new Forest(lines.ToArray());
            }
        }
    }
}
