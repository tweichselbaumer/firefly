using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FireFly.Data.Storage
{
    public class PhotometricCalibratrionExporter
    {
        private string _Config;
        private List<Tuple<long, double>> _ExposureTimes = new List<Tuple<long, double>>();
        private string _OutputPath;
        private bool _DistinctExposure;

        public PhotometricCalibratrionExporter(double fxO, double fyO, double cxO, double cyO, double fxN, double fyN, double cxN, double cyN, int width, int height, double k1, double k2, double k3, double k4, string outputPath, bool distinctExposure)
        {
            _OutputPath = outputPath;
            _Config = string.Format(CultureInfo.InvariantCulture, "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\n{8}\t{9}\n{10}\t{11}\t{12}\t{13}\n{8}\t{9}", fxO / width, fyO / height, cxO / width, cyO / height, k1, k2, k3, k4, width, height, fxN / width, fyN / height, cxN / width, cyN / height);
            _DistinctExposure = distinctExposure;
        }

        public void AddFromReader(DataReader reader, Action<double> progress = null)
        {
            int count = reader.Count;

            int i = 0;
            int j = 0;

            while (reader.HasNext())
            {
                i++;
                if (i % 100 == 0)
                {
                    progress?.Invoke((double)i / count);
                }

                Tuple<long, List<Tuple<ReaderMode, object>>> res = reader.Next();

                foreach (Tuple<ReaderMode, object> val in res.Item2)
                {
                    if (val.Item1 == ReaderMode.Camera0)
                    {
                        Tuple<double, byte[]> item = (Tuple<double, byte[]>)val.Item2;
                        if (!_ExposureTimes.Any(c => c.Item2 == item.Item1) || !_DistinctExposure)
                        {
                            File.WriteAllBytes(Path.Combine(_OutputPath, "images", string.Format("{0:D5}.png", j++)), item.Item2);
                            _ExposureTimes.Add(new Tuple<long, double>(res.Item1, item.Item1));
                        }
                    }
                }
            }
        }

        public void Close()
        {
            List<string> times = new List<string>();
            int i = 0;
            foreach (Tuple<long, double> item in _ExposureTimes)
            {
                times.Add(string.Format(CultureInfo.InvariantCulture, "{0:D5}\t{1}\t{2}", i, ((double)item.Item1) / (1000 * 1000 * 1000), item.Item2 / 1000));
                i++;
            }
            File.WriteAllText(Path.Combine(_OutputPath, "times.txt"), string.Join("\n", times));
        }

        public void Open()
        {
            if (Directory.Exists(_OutputPath))
            {
                Directory.Delete(_OutputPath, true);
            }
            Directory.CreateDirectory(_OutputPath);
            Directory.CreateDirectory(Path.Combine(_OutputPath, "images"));
            File.WriteAllText(Path.Combine(_OutputPath, "camera.txt"), _Config);
        }
    }
}