using System;
using System.Collections.Generic;
using System.Text;

namespace BLPDiffer
{
    public enum OutputStyle
    {
        /// <summary>
        /// Returns the newest image with the differences highlighted over it
        /// </summary>
        Single,
        /// <summary>
        /// Returns the previous image next to the new image with the differences highlighted over it
        /// </summary>
        SideBySide,
        /// <summary>
        /// Returns the previous image, new image and then the new image with the differences highlighted over it
        /// </summary>
        FullSideBySide,
        /// <summary>
        /// Returns the differences only
        /// </summary>
        DifferenceMask
    }
}
