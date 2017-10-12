using System;
using System.IO;

namespace Simplify
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: {0} filename.poly tolerance-degree", AppDomain.CurrentDomain.FriendlyName);
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File {0} not found.", args[0]);
                return;
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            double tolerance;
            if (!Double.TryParse(args[1], out tolerance))
            {
                Console.WriteLine("Tolerance is not a number.");
                return;
            }

            tolerance *= 10000000;

            var outputFilepath = Path.GetFileNameWithoutExtension(Path.GetFullPath(args[0])) + "-simplify.poly";

            var polyFile = PolyFile.FromFile(args[0]);

            int pointsOrig = 0;
            int pointsSimp = 0;

            for (int i = 0; i < polyFile.Outer.Count; i++)
            {
                pointsOrig += polyFile.Outer[i].Nodes.Count;
                var simplify = Simplify.SimplifyDouglasPeuckerPolyline(polyFile.Outer[i].Nodes, tolerance);
                pointsSimp += simplify.Count;
                polyFile.Outer[i] = new Polygon(simplify);
            }
            for (int i = 0; i < polyFile.Inner.Count; i++)
            {
                pointsOrig += polyFile.Inner[i].Nodes.Count;
                var simplify = Simplify.SimplifyDouglasPeuckerPolyline(polyFile.Inner[i].Nodes, tolerance);
                pointsSimp += simplify.Count;
                polyFile.Inner[i] = new Polygon(simplify);
            }

            polyFile.Save(outputFilepath);

            Console.WriteLine("Points: {0} -> {1} ({2:P0})", pointsOrig, pointsSimp, pointsSimp / (float)pointsOrig);
        }
    }
}
