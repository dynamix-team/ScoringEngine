using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Core
{
#if OFFLINE
    /// <summary>
    /// A class for reporting scores
    /// </summary>
    internal static class ScoreReporting
    {
        /// <summary>
        /// Generate an HTML scoring report from the scoring items given
        /// </summary>
        /// <param name="items">A list of scoring items</param>
        /// <returns></returns>
        internal static string GenerateHTMLReport(Scoring.ScoringItem[] items)
        {
            return ""; //TODO
        }
    }
#endif
}
