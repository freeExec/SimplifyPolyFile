/*
MIT License

Copyright(c) 2017 freeExec | https://github.com/freeExec/SimplifyPolyFile

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Simplify
{
    public class Polygon
    {
        private List<int[]> _nodes;

        public IList<int[]> Nodes { get { return _nodes; } }

        public Polygon()
        {
            _nodes = new List<int[]>();
        }

        public Polygon(IEnumerable<int[]> nodes)
        {
            _nodes = new List<int[]>(nodes);
        }

        // lon => X, lat => Y
        public void AddNode(int lon, int lat)
        {
            _nodes.Add(new int[] { lon, lat });
        }

        public bool IsClose
        {
            get
            {
                if (_nodes.Count < 3) return false;

                var n0 = _nodes[0];
                var nx = _nodes[_nodes.Count - 1];

                return n0[0] == nx[0] && n0[1] == nx[1];
            }
        }

        public void Close()
        {
            if (IsClose) return;

            var n0 = _nodes[0];
            AddNode(n0[0], n0[1]);
        }
    }

    public class PolyFile
    {
        public string Name;
        public List<Polygon> Inner = new List<Polygon>(1);
        public List<Polygon> Outer = new List<Polygon>(2);

        public static PolyFile FromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine("File {0} not found.", filePath);
                throw new FileNotFoundException();
            }

            var polyFile = new PolyFile();
            string[] borderLines = File.ReadAllLines(filePath /*, Encoding.GetEncoding(1251)*/);
            polyFile.Name = borderLines[0];
            Polygon polyline = null;
            bool polygonInner = false;
            for (int i = 1; i < borderLines.Length; i++)
            {
                var line = borderLines[i];
                if (line[0] != ' ')
                {
                    if (line == "END")
                    {
                        if (polyline != null && polyline.IsClose)
                        {
                            if (polygonInner)
                            {
                                polyFile.Inner.Add(polyline);
                            }
                            else
                            {
                                polyFile.Outer.Add(polyline);
                            }
                        }
                        polyline = null;
                        continue;
                    }                    
                    polyline = new Polygon();
                    polygonInner = (line[0] == '!');

                    continue;
                }

                var coords = borderLines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int lon = ((int)Math.Round(Double.Parse(coords[0], System.Globalization.CultureInfo.InvariantCulture) * 10000000));
                int lat = ((int)Math.Round(Double.Parse(coords[1], System.Globalization.CultureInfo.InvariantCulture) * 10000000));

                polyline.AddNode(lon, lat);
            }

            return polyFile;
        }

        public void Save(string filePath)
        {
            var backupCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            using (var writer = new StreamWriter(filePath))
            {
                writer.NewLine = "\n";
                writer.WriteLine(Name);

                int counter = 1;
                foreach(var polygon in Outer)
                {
                    WriteRing(writer, counter, false, polygon);
                    counter++;
                }
                foreach (var polygon in Inner)
                {
                    WriteRing(writer, counter, true, polygon);
                    counter++;
                }
                writer.WriteLine("END");
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = backupCulture;
        }

        private static void WriteRing(StreamWriter writer, int counter, bool inner, Polygon polygon)
        {
            if (inner) writer.Write('!');
            writer.WriteLine(counter);

            foreach (var node in polygon.Nodes)
            {
                writer.Write("   ");
                writer.Write(node[0] / 10000000d);
                writer.Write(" ");
                writer.WriteLine(node[1] / 10000000d);
            }

            writer.WriteLine("END");
        }
    }
}
