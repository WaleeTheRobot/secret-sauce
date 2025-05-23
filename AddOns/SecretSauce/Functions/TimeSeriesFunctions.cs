using System;
using System.Collections.Generic;

namespace NinjaTrader.Custom.AddOns.SecretSauce.Functions
{
    public static class TimeSeriesFunctions
    {
        public static double CalculateAutocorrelation(
            IReadOnlyList<double> series,
            int lag = 1,
            double tolerance = 1e-10)
        {
            if (series == null || series.Count <= lag)
                return 0.0;

            int n = series.Count;
            double mean = 0.0;

            for (int i = 0; i < n; i++)
                mean += series[i];
            mean /= n;

            double num = 0.0, den = 0.0;

            for (int i = lag; i < n; i++)
            {
                double d0 = series[i] - mean;
                double dLag = series[i - lag] - mean;
                num += d0 * dLag;
            }

            for (int i = 0; i < n; i++)
            {
                double d = series[i] - mean;
                den += d * d;
            }

            return Math.Abs(den) < tolerance ? 0.0 : num / den;
        }

        public static double CalculateLogReturn(
        double current,
        double previous,
        double tolerance = 1e-10)
        {
            if (previous < tolerance || current < tolerance)
                return 0.0;

            return Math.Log(current / previous);
        }

        public static List<double> CalculateLogReturns(IReadOnlyList<double> series)
        {
            var returns = new List<double>();

            for (int i = 1; i < series.Count; i++)
            {
                double current = series[i];
                double previous = series[i - 1];

                returns.Add(CalculateLogReturn(current, previous));
            }

            return returns;
        }
    }
}
