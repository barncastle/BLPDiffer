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


        public Bitmap Compare(string previous, string current, OutputStyle style)
        {
            using (var pStream = File.OpenRead(previous))
            using (var cStream = File.OpenRead(current))
                return Compare(new BlpFile(pStream), new BlpFile(cStream), style);
        }

        public Bitmap Compare(BlpFile previous, BlpFile current, OutputStyle style)
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

                switch(style)
                {
                    case OutputStyle.SideBySide:
                        return GenerateSideBySide(pImg, cImg, boundingBoxes, false);
                    case OutputStyle.FullSideBySide:
                        return GenerateSideBySide(pImg, cImg, boundingBoxes, true);
                    case OutputStyle.DifferenceMask:
                        return GenerateOutput(new Bitmap(cImg.Width, cImg.Height), boundingBoxes);
                    default:
                        return GenerateOutput(cImg, boundingBoxes);
                }
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

        private Bitmap GenerateSideBySide(Bitmap previous, Bitmap current, IEnumerable<Rectangle> boundingBoxes, bool full)
        {
            using (var highlighted = GenerateOutput(current, boundingBoxes))
            {
                int imgcount = full ? 3 : 2;
                int width = (previous.Width + _settings.ImageSpacing) * imgcount - _settings.ImageSpacing;

                Bitmap bmp = new Bitmap(width, previous.Height);

                using (var g = Graphics.FromImage(bmp))
                {
                    if (full)
                        g.DrawImage(current, previous.Width + _settings.ImageSpacing, 0);

                    g.DrawImage(previous, 0, 0);
                    g.DrawImage(highlighted, bmp.Width - highlighted.Width, 0);                    
                }

                return bmp;
            }
        }
    }
}
