﻿using System;
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
        private bool _IncreasingExposure;
        private string _OutputPath;
        private List<double> _ResponseValues = new List<double>();

        public PhotometricCalibratrionExporter(double fxO, double fyO, double cxO, double cyO, double fxN, double fyN, double cxN, double cyN, int width, int height, double k1, double k2, double k3, double k4, string outputPath, bool increasingExposure, List<double> responseValues)
        {
            _OutputPath = outputPath;
            _Config = string.Format(CultureInfo.InvariantCulture, "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\n{8}\t{9}\n{10}\t{11}\t{12}\t{13}\n{8}\t{9}", fxO / width, fyO / height, cxO / width, cyO / height, k1, k2, k3, k4, width, height, fxN / width, fyN / height, cxN / width, cyN / height);
            _IncreasingExposure = increasingExposure;
            _ResponseValues = responseValues;
        }

        public static void ExporterSettings(double fxO, double fyO, double cxO, double cyO, double fxN, double fyN, double cxN, double cyN, int width, int height, double k1, double k2, double k3, double k4, string outputPath, List<double> responseValues, byte[] vignette)
        {
            File.WriteAllText(Path.Combine(outputPath, "camera.txt"), string.Format(CultureInfo.InvariantCulture, "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\n{8}\t{9}\n{10}\t{11}\t{12}\t{13}\n{8}\t{9}", fxO / width, fyO / height, cxO / width, cyO / height, k1, k2, k3, k4, width, height, fxN / width, fyN / height, cxN / width, cyN / height));
            File.WriteAllText(Path.Combine(outputPath, "pcalib.txt"), string.Format("{0}\r\n", string.Join(" ", responseValues.Select(c => string.Format(CultureInfo.InvariantCulture, "{0}", c)))));
            File.WriteAllBytes(Path.Combine(outputPath, "vignette.png"), vignette);
        }

        public static void ExporterSettingsVOLocal(double fxO, double fyO, double cxO, double cyO, double fxN, double fyN, double cxN, double cyN, int width, int height, double k1, double k2, double k3, double k4, string outputPath, List<double> responseValues, byte[] vignette)
        {
            File.WriteAllText(Path.Combine(outputPath, "camera.txt"), string.Format(CultureInfo.InvariantCulture, "EquiDistant\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\n{8}\t{9}\n{10}\t{11}\t{12}\t{13}\t0\n{8}\t{9}\n", fxO / width, fyO / height, cxO / width, cyO / height, k1, k2, k3, k4, width, height, fxN / width, fyN / height, cxN / width, cyN / height));
            File.WriteAllText(Path.Combine(outputPath, "pcalib.txt"), string.Format("{0}\r\n", string.Join(" ", responseValues.Select(c => string.Format(CultureInfo.InvariantCulture, "{0}", c)))));
            File.WriteAllBytes(Path.Combine(outputPath, "vignette.png"), vignette);
            if (!Directory.Exists(Path.Combine(outputPath, "log")))
            {
                Directory.CreateDirectory(Path.Combine(outputPath, "log"));
            }
        }

        public static void ExporterSettingsVORemote(double fxO, double fyO, double cxO, double cyO, double fxN, double fyN, double cxN, double cyN, int width, int height, double k1, double k2, double k3, double k4, string outputPath, List<double> responseValues, byte[] vignette, string hostname, string ipAddress, string username, string password)
        {
            RemoteDataStore remoteDataStore = new RemoteDataStore(ipAddress, username, password);
            remoteDataStore.UploadContentToFile(string.Format("{0}/camera.txt", outputPath), string.Format(CultureInfo.InvariantCulture, "EquiDistant\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\n{8}\t{9}\n{10}\t{11}\t{12}\t{13}\t0\n{8}\t{9}", fxO / width, fyO / height, cxO / width, cyO / height, k1, k2, k3, k4, width, height, fxN / width, fyN / height, cxN / width, cyN / height));
            remoteDataStore.UploadContentToFile(string.Format("{0}/pcalib.txt", outputPath), string.Format("{0}\r\n", string.Join(" ", responseValues.Select(c => string.Format(CultureInfo.InvariantCulture, "{0}", c)))));
            remoteDataStore.UploadContentToFile(string.Format("{0}/vignette.png", outputPath), vignette);
            remoteDataStore.ExecuteCommands(new List<string>() { string.Format("mkdir {0}/log", outputPath) }, string.Format("{0}@{1}:.{{0,}}[$]", username, hostname));
        }

        public void AddFromReader(RawDataReader reader, Action<double> progress = null)
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

                Tuple<long, List<Tuple<RawReaderMode, object>>> res = reader.Next();

                foreach (Tuple<RawReaderMode, object> val in res.Item2)
                {
                    if (val.Item1 == RawReaderMode.Camera0)
                    {
                        Tuple<double, byte[]> item = (Tuple<double, byte[]>)val.Item2;
                        if (!_ExposureTimes.Any(c => c.Item2 > item.Item1) || !_IncreasingExposure)
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
                times.Add(string.Format(CultureInfo.InvariantCulture, "{0:D5}\t{1}\t{2}", i, ((double)item.Item1) / (1000 * 1000 * 1000), item.Item2));
                i++;
            }
            File.WriteAllText(Path.Combine(_OutputPath, "times.txt"), string.Join("\n", times));

            if (_ResponseValues != null && _ResponseValues.Count > 0)
            {
                File.WriteAllText(Path.Combine(_OutputPath, "pcalib.txt"), string.Format("{0}\r\n", string.Join(" ", _ResponseValues.Select(c => string.Format(CultureInfo.InvariantCulture, "{0}", c)))));
            }
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