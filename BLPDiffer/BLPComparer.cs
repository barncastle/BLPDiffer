using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using BLPDiffer.Analysers;
using SereniaBLPLib;

namespace BLPDiffer
{
    public sealed class BLPComparer
    {
        private readonly AnalyserType _analyserType;
        private readonly BLPComparerSettings _settings;

        public BLPComparer(AnalyserType analyserType, BLPComparerSettings settings = null)
        {
            _analyserType = analyserType;
            _settings = settings ?? new BLPComparerSettings();

            _settings.Validate(_analyserType);
        }


        public Bitmap Compare(string previous, string current)
        {
            using (var pStream = File.OpenRead(previous))
            using (var cStream = File.OpenRead(current))
                return Compare(new BlpFile(pStream), new BlpFile(cStream));
        }

        public Bitmap Compare(BlpFile previous, BlpFile current)
        {
            if (previous == null)
                throw new ArgumentNullException(nameof(previous));
            if (current == null)
                throw new ArgumentNullException(nameof(current));

            using (var pImg = previous.GetBitmap(previous.MipMapCount - 1))
            using (var cImg = current.GetBitmap(current.MipMapCount - 1))
            {
                if (pImg.Width != cImg.Width || pImg.Height != cImg.Height)
                    throw new ArgumentException("Images must be the same size");

                var differ = AnalyserFactory.Create(_analyserType, _settings.NoticeableDifference);
                var labeler = new ConnectedComponentLabeler(_settings.LabelerPadding);
                var boxer = new MultipleBoundingBoxIdentifier(_settings.BoundingBoxPadding);

                var differenceMap = differ.Analyse(pImg, cImg);
                var labels = labeler.Label(differenceMap);
                var boundingBoxes = boxer.CreateBoundingBoxes(labels);
                return GenerateOutput(cImg, boundingBoxes);
            }
        }

        private Bitmap GenerateOutput(Bitmap current, IEnumerable<Rectangle> boundingBoxes)
        {
            Bitmap bmp = (Bitmap)current.Clone();

            if (!boundingBoxes.Any())
                return bmp;

            using (var g = Graphics.FromImage(bmp))
            {
                var pen = new Pen(_settings.BoundingBoxColour);

                foreach (var boundingRectangle in boundingBoxes)
                {
                    switch (_settings.BoundingBoxMode)
                    {
                        case BoundingBoxDrawMode.Highlight:
                            g.FillRectangle(pen.Brush, boundingRectangle);
                            break;
                        default:
                            g.DrawRectangle(pen, boundingRectangle);
                            break;
                    }
                }
            }

            return bmp;
        }
    }
}
