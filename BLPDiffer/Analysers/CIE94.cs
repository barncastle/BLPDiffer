using System;
using System.Drawing;
using System.Drawing.Imaging;
using BLPDiffer.Analysers.CIE;

namespace BLPDiffer.Analysers
{
    internal class CIE94 : IAnalyser
    {
#pragma warning disable IDE1006 // Naming Styles
        private const double kL = 1;
        private const double k1 = 0.045;
        private const double k2 = 0.015;
#pragma warning restore IDE1006 // Naming Styles

        public double NoticeableDifference { get; set; }

        public CIE94(double noticeableDifference) => NoticeableDifference = noticeableDifference;


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
            var cStd = Math.Sqrt(stdLab.A * stdLab.A + stdLab.B * stdLab.B);
            var cSmp = Math.Sqrt(smpLab.A * smpLab.A + smpLab.B * smpLab.B);

            var dL2 = Math.Pow(stdLab.L - smpLab.L, 2);
            var dC2 = Math.Pow(cStd - cSmp, 2);
            var dH2 = Math.Pow(stdLab.A - smpLab.A, 2) + Math.Pow(stdLab.B - smpLab.B, 2) - dC2;

            var score = Math.Sqrt(dL2 / Math.Pow(kL, 2) +
                                  dC2 / Math.Pow(1 + k1 * cStd, 2) +
                                  dH2 / Math.Pow(1 + k2 * cStd, 2));

            return score >= NoticeableDifference;
        }
    }
}
