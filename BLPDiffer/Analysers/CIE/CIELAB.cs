using System;

namespace BLPDiffer.Analysers.CIE
{
    internal class CIELAB
    {
        private const double V = 1.0 / 3.0;
        private const double V1 = 16.0 / 116.0;

        public double L { get; set; }
        public double A { get; set; }
        public double B { get; set; }

        public CIELAB(double l, double a, double b) => (L, A, B) = (l, a, b);


        public static CIELAB FromARGB(int color) => FromCIEXYZ(CIEXYZ.FromARGB(color));

        public static CIELAB FromCIEXYZ(CIEXYZ xyzColor)
        {
            double Transform(double t) => (t > 0.008856) ? Math.Pow(t, V) : ((7.787 * t) + V1);

            var x = Transform(xyzColor.X / CIEXYZ.RefX);
            var y = Transform(xyzColor.Y / CIEXYZ.RefY);
            var z = Transform(xyzColor.Z / CIEXYZ.RefZ);

            return new CIELAB(
                116.0 * y - 16,
                500.0 * (x - y),
                200.0 * (y - z)
            );
        }
    }
}
