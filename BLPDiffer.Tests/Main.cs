using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BLPDiffer.Tests
{
    [TestClass]
    public class Main
    {
        [TestMethod]
        public void ExampleTest()
        {
            var settings = new BLPComparerSettings
            {
                NoticeableDifference = 0.3,
                BoundingBoxMode = BoundingBoxDrawMode.Outline,
                BoundingBoxColour = Color.FromArgb(100, Color.Yellow),
                BoundingBoxPadding = 1,
                LabelerPadding = 1
            };

            var blpComparer = new BLPComparer(AnalyserType.CIEDE2000, settings);

            using (var bmp = blpComparer.Compare("2832258_old.blp", "2832258_new.blp"))
                bmp.Save("test.png", ImageFormat.Png);
        }
    }
}
