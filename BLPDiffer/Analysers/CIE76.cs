using System;
using System.Drawing;
using System.Drawing.Imaging;
using BLPDiffer.Analysers.CIE;

namespace BLPDiffer.Analysers
{
    internal class CIE76 : IAnalyser
    {
        public double NoticeableDifference { get; set; }

        public CIE76(double noticeableDifference) => NoticeableDifference = noticeableDifference;


        public bool[,] Analyse(Bitmap previous, Bitmap current)
        {
            int width = previous.Width, height = previous.Height;

            bool[,] diff = new bool[width, height];

            var pData = previous.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, previous.PixelFormat);
            var cData = current.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, current.PixelFormat);

            unsafe
            {
                byte* pScan0 = (byte*)pData.Scan0, cScan0 = (byte*)cData.Scan0;
                int p1, p2;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        p1 = *(int*)pScan0;
                        p2 = *(int*)cScan0;
                        pScan0 += 4;
                        cScan0 += 4;

                        // shortcut same values
                        if (p1 == p2)
                            continue;

                        diff[y, x] = IsDifferentEnough(CIELAB.FromARGB(p1), CIELAB.FromARGB(p2));
                    }
                }
            }

            previous.UnlockBits(pData);
            current.UnlockBits(cData);

            return diff;
        }

        private bool IsDifferentEnough(CIELAB stdLab, CIELAB smpLab)
        {
            var score = Math.Sqrt(Math.Pow(smpLab.L - stdLab.L, 2) +
                                  Math.Pow(smpLab.A - stdLab.A, 2) +
                                  Math.Pow(smpLab.B - stdLab.B, 2));

            return score >= NoticeableDifference;
        }
    }
}
