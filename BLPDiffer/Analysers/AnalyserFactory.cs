using System;

namespace BLPDiffer.Analysers
{
    internal static class AnalyserFactory
    {
        public static IAnalyser Create(AnalyserType type, double noticeableDifference)
        {
            switch (type)
            {
                case AnalyserType.ExactMatch:
                    return new ExactMatch();
                case AnalyserType.CIE76:
                    return new CIE76(noticeableDifference);
                case AnalyserType.CIE94:
                    return new CIE94(noticeableDifference);
                case AnalyserType.CIEDE2000:
                    return new CIEDE2000(noticeableDifference);
                default:
                    throw new ArgumentException($"Unrecognized analyser {type}");
            }
        }
    }
}
