using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BLPDiffer
{
    internal class ConnectedComponentLabeler
    {
        private readonly int _padding;
        private readonly Dictionary<int, List<int>> _linked;
        private int[,] Labels;

        public ConnectedComponentLabeler(int padding)
        {
            _padding = padding;
            _linked = new Dictionary<int, List<int>>();
        }


        public int[,] Label(bool[,] differenceMap)
        {
            int width = differenceMap.GetLength(0);
            int height = differenceMap.GetLength(1);

            Labels = new int[width, height];

            var nextLabel = 1;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!differenceMap[x, y])
                        continue;

                    var neighbors = Neighbors(differenceMap, width, x, y);
                    if (neighbors == null || neighbors.Length == 0)
                    {
                        _linked.Add(nextLabel, new List<int> { nextLabel });
                        Labels[x, y] = nextLabel;
                        nextLabel++;
                    }
                    else
                    {
                        var neighborsLabels = NeighborsLabels(neighbors);
                        Labels[x, y] = neighborsLabels.Min();
                        Array.ForEach(neighborsLabels, label => _linked[label].AddRange(neighborsLabels));
                    }
                }
            }

            // second pass
            var linkedMinMap = _linked.ToDictionary(x => x.Key, x => x.Value.Min());

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var currentLabel = Labels[x, y];
                    if (currentLabel == 0)
                        continue;

                    Labels[x, y] = linkedMinMap[currentLabel];
                }
            }

            return Labels;
        }

        private int[] NeighborsLabels(IEnumerable<Point> neighbors) => neighbors.Select(n => Labels[n.X, n.Y]).ToArray();

        private Point[] Neighbors(bool[,] bitmap, int width, int x, int y) => GenerateNeighbors(width, x, y).Where(p => bitmap[p.X, p.Y]).ToArray();

        private IEnumerable<Point> GenerateNeighbors(int width, int x, int y)
        {
            var points = new List<Point>(_padding * 4);

            int offset;
            for (int counter = 0; counter <= _padding; counter++)
            {
                offset = counter + 1;

                if (x > counter)
                {
                    points.Add(new Point(x - offset, y));
                    if (y > counter)
                        points.Add(new Point(x - offset, y - offset));
                }

                if (y > counter)
                {
                    points.Add(new Point(x, y - offset));
                    if (x < (width - offset))
                        points.Add(new Point(x + offset, y - offset));
                }

            }

            return points;
        }
    }
}
