using System;
using System.Collections.Generic;
using System.Linq;

namespace FireFly.VI.Calibration
{
    public static class AllenDeviation
    {
        public static (double[] time, double[] sigma) Calculate(double[] values, double sampleTime, int points = 1000)
        {
            int maxN = (int)Math.Pow(2, Math.Floor(Math.Log(values.Length, 2)));

            List<double> logSpace = LogSpace(0, Math.Log10(maxN), points).Distinct().ToList();
            List<double> times = logSpace.Select(c => c * 1 / sampleTime).ToList();

            List<double> sigmas2 = Enumerable.Range(1, logSpace.Count).Select(c => 0.0).ToList();

            List<double> theta = values.CumulativeSum().Select(c => c / sampleTime).ToList();

            for (int i = 0; i < logSpace.Count; i++)
            {
                for (int k = 0; k < Math.Floor(values.Length - 2 * logSpace[i]); k++)
                {
                    sigmas2[i] += Math.Pow(theta[(int)Math.Floor(k + 2 * logSpace[i])] - 2 * theta[(int)Math.Floor(k + logSpace[i])] + theta[k], 2);
                }
            }

            List<double> sigmas = sigmas2.Select((c, i) => Math.Sqrt(c / (2 * Math.Pow(times[i], 2) * (values.Length - 2 * logSpace[i])))).ToList();

            return (times.ToArray(), sigmas.ToArray());
        }

        private static IEnumerable<double> CumulativeSum(this IEnumerable<double> sequence)
        {
            double sum = 0;
            foreach (var item in sequence)
            {
                sum += item;
                yield return sum;
            }
        }

        private static IEnumerable<double> LogSpace(double start, double end, int count)
        {
            double d = (double)count, p = end / start;
            return Enumerable.Range(0, count).Select(i => start * Math.Pow(p, i / d));
        }
    }
}