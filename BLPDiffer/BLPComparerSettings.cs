using System;
using System.Drawing;

namespace BLPDiffer
{
    public sealed class BLPComparerSettings
    {
        /// <summary>
        /// Difference score required for the CIE analysers
        /// </summary>
        public double NoticeableDifference { get; set; } = 0.3;
        /// <summary>
        /// Space between neighbouring differences. This affects difference consolidation
        /// </summary>
        public int LabelerPadding { get; set; } = 1;
        /// <summary>
        /// Pixel padding between the difference and the bounding box
        /// </summary>
        public int BoundingBoxPadding { get; set; } = 1;
        /// <summary>
        /// Colour of the bounding boxes. Defaults to semi-transparent Yellow (255, 255, 0, 0.4)
        /// </summary>
        public Color BoundingBoxColour { get; set; } = Color.FromArgb(100, Color.Yellow);
        /// <summary>
        /// Controls the bounding box style
        /// </summary>
        public BoundingBoxDrawMode BoundingBoxMode { get; set; } = BoundingBoxDrawMode.Outline;
        /// <summary>
        /// Space between images when using a SideBySide output style
        /// </summary>
        public int ImageSpacing { get; set; } = 5;


        internal void Validate(AnalyserType analyser)
        {
            if (analyser != AnalyserType.ExactMatch && NoticeableDifference <= 0)
                throw new ArgumentException(nameof(NoticeableDifference));
            if (LabelerPadding <= 0)
                throw new ArgumentException(nameof(LabelerPadding));
            if (BoundingBoxPadding <= 0)
                throw new ArgumentException(nameof(BoundingBoxPadding));
            if (BoundingBoxColour == null)
                throw new ArgumentException(nameof(BoundingBoxColour));
            if (ImageSpacing <= 0)
                throw new ArgumentException(nameof(ImageSpacing));
        }
    }
}
