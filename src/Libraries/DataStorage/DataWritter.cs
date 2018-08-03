using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace FireFly.Data.Storage
{
    public class DataWritter
    {
        private string _FileName;
        private Dictionary<int, StreamWriter> _ImuStreams = new Dictionary<int, StreamWriter>();
        private ZipArchive _ZipArchive;
        private FileStream _ZipFile;

        public DataWritter(string filename)
        {
            _FileName = filename;
        }

        public void AddImage(int camIndex, long timestamp_ns, byte[] data)
        {
            string imageFileName = string.Format(@"cam{0}\{1}.png", camIndex, timestamp_ns);

            ZipArchiveEntry imageEntry = _ZipArchive.CreateEntry(imageFileName);
            Stream stream = imageEntry.Open();
            stream.Write(data, 0, data.Length);
            stream.Dispose();
        }

        public void AddImu(int imuIndex, long timespampNanoSeconds, double omega_x, double omega_y, double omega_z, double alpha_x, double alpha_y, double alpha_z)
        {
            lock (_ImuStreams)
            {
                StreamWriter writer;
                if (!_ImuStreams.Any(c => c.Key == imuIndex))
                {
                    MemoryStream stream = new MemoryStream();
                    writer = new StreamWriter(stream);
                    writer.WriteLine("timestamp,omega_x,omega_y,omega_z,alpha_x,alpha_y,alpha_z");
                    _ImuStreams.Add(imuIndex, writer);
                }
                else
                {
                    writer = _ImuStreams[imuIndex];
                }
                writer.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6}", timespampNanoSeconds, omega_x, omega_y, omega_z, alpha_x, alpha_y, alpha_z));
                writer.Flush();
            }
        }

        public void Close()
        {
            lock (_ImuStreams)
            {
                foreach (KeyValuePair<int, StreamWriter> kvp in _ImuStreams)
                {
                    string imuFileName = string.Format(@"imu{0}.csv", kvp.Key);
                    ZipArchiveEntry imuEntry = _ZipArchive.CreateEntry(imuFileName);
                    MemoryStream memoryStream = (MemoryStream)kvp.Value.BaseStream;
                    Stream stream = imuEntry.Open();
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        using (StreamReader reader = new StreamReader(memoryStream))
                        {
                            writer.Write(reader.ReadToEnd());
                        }
                    }
                }
                _ZipArchive.Dispose();
                _ZipFile.Dispose();
            }
        }

        public void Open()
        {
            lock (_ImuStreams)
            {
                _ZipFile = new FileStream(_FileName, FileMode.OpenOrCreate);
                _ZipArchive = new ZipArchive(_ZipFile, ZipArchiveMode.Create);
                _ImuStreams.Clear();
            }
        }

        public void UpdateNotes(string notes)
        {
        }
    }
}