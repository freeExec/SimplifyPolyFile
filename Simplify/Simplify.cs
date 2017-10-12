/*
MIT License

Copyright (c) 2017 freeExec | https://github.com/freeExec/SimplifyPolyFile

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

using System.Collections.Generic;

namespace Simplify
{
    /// <summary>
    /// Simplify using Douglas-Peucker algorithm
    /// </summary>
    public class Simplify
    {
        private struct Segment
        {
            public int Start;
            public int Finish;

            public Segment(int start, int finish)
            {
                Start = start;
                Finish = finish;
            }
        }

        private static int[] normatToBase = new int[2];
        private static double GetSqHeightTriangle(int[] vertex, int[] baseTriangleB, int[] baseTriangleA)
        {
            double dxAB = baseTriangleB[0] - baseTriangleA[0];
            double dyAB = baseTriangleB[1] - baseTriangleA[1];

            if (dxAB != 0.0d || dyAB != 0.0d)
            {
                double k = ((vertex[0] - baseTriangleA[0]) * dxAB + (vertex[1] - baseTriangleA[1]) * dyAB) / (dxAB * dxAB + dyAB * dyAB);

                if (k > 1.0d)
                {
                    normatToBase[0] = baseTriangleB[0];
                    normatToBase[1] = baseTriangleB[1];
                }
                else if (k > 0.0d)
                {
                    normatToBase[0] = baseTriangleA[0] + (int)(dxAB * k);
                    normatToBase[1] = baseTriangleA[1] + (int)(dyAB * k);
                }
            }

            dxAB = vertex[0] - normatToBase[0];
            dyAB = vertex[1] - normatToBase[1];

            return dxAB * dxAB + dyAB * dyAB;
        }

        public static List<int[]> SimplifyDouglasPeuckerPolyline(IList<int[]> nodes, double tolerance)
        {
            double sqTolerance = tolerance * tolerance;

            var bitMask = new System.Collections.BitArray(nodes.Count);

            bitMask[0] = true;
            bitMask[nodes.Count - 1] = true;

            var stack = new Stack<Segment>(nodes.Count / 1000);
            stack.Push(new Segment(0, nodes.Count - 1));

            while(stack.Count > 0)
            {
                var segment = stack.Pop();

                int indexMax = -1;
                double maxSqDistance = 0f;

                for (int i = segment.Start + 1; i < segment.Finish; i++)
                {
                    double sqDistance = GetSqHeightTriangle(nodes[i], nodes[segment.Start], nodes[segment.Finish]);

                    if (sqDistance > maxSqDistance)
                    {
                        indexMax = i;
                        maxSqDistance = sqDistance;
                    }
                }

                if (maxSqDistance > sqTolerance)
                {
                    bitMask[indexMax] = true;

                    stack.Push(new Segment(segment.Start, indexMax));
                    stack.Push(new Segment(indexMax, segment.Finish));
                }
            }

            int actualNodesCount = 0;
            for (int i = 0; i < bitMask.Count; i++)
            {
                if (bitMask[i]) actualNodesCount++;
            }

            var result = new List<int[]>(actualNodesCount);
            for (int i = 0; i < bitMask.Count; i++)
            {
                if (bitMask[i]) result.Add(nodes[i]);
            }

            return result;
        }
    }
}
