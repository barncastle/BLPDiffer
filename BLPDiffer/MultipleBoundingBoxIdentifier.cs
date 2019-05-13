using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BLPDiffer
{
    internal class MultipleBoundingBoxIdentifier
    {
        private readonly int _padding;

        public MultipleBoundingBoxIdentifier(int padding)
        {
            _padding = padding;
        }


        public IEnumerable<Rectangle> CreateBoundingBoxes(int[,] labelMap)
        {
            var boundedPoints = FindLabeledPointGroups(labelMap);
            return CreateBoundingBoxes(boundedPoints);
        }

        private IEnumerable<Rectangle> CreateBoundingBoxes(Dictionary<int, List<Point>> boundedPoints)
        {
            if (boundedPoints == null || boundedPoints.Count == 0)
                yield break;

            foreach (var kvp in boundedPoints)
            {
                var points = kvp.Value;
                var minPoint = new Point(points.Min(x => x.X), points.Min(y => y.Y));
                var maxPoint = new Point(points.Max(x => x.X), points.Max(y => y.Y));

                yield return new Rectangle(minPoint.X - _padding,
                                           minPoint.Y - _padding,
                                           maxPoint.X - minPoint.X + (_padding * 2),
                                           maxPoint.Y - minPoint.Y + (_padding * 2));
            }
        }

        private static Dictionary<int, List<Point>> FindLabeledPointGroups(int[,] labelMap)
        {
            var width = labelMap.GetLength(0);
            var height = labelMap.GetLength(1);

            var boundedPoints = new Dictionary<int, List<Point>>(width * height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (labelMap[x, y] == 0)
                        continue;

                    var label = labelMap[x, y];

                    if (!boundedPoints.ContainsKey(label))
                        boundedPoints.Add(label, new List<Point>());

                    boundedPoints[label].Add(new Point(x, y));
                }
            }

            boundedPoints.TrimExcess();
            return boundedPoints;
        }
    }
}
