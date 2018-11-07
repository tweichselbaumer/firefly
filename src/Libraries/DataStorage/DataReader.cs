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
        private Dictionary<long, double> _CamCache = new Dictionary<long, double>();
        private string _FileName;
        private Dictionary<long, Tuple<double, double, double, double, double, double>> _ImuCache = new Dictionary<long, Tuple<double, double, double, double, double, double>>();
        private int _Index = 0;
        private TimeSpan _Length = new TimeSpan();
        private ReaderMode _Mode;
        private List<ReaderMode> _ReaderModes = new List<ReaderMode>();
        private bool _Remote;
        private RemoteDataStore _RemoteDataStore;
        private List<long> _Timestamps = new List<long>();
        private ZipArchive _ZipArchive;
        private FileStream _ZipFile;
        private int _DeltaTimeMs;

        public DataReader(string filename, ReaderMode mode, RemoteDataStore remoteDataStore = null)
        {
            _FileName = filename;
            _Mode = mode;
            _Remote = remoteDataStore != null;
            _RemoteDataStore = remoteDataStore;
        }

        public int Count
        {
            get
            {
                return _Timestamps.Count;
            }
        }

        public int DeltaTimeMs
        {
            get
            {
                return _DeltaTimeMs;
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
            _ReaderModes.Clear();

            if (!_Remote)
            {
                _ZipArchive.Dispose();
                _ZipFile.Dispose();
            }
        }

        public bool HasNext()
        {
            return Count > _Index;
        }

        public Tuple<long, List<Tuple<ReaderMode, object>>> Next()
        {
            List<Tuple<ReaderMode, object>> result = new List<Tuple<ReaderMode, object>>();

            long currentTimestamp = 0;
            ReaderMode readerMode = 0;

            currentTimestamp = _Timestamps[_Index];
            readerMode = _ReaderModes[_Index];

            _Index++;

            if (readerMode.HasFlag(ReaderMode.Imu0))
            {
                result.Add(new Tuple<ReaderMode, object>(ReaderMode.Imu0, _ImuCache[currentTimestamp]));
            }

            if (readerMode.HasFlag(ReaderMode.Camera0))
            {
                double exposerTime = 0;
                if (_CamCache.Any(c => c.Key == currentTimestamp))
                    exposerTime = _CamCache.FirstOrDefault(c => c.Key == currentTimestamp).Value;
                ZipArchiveEntry imageEntry = _ZipArchive.GetEntry(string.Format("cam0\\{0}.png", currentTimestamp));
                using (Stream stream = imageEntry.Open())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        result.Add(new Tuple<ReaderMode, object>(ReaderMode.Camera0, new Tuple<double, byte[]>(exposerTime, ms.ToArray())));
                    }
                }
            }

            return new Tuple<long, List<Tuple<ReaderMode, object>>>(currentTimestamp, result);
        }

        public void Open(Action<double> progress = null)
        {
            Dictionary<long, ReaderMode> timestampDict = new Dictionary<long, ReaderMode>();

            if (!_Remote)
            {
                _ZipFile = new FileStream(_FileName, FileMode.Open);
                _ZipArchive = new ZipArchive(_ZipFile, ZipArchiveMode.Read);

                if (_Mode.HasFlag(ReaderMode.Imu0))
                {
                    ZipArchiveEntry entry = _ZipArchive.GetEntry("imu0.csv");
                    using (StreamReader reader = new StreamReader(entry.Open()))
                    {
                        string content = reader.ReadToEnd();
                        ParseImu(content, true, timestampDict);
                    }
                }
                if (_Mode.HasFlag(ReaderMode.Camera0))
                {
                    foreach (ZipArchiveEntry entry in _ZipArchive.Entries)
                    {
                        if (entry.FullName.StartsWith("cam0\\"))
                        {
                            long timestamp = long.Parse(entry.Name.Replace(".png", ""), CultureInfo.InvariantCulture);

                            if (timestampDict.ContainsKey(timestamp))
                            {
                                timestampDict[timestamp] |= ReaderMode.Camera0;
                            }
                            else
                            {
                                timestampDict.Add(timestamp, ReaderMode.Camera0);
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
                _Length = TimeSpan.FromMilliseconds((timestampDict.Keys.Max() - timestampDict.Keys.Min()) / (1000 * 1000));
            }
            else
            {
                string tempfile = Path.GetTempFileName();
                _RemoteDataStore.DownloadFile(_FileName, tempfile, delegate (double percent)
               {
                   progress?.Invoke(percent * 0.5);
               });

                bool skipFirst = true;
                int i = 0;
                int count = File.ReadLines(tempfile).Count();
                foreach (string line in File.ReadLines(tempfile))
                {
                    i++;
                    if (skipFirst)
                    {
                        skipFirst = false;
                    }
                    else
                    {
                        ParseImuLine(line, false, timestampDict);
                    }
                    if (i % 100 == 0)
                    {
                        progress?.Invoke(0.5 + ((double)i / count) * 0.5);
                    }
                }
                File.Delete(tempfile);
            }

            IEnumerable<KeyValuePair<long, ReaderMode>> temp = timestampDict.OrderBy(c => c.Key);

            _Timestamps = temp.Select(c => c.Key).ToList();
            _ReaderModes = temp.Select(c => c.Value).ToList();

            if (_Timestamps.Count >= 2)
                _DeltaTimeMs = (int)Math.Round(_Timestamps.Take(_Timestamps.Count - 1).Select((v, i) => _Timestamps[i + 1] - v).Sum() / ((_Timestamps.Count - 1) * 1000 * 1000.0));
            else
                _DeltaTimeMs = 1000 / 200;

        }

        private void ParseImu(string content, bool isOmega, Dictionary<long, ReaderMode> timestampDict)
        {
            bool skipFirst = true;
            foreach (string line in content.Split('\n'))
            {
                if (skipFirst)
                {
                    skipFirst = false;
                }
                else
                {
                    ParseImuLine(line, isOmega, timestampDict);
                }
            }
        }

        private void ParseImuLine(string line, bool isOmega, Dictionary<long, ReaderMode> timestampDict)
        {
            string[] values = line.Split(',');
            if (values.Length >= 7)
            {
                long timestamp = long.Parse(values[0], CultureInfo.InvariantCulture);

                double omega_x = double.Parse(values[1], CultureInfo.InvariantCulture);
                double omega_y = double.Parse(values[2], CultureInfo.InvariantCulture);
                double omega_z = double.Parse(values[3], CultureInfo.InvariantCulture);

                double alpha_x = double.Parse(values[4], CultureInfo.InvariantCulture);
                double alpha_y = double.Parse(values[5], CultureInfo.InvariantCulture);
                double alpha_z = double.Parse(values[6], CultureInfo.InvariantCulture);

                Tuple<double, double, double, double, double, double> tuple;

                if (isOmega)
                    tuple = new Tuple<double, double, double, double, double, double>(omega_x * 180 / Math.PI, omega_y * 180 / Math.PI, omega_z * 180 / Math.PI, alpha_x, alpha_y, alpha_z);
                else
                    tuple = new Tuple<double, double, double, double, double, double>(omega_x, omega_y, omega_z, alpha_x, alpha_y, alpha_z);

                timestampDict.Add(timestamp, ReaderMode.Imu0);
                _ImuCache.Add(timestamp, tuple);
            }
        }
    }
}