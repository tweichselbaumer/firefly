using FireFly.Utilities;
using MatFileHandler;
using System;
using System.Collections.Generic;
using System.IO;

namespace FireFly.Data.Storage
{
    [Flags]
    public enum MatlabFormat
    {
        Imu0 = 1,
        Camera0 = 2,
    }

    public class RawMatlabExporter
    {
        private DataBuilder _DataBuilder;
        private IStructureArray _DataStruct;
        private string _FileName;
        private MatlabFormat _MatlabFormat;

        public RawMatlabExporter(string filename, MatlabFormat matlabFormat)
        {
            _FileName = filename;
            _MatlabFormat = matlabFormat;
        }

        public void AddFromReader(RawDataReader reader, Action<double> progress = null)
        {
            IArray time_imu0 = null;
            IArray time_cam0 = null;
            IArray gyrox = null;
            IArray gyroy = null;
            IArray gyroz = null;
            IArray accx = null;
            IArray accy = null;
            IArray accz = null;

            if (_MatlabFormat.HasFlag(MatlabFormat.Imu0))
            {
                time_imu0 = ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["time", 0];

                gyrox = ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyrox", 0];
                gyroy = ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyroy", 0];
                gyroz = ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyroz", 0];

                accx = ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accx", 0];
                accy = ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accy", 0];
                accz = ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accz", 0];
            }

            if (_MatlabFormat.HasFlag(MatlabFormat.Camera0))
            {
                time_cam0 = ((_DataStruct["raw", 0] as IStructureArray)["cam0", 0] as IStructureArray)["time", 0];
            }

            int imu0Index = 0;
            int cam0Index = 0;

            int count = reader.Count;

            int i = 0;

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
                    if (val.Item1 == RawReaderMode.Imu0 && _MatlabFormat.HasFlag(MatlabFormat.Imu0))
                    {
                        if (time_imu0.Dimensions.Length == 0 || imu0Index >= time_imu0.Dimensions[1])
                        {
                            time_imu0 = ResizeArray<double>(time_imu0, 1, imu0Index * 2 + 1);
                            gyrox = ResizeArray<double>(gyrox, 1, imu0Index * 2 + 1);
                            gyroy = ResizeArray<double>(gyroy, 1, imu0Index * 2 + 1);
                            gyroz = ResizeArray<double>(gyroz, 1, imu0Index * 2 + 1);
                            accx = ResizeArray<double>(accx, 1, imu0Index * 2 + 1);
                            accy = ResizeArray<double>(accy, 1, imu0Index * 2 + 1);
                            accz = ResizeArray<double>(accz, 1, imu0Index * 2 + 1);
                        }

                        ((IArrayOf<double>)time_imu0)[0, imu0Index] = (double)res.Item1 / (1000 * 1000 * 1000);

                        ((IArrayOf<double>)gyrox)[0, imu0Index] = ((Tuple<double, double, double, double, double, double>)val.Item2).Item1;

                        ((IArrayOf<double>)gyroy)[0, imu0Index] = ((Tuple<double, double, double, double, double, double>)val.Item2).Item2;

                        ((IArrayOf<double>)gyroz)[0, imu0Index] = ((Tuple<double, double, double, double, double, double>)val.Item2).Item3;

                        ((IArrayOf<double>)accx)[0, imu0Index] = ((Tuple<double, double, double, double, double, double>)val.Item2).Item4;

                        ((IArrayOf<double>)accy)[0, imu0Index] = ((Tuple<double, double, double, double, double, double>)val.Item2).Item5;

                        ((IArrayOf<double>)accz)[0, imu0Index] = ((Tuple<double, double, double, double, double, double>)val.Item2).Item6;

                        imu0Index++;
                    }
                    if (val.Item1 == RawReaderMode.Camera0 && _MatlabFormat.HasFlag(MatlabFormat.Camera0))
                    {
                        if (time_cam0.Dimensions.Length == 0 || cam0Index >= time_cam0.Dimensions[1])
                        {
                            time_cam0 = ResizeArray<double>(time_cam0, 1, cam0Index * 2 + 1);
                        }
                        ((IArrayOf<double>)time_cam0)[0, cam0Index] = (double)res.Item1 / (1000 * 1000 * 1000);
                        cam0Index++;
                    }
                }
            }

            if (_MatlabFormat.HasFlag(MatlabFormat.Imu0))
            {
                time_imu0 = ResizeArray<double>(time_imu0, 1, imu0Index);
                gyrox = ResizeArray<double>(gyrox, 1, imu0Index);
                gyroy = ResizeArray<double>(gyroy, 1, imu0Index);
                gyroz = ResizeArray<double>(gyroz, 1, imu0Index);
                accx = ResizeArray<double>(accx, 1, imu0Index);
                accy = ResizeArray<double>(accy, 1, imu0Index);
                accz = ResizeArray<double>(accz, 1, imu0Index);

                ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["time", 0] = time_imu0;
                ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyrox", 0] = gyrox;
                ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyroy", 0] = gyroy;
                ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyroz", 0] = gyroz;

                ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accx", 0] = accx;
                ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accy", 0] = accy;
                ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accz", 0] = accz;
            }

            if (_MatlabFormat.HasFlag(MatlabFormat.Camera0))
            {
                time_cam0 = ResizeArray<double>(time_cam0, 1, cam0Index);

                ((_DataStruct["raw", 0] as IStructureArray)["cam0", 0] as IStructureArray)["time", 0] = time_cam0;
            }
        }

        public void Close()
        {
            using (Stream stream = new FileStream(_FileName, FileMode.Create))
            {
                var writer = new MatFileWriter(stream);
                writer.Write(_DataBuilder.NewFile(new[] { _DataBuilder.NewVariable("data", _DataStruct) }));
            }
        }

        public void Open()
        {
            _DataBuilder = new DataBuilder();
            _DataStruct = _DataBuilder.NewStructureArray(new[] { "raw" }, 1);

            if (_MatlabFormat.HasFlag(MatlabFormat.Imu0))
            {
                _DataStruct["raw", 0] = AddFieldToStructureArray(_DataStruct["raw", 0], "imu0", 1);

                (_DataStruct["raw", 0] as IStructureArray)["imu0", 0] = AddFieldToStructureArray((_DataStruct["raw", 0] as IStructureArray)["imu0", 0], "time", 1);

                (_DataStruct["raw", 0] as IStructureArray)["imu0", 0] = AddFieldToStructureArray((_DataStruct["raw", 0] as IStructureArray)["imu0", 0], "gyrox", 1);
                (_DataStruct["raw", 0] as IStructureArray)["imu0", 0] = AddFieldToStructureArray((_DataStruct["raw", 0] as IStructureArray)["imu0", 0], "gyroy", 1);
                (_DataStruct["raw", 0] as IStructureArray)["imu0", 0] = AddFieldToStructureArray((_DataStruct["raw", 0] as IStructureArray)["imu0", 0], "gyroz", 1);

                (_DataStruct["raw", 0] as IStructureArray)["imu0", 0] = AddFieldToStructureArray((_DataStruct["raw", 0] as IStructureArray)["imu0", 0], "accx", 1);
                (_DataStruct["raw", 0] as IStructureArray)["imu0", 0] = AddFieldToStructureArray((_DataStruct["raw", 0] as IStructureArray)["imu0", 0], "accy", 1);
                (_DataStruct["raw", 0] as IStructureArray)["imu0", 0] = AddFieldToStructureArray((_DataStruct["raw", 0] as IStructureArray)["imu0", 0], "accz", 1);

                ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyrox", 0] = _DataBuilder.NewArray<double>(1, 0);
                ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyroy", 0] = _DataBuilder.NewArray<double>(1, 0);
                ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyroz", 0] = _DataBuilder.NewArray<double>(1, 0);

                ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accx", 0] = _DataBuilder.NewArray<double>(1, 0);
                ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accy", 0] = _DataBuilder.NewArray<double>(1, 0);
                ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accz", 0] = _DataBuilder.NewArray<double>(1, 0);
            }
            if (_MatlabFormat.HasFlag(MatlabFormat.Camera0))
            {
                _DataStruct["raw", 0] = AddFieldToStructureArray(_DataStruct["raw", 0], "cam0", 1);

                (_DataStruct["raw", 0] as IStructureArray)["cam0", 0] = AddFieldToStructureArray((_DataStruct["raw", 0] as IStructureArray)["cam0", 0], "time", 1);
            }
        }

        private IStructureArray AddFieldToStructureArray(IArray structureArray, string fieldName, params int[] dimensions)
        {
            return MatlabUtilities.AddFieldToStructureArray(_DataBuilder, structureArray, fieldName, dimensions);
        }

        private IArray ResizeArray<T>(IArray array, params int[] dimensions) where T : struct
        {
            return MatlabUtilities.ResizeArray<T>(_DataBuilder, array, dimensions);
        }
    }
}