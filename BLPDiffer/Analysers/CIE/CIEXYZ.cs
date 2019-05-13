using System;

namespace BLPDiffer.Analysers.CIE
{
    internal class CIEXYZ
    {
        public const double RefX = 95.047;
        public const double RefY = 100.000;
        public const double RefZ = 108.883;

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public CIEXYZ(double x, double y, double z) => (X, Y, Z) = (x, y, z);


        public static CIEXYZ FromARGB(int color)
        {
            var rLinear = ((color >> 16) & 0xFF) / 255.0;
            var gLinear = ((color >> 8) & 0xFF) / 255.0;
            var bLinear = (color & 0xFF) / 255.0;

            var r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / 1.055, 2.4) : (rLinear / 12.92);
            var g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / 1.055, 2.4) : (gLinear / 12.92);
            var b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / 1.055, 2.4) : (bLinear / 12.92);

            return new CIEXYZ(
                r * 0.4124 + g * 0.3576 + b * 0.1805,
                r * 0.2126 + g * 0.7152 + b * 0.0722,
                r * 0.0193 + g * 0.1192 + b * 0.9505
            );
        }
    }
}
