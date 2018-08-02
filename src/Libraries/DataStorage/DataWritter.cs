using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireFly.Data.Storage
{
    public class DataWritter
    {
        private FileStream _ZipFile;
        private ZipArchive _ZipArchive;
        private string _FileName;

        public DataWritter(string filename)
        {
            _FileName = filename;
        }

        public void Open()
        {
            _ZipFile = new FileStream(_FileName, FileMode.OpenOrCreate);
            _ZipArchive = new ZipArchive(_ZipFile, ZipArchiveMode.Update);
        }

        public void Close()
        {
            _ZipArchive.Dispose();
            _ZipFile.Dispose();
        }

        public void UpdateNotes(string notes)
        {

        }

        public void AddImage(int camIndex, long timestamp_ns, byte[] data)
        {
            string imageFileName = string.Format(@"cam{0}\{1}.png", camIndex, timestamp_ns);

            ZipArchiveEntry imageEntry = _ZipArchive.CreateEntry(imageFileName);
            Stream stream = imageEntry.Open();
            stream.Write(data, 0, data.Length);
            stream.Dispose();
            _ZipFile.Flush();
        }

        public void AddImu(int imuIndex, long timespampNanoSeconds, double omega_x, double omega_y, double omega_z, double alpha_x, double alpha_y, double alpha_z)
        {
            string imuFileName = string.Format(@"imu{0}.csv", imuIndex);
            bool newEntry = false;
            ZipArchiveEntry imuEntry = _ZipArchive.GetEntry(imuFileName);
            if (imuEntry == null)
            {
                imuEntry = _ZipArchive.CreateEntry(imuFileName);
                newEntry = true;
            }
            Stream stream = imuEntry.Open();
            using (StreamWriter writer = new StreamWriter(stream))
            {
                long endPoint = stream.Length;
                stream.Seek(endPoint, SeekOrigin.Begin);
                if (newEntry)
                {
                    writer.WriteLine("timestamp,omega_x,omega_y,omega_z,alpha_x,alpha_y,alpha_z");
                }
                writer.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6}", timespampNanoSeconds, omega_x, omega_y, omega_z, alpha_x, alpha_y, alpha_z));
            }
            stream.Dispose();
            _ZipFile.Flush();
        }
    }
}
