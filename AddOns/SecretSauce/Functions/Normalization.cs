using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTrader.Custom.AddOns.SecretSauce.Functions
{
    public static class Normalization
    {
        public static double NormalizeMinMax(
           double value,
           double min,
           double max,
           double tolerance = 1e-10)
        {
            if (min > max) throw new ArgumentException("Min cannot be greater than Max");

            double range = max - min;
            if (range < tolerance)
            {
                return 0;
            }
            return (value - min) / range;
        }

        public static double CalculateZScore(
            double value,
            IReadOnlyList<double> series,
            double tolerance = 1e-10)
        {
            if (series == null || series.Count <= 1) return 0.0;

            double mean = series.Average();
            double variance = series.Select(x => Math.Pow(x - mean, 2)).Average();
            double stdDev = Math.Sqrt(variance);

            if (stdDev < tolerance)
                return 0;

            return (value - mean) / stdDev;
        }

        public static (double ZScore, double Mean, double StdDev) RollingStats(
            IReadOnlyList<double> series,
            int period,
            double tolerance = 1e-10)
        {
            if (series == null || series.Count < period)
                return (0, 0, 0);

            var recent = series.Skip(series.Count - period).ToList();
            double mean = recent.Average();
            double variance = recent.Select(x => Math.Pow(x - mean, 2)).Average();
            double stdDev = Math.Sqrt(variance);

            if (stdDev < tolerance)
                return (0.0, mean, stdDev);

            double latest = series.Last();
            double z = (latest - mean) / stdDev;
            return (z, mean, stdDev);
        }

        public static (double ZScore, double Mean, double StdDev) RollingROCStats(
            IReadOnlyList<double> series,
            int period,
            int rocPeriod,
            double tolerance = 1e-10)
        {
            if (series == null || series.Count < period + rocPeriod)
                return (0, 0, 0);

            List<double> rocValues = new List<double>();

            // Calculate ROC for each point in the rolling window
            for (int i = series.Count - period; i < series.Count; i++)
            {
                double current = series[i];
                double past = series[i - rocPeriod];
                if (Math.Abs(past) < tolerance)
                    rocValues.Add(0.0);
                else
                    rocValues.Add((current - past) / past * 100);
            }

            // Calculate mean and standard deviation of the ROC values
            double mean = rocValues.Average();
            double variance = rocValues.Select(x => Math.Pow(x - mean, 2)).Average();
            double stdDev = Math.Sqrt(variance);

            if (stdDev < tolerance)
                return (0.0, mean, stdDev);

            // Calculate Z-score of the latest ROC value
            double latestROC = rocValues.Last();
            double z = (latestROC - mean) / stdDev;

            return (z, mean, stdDev);
        }
    }
}
