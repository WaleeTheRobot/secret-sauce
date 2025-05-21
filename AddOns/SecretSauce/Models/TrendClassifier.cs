using NinjaTrader.Custom.AddOns.ScalpMaster.Events;
using NinjaTrader.Custom.AddOns.SecretSauce.Functions;
using NinjaTrader.NinjaScript;
using System;
using System.Collections.Generic;

namespace NinjaTrader.Custom.AddOns.SecretSauce.Models
{
    /// <summary>
    /// Runs two statistical tests (OLS slope, Mann–Kendall)
    /// on provided price windows and classifies the result into a trend score.
    /// </summary>
    public class TrendClassifier
    {
        private readonly int _period;

        public TrendClassifier(int period)
        {
            _period = period;
        }

        public double CalculateTrendScore(ISeries<double> priceSeries, int currentBar)
        {
            if (currentBar + 1 < _period)
                return 0;

            var pricesForSlope = new List<double>();
            // Reverse order due to NT series
            for (int i = _period - 1; i >= 0; i--)
                pricesForSlope.Add(priceSeries[i]);

            if (pricesForSlope.Count == 0)
            {
                EventManager.PrintMessage("Warning: Price lists are empty. Cannot calculate trend score.");
                return 0;
            }

            return GetNormalizedTrendScore(pricesForSlope);
        }

        /// <summary>
        /// Computes a normalized trend score between -1 and 1 based on OLS slope and Mann-Kendall tau.
        /// </summary>
        /// <param name="pricesForSlope">
        /// Recent closing prices for slope and Kendall tests.
        /// </param>
        /// <returns>
        /// A double between -1 and 1, where 1 is a strong uptrend, -1 is a strong downtrend, and 0 is neutral.
        /// </returns>
        private static double GetNormalizedTrendScore(
            IReadOnlyList<double> pricesForSlope)
        {
            // Compute OLS regression statistics
            var s = RollingSlope.RollingSlopeTest(pricesForSlope);
            // Compute Mann-Kendall statistics
            var t = MannKendall.MannKendallTest(pricesForSlope);

            // Calculate correlation coefficient r = sign(slope) * sqrt(R²)
            double r = Math.Sign(s.slope) * Math.Sqrt(s.rSquared);
            // Combine with tau to get normalized trend score
            double combinedTrend = (t.tau + r) / 2.0;

            return combinedTrend;
        }
    }
}
