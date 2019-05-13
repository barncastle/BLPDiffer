namespace BLPDiffer
{
    public enum AnalyserType
    {
        /// <summary>
        /// 1976 Colour Difference formula
        /// </summary>
        CIE76,
        /// <summary>
        /// 1994 Colour Difference formula
        /// </summary>
        CIE94,
        /// <summary>
        /// 2000 Colour Difference formula
        /// </summary>
        CIEDE2000,
        /// <summary>
        /// Pixel "like-for-like" comparison
        /// </summary>
        ExactMatch,
    }
}
