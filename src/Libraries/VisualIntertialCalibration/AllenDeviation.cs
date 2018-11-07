using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FireFly.VI.Calibration
{
    public static class AllenDeviation
    {
        public static (List<double> time, List<double> sigma) Calculate(List<double> values, double sampleTime, int points = 1000)
        {
            int maxN = (int)Math.Pow(2, Math.Floor(Math.Log(values.Count / 2, 2)));

            List<double> logSpace = LogSpace10(0, Math.Log10(maxN), points).Select(c => Math.Ceiling(c)).Distinct().ToList();
            List<double> times = logSpace.Select(c => c * 1 / sampleTime).ToList();

            List<double> sigmas2 = Enumerable.Range(1, logSpace.Count).Select(c => 0.0).ToList();

            List<double> theta = values.CumulativeSum().Select(c => c / sampleTime).ToList();

            //for (int i = 0; i < logSpace.Count; i++)
            Parallel.For(0, logSpace.Count, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 }, i =>
              {
                  double value = 0.0;
                  double logSpace_i = logSpace[i];
                  double end = Math.Floor(values.Count - 2 * logSpace_i);
                  for (int k = 0; k < end; k++)
                  {
                      value += Math.Pow(theta[(int)Math.Floor(k + 2 * logSpace_i)] - 2 * theta[(int)Math.Floor(k + logSpace_i)] + theta[k], 2);
                  }
                  sigmas2[i] = value;
                //}
            });

            List<double> sigmas = sigmas2.Select((c, i) => Math.Sqrt(c / (2 * Math.Pow(times[i], 2) * (values.Count - 2 * logSpace[i])))).ToList();

            return (times, sigmas);
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

        private static IEnumerable<double> LogSpace10(double start, double end, int count)
        {
            return Enumerable.Range(0, count).Select(i => Math.Pow(10, start + (end - start) / (count - 1) * i));
        }
    }
}