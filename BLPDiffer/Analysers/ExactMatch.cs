using System.Drawing;
using System.Drawing.Imaging;

namespace BLPDiffer.Analysers
{
    internal class ExactMatch : IAnalyser
    {
        public bool[,] Analyse(Bitmap previous, Bitmap current)
        {
            int width = previous.Width, height = previous.Height;

            bool[,] diff = new bool[width, height];

            var pData = previous.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, previous.PixelFormat);
            var cData = current.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, current.PixelFormat);

            unsafe
            {
                byte* pScan0 = (byte*)pData.Scan0, cScan0 = (byte*)cData.Scan0;


                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        diff[y, x] = *(int*)pScan0 != *(int*)cScan0;
                        pScan0 += 4;
                        cScan0 += 4;
                    }
                }
            }

            previous.UnlockBits(pData);
            current.UnlockBits(cData);

            return diff;
        }
    }
}
