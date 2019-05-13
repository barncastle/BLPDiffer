using System;
using System.Drawing;
using System.Drawing.Imaging;
using BLPDiffer.Analysers.CIE;

namespace BLPDiffer.Analysers
{
    internal class CIEDE2000 : IAnalyser
    {
#pragma warning disable IDE1006 // Naming Styles
        private const double kL = 1;
        private const double kC = 1;
        private const double kH = 1;
#pragma warning restore IDE1006 // Naming Styles

        public double NoticeableDifference { get; set; }

        public CIEDE2000(double noticeableDifference) => NoticeableDifference = noticeableDifference;


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
            var cAvg = (cStd + cSmp) / 2;

            var G = 0.5 * (1 - Math.Sqrt(Math.Pow(cAvg, 7) / (Math.Pow(cAvg, 7) + Math.Pow(25, 7))));

            var apStd = stdLab.A * (1 + G);
            var apSmp = smpLab.A * (1 + G);

            var cpStd = Math.Sqrt(apStd * apStd + stdLab.B * stdLab.B);
            var cpSmp = Math.Sqrt(apSmp * apSmp + smpLab.B * smpLab.B);

            var hpStd = Math.Abs(apStd) + Math.Abs(stdLab.B) == 0 ? 0 : Math.Atan2(stdLab.B, apStd);
            if (hpStd < 0)
                hpStd *= 2 * Math.PI;

            var hpSmp = Math.Abs(apSmp) + Math.Abs(smpLab.B) == 0 ? 0 : Math.Atan2(smpLab.B, apSmp);
            if (hpSmp < 0)
                hpSmp *= 2 * Math.PI;

            var dL = smpLab.L - stdLab.L;
            var dC = cpSmp - cpStd;

            var dhp = cpStd * cpSmp == 0 ? 0 : hpSmp - hpStd;
            if (dhp > Math.PI)
                dhp *= 2 * Math.PI;
            if (dhp < -Math.PI)
                dhp += 2 * Math.PI;

            var dH = 2 * Math.Sqrt(cpStd * cpSmp) * Math.Sin(dhp / 2);

            var Lp = (stdLab.L + smpLab.L) / 2;
            var Cp = (cpStd + cpSmp) / 2;

            var hp = hpStd + hpSmp;
            if (cpStd * cpSmp != 0)
            {
                hp /= 2;
                if (Math.Abs(hpStd - hpSmp) > Math.PI)
                    hp -= Math.PI;
                if (hp < 0)
                    hp += 2 * Math.PI;
            }

            var Lpm50 = Math.Pow(Lp - 50, 2);
            var T = 1 -
                    0.17 * Math.Cos(hp - Math.PI / 6) +
                    0.24 * Math.Cos(2 * hp) +
                    0.32 * Math.Cos(3 * hp + Math.PI / 30) -
                    0.20 * Math.Cos(4 * hp - 63 * Math.PI / 180);

            var Sl = 1 + 0.015 * Lpm50 / Math.Sqrt(20 + Lpm50);
            var Sc = 1 + 0.045 * Cp;
            var Sh = 1 + 0.015 * Cp * T;

            var deltaTheta = 30 * Math.PI / 180 * Math.Exp(-1 * Math.Pow((180 / Math.PI * hp - 275) / 25, 2));
            var Rc = 2 * Math.Sqrt(Math.Pow(Cp, 7) / (Math.Pow(Cp, 7) + Math.Pow(25, 7)));
            var Rt = -1 * Math.Sin(2 * deltaTheta) * Rc;

            var score = Math.Sqrt(Math.Pow(dL / (kL * Sl), 2) +
                                  Math.Pow(dC / (kC * Sc), 2) +
                                  Math.Pow(dH / (kH * Sh), 2) +
                                  Rt * dC / (kC * Sc) * dH / (kH * Sh));

            return score >= NoticeableDifference;
        }
    }
}
