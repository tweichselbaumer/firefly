using MatFileHandler;
using System.Collections.Generic;
using System.Linq;

namespace FireFly.Data.Storage
{
    public class RawMatlabImporter
    {
        private string _FileName;
        private MatlabFormat _MatlabFormat;

        public RawMatlabImporter(string filename, MatlabFormat matlabFormat)
        {
            _FileName = filename;
            _MatlabFormat = matlabFormat;
        }

        public (List<double> time, List<double> gyrox, List<double> gyroy, List<double> gyroz, List<double> accx, List<double> accy, List<double> accz) Load()
        {
            IMatFile matFile;
            using (var fileStream = new System.IO.FileStream(_FileName, System.IO.FileMode.Open))
            {
                var reader = new MatFileReader(fileStream);
                matFile = reader.Read();
            }

            IArray itime = (((matFile["data"].Value as IStructureArray)["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["time", 0];

            IArray igyrox = (((matFile["data"].Value as IStructureArray)["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyrox", 0];
            IArray igyroy = (((matFile["data"].Value as IStructureArray)["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyroy", 0];
            IArray igyroz = (((matFile["data"].Value as IStructureArray)["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyroz", 0];

            IArray iaccx = (((matFile["data"].Value as IStructureArray)["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accx", 0];
            IArray iaccy = (((matFile["data"].Value as IStructureArray)["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accy", 0];
            IArray iaccz = (((matFile["data"].Value as IStructureArray)["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accz", 0];

            List<double> time = itime.ConvertToDoubleArray().ToList();

            List<double> gyrox = igyrox.ConvertToDoubleArray().ToList();
            List<double> gyroy = igyroy.ConvertToDoubleArray().ToList();
            List<double> gyroz = igyroz.ConvertToDoubleArray().ToList();

            List<double> accx = iaccx.ConvertToDoubleArray().ToList();
            List<double> accy = iaccy.ConvertToDoubleArray().ToList();
            List<double> accz = iaccz.ConvertToDoubleArray().ToList();

            return (time, gyrox, gyroy, gyroz, accx, accy, accz);
        }
    }
}