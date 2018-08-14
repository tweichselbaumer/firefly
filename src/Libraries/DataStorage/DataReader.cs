using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace FireFly.Data.Storage
{
    [Flags]
    public enum ReaderMode
    {
        Camera0 = 1,
        Imu0 = 2
    }

    public class DataReader
    {
        private long _CurrentTimestamp = -1;
        private string _FileName;
        private Dictionary<long, Tuple<double, double, double, double, double, double>> _ImuCache = new Dictionary<long, Tuple<double, double, double, double, double, double>>();
        private Dictionary<long, double> _CamCache = new Dictionary<long, double>();
        private ReaderMode _Mode;
        private Dictionary<long, ReaderMode> _Timestamps = new Dictionary<long, ReaderMode>();
        private ZipArchive _ZipArchive;
        private FileStream _ZipFile;
        private TimeSpan _Length = new TimeSpan();

        public DataReader(string filename, ReaderMode mode)
        {
            _FileName = filename;
            _Mode = mode;
        }

        public int DeltaTimeMs
        {
            get
            {//TODO:
                return 1000 / 200;
            }
        }

        public TimeSpan Length
        {
            get
            {
                return _Length;
            }
        }

        public static ReaderMode AvailableReaderModes(string filename)
        {
            return 0;
        }

        public void Close()
        {
            _Timestamps.Clear();
            _ImuCache.Clear();
            _ZipArchive.Dispose();
            _ZipFile.Dispose();
        }

        public bool HasNext()
        {
            return _Timestamps.Any(c => c.Key > _CurrentTimestamp);
        }

        public Tuple<long, List<Tuple<ReaderMode, object>>> Next()
        {
            List<Tuple<ReaderMode, object>> result = new List<Tuple<ReaderMode, object>>();
            _CurrentTimestamp = _Timestamps.FirstOrDefault(c => c.Key > _CurrentTimestamp).Key;

            ReaderMode readerMode = _Timestamps[_CurrentTimestamp];

            if (readerMode.HasFlag(ReaderMode.Imu0))
            {
                result.Add(new Tuple<ReaderMode, object>(ReaderMode.Imu0, _ImuCache[_CurrentTimestamp]));
            }

            if (readerMode.HasFlag(ReaderMode.Camera0))
            {
                double exposerTime = 0;
                if (_CamCache.Any(c => c.Key == _CurrentTimestamp))
                    exposerTime = _CamCache.FirstOrDefault(c => c.Key == _CurrentTimestamp).Value;
                ZipArchiveEntry imageEntry = _ZipArchive.GetEntry(string.Format("cam0\\{0}.png", _CurrentTimestamp));
                using (Stream stream = imageEntry.Open())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        result.Add(new Tuple<ReaderMode, object>(ReaderMode.Camera0, new Tuple<double, byte[]>(exposerTime, ms.ToArray())));
                    }
                }
            }

            return new Tuple<long, List<Tuple<ReaderMode, object>>>(_CurrentTimestamp, result);
        }

        public void Open()
        {
            _ZipFile = new FileStream(_FileName, FileMode.Open);
            _ZipArchive = new ZipArchive(_ZipFile, ZipArchiveMode.Read);

            if (_Mode.HasFlag(ReaderMode.Imu0))
            {
                ZipArchiveEntry entry = _ZipArchive.GetEntry("imu0.csv");
                using (StreamReader reader = new StreamReader(entry.Open()))
                {
                    string content = reader.ReadToEnd();
                    bool skipFirst = true;
                    foreach (string line in content.Split('\n'))
                    {
                        if (skipFirst)
                        {
                            skipFirst = false;
                        }
                        else
                        {
                            string[] values = line.Split(',');
                            if (values.Length == 7)
                            {
                                long timestamp = long.Parse(values[0], CultureInfo.InvariantCulture);

                                double omega_x = double.Parse(values[1], CultureInfo.InvariantCulture);
                                double omega_y = double.Parse(values[2], CultureInfo.InvariantCulture);
                                double omega_z = double.Parse(values[3], CultureInfo.InvariantCulture);

                                double alpha_x = double.Parse(values[4], CultureInfo.InvariantCulture);
                                double alpha_y = double.Parse(values[5], CultureInfo.InvariantCulture);
                                double alpha_z = double.Parse(values[6], CultureInfo.InvariantCulture);

                                Tuple<double, double, double, double, double, double> tuple = new Tuple<double, double, double, double, double, double>(omega_x * 180 / Math.PI, omega_y * 180 / Math.PI, omega_z * 180 / Math.PI, alpha_x, alpha_y, alpha_z);
                                _Timestamps.Add(timestamp, ReaderMode.Imu0);
                                _ImuCache.Add(timestamp, tuple);
                            }
                        }
                    }
                }
            }
            if (_Mode.HasFlag(ReaderMode.Camera0))
            {
                foreach (ZipArchiveEntry entry in _ZipArchive.Entries)
                {
                    if (entry.FullName.StartsWith("cam0\\"))
                    {
                        long timestamp = long.Parse(entry.Name.Replace(".png", ""), CultureInfo.InvariantCulture);

                        if (_Timestamps.ContainsKey(timestamp))
                        {
                            _Timestamps[timestamp] |= ReaderMode.Camera0;
                        }
                        else
                        {
                            _Timestamps.Add(timestamp, ReaderMode.Camera0);
                        }
                    }
                }
                ZipArchiveEntry camentry = _ZipArchive.GetEntry("cam0.csv");
                if (camentry != null)
                {
                    using (StreamReader reader = new StreamReader(camentry.Open()))
                    {
                        string content = reader.ReadToEnd();
                        bool skipFirst = true;
                        foreach (string line in content.Split('\n'))
                        {
                            if (skipFirst)
                            {
                                skipFirst = false;
                            }
                            else
                            {
                                string[] values = line.Split(',');
                                if (values.Length == 2)
                                {
                                    long timestamp = long.Parse(values[0], CultureInfo.InvariantCulture);

                                    double exposureTime = double.Parse(values[1], CultureInfo.InvariantCulture);
                                    _CamCache.Add(timestamp, exposureTime);
                                }
                            }
                        }
                    }
                }
            }
            _Length = TimeSpan.FromMilliseconds((_Timestamps.Keys.Max() - _Timestamps.Keys.Min()) / (1000 * 1000));
        }
    }
}