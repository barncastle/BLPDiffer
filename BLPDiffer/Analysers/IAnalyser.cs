using System.Drawing;

namespace BLPDiffer.Analysers
{
    interface IAnalyser
    {
        bool[,] Analyse(Bitmap previous, Bitmap current);
    }
}
