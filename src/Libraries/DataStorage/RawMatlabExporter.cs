﻿using MatFileHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FireFly.Data.Storage
{
    [Flags]
    public enum MatlabFormat
    {
        Imu0 = 1,
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
            IArray time = ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["time", 0];

            IArray gyrox = ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyrox", 0];
            IArray gyroy = ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyroy", 0];
            IArray gyroz = ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyroz", 0];

            IArray accx = ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accx", 0];
            IArray accy = ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accy", 0];
            IArray accz = ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accz", 0];

            int imu0Index = 0;

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
                        if (imu0Index >= gyrox.Dimensions[1])
                        {
                            time = ResizeArray<double>(time, 1, imu0Index * 2 + 1);
                            gyrox = ResizeArray<double>(gyrox, 1, imu0Index * 2 + 1);
                            gyroy = ResizeArray<double>(gyroy, 1, imu0Index * 2 + 1);
                            gyroz = ResizeArray<double>(gyroz, 1, imu0Index * 2 + 1);
                            accx = ResizeArray<double>(accx, 1, imu0Index * 2 + 1);
                            accy = ResizeArray<double>(accy, 1, imu0Index * 2 + 1);
                            accz = ResizeArray<double>(accz, 1, imu0Index * 2 + 1);
                        }

                        ((IArrayOf<double>)time)[0, imu0Index] = (double)res.Item1 / (1000 * 1000 * 1000);

                        ((IArrayOf<double>)gyrox)[0, imu0Index] = ((Tuple<double, double, double, double, double, double>)val.Item2).Item1;

                        ((IArrayOf<double>)gyroy)[0, imu0Index] = ((Tuple<double, double, double, double, double, double>)val.Item2).Item2;

                        ((IArrayOf<double>)gyroz)[0, imu0Index] = ((Tuple<double, double, double, double, double, double>)val.Item2).Item3;

                        ((IArrayOf<double>)accx)[0, imu0Index] = ((Tuple<double, double, double, double, double, double>)val.Item2).Item4;

                        ((IArrayOf<double>)accy)[0, imu0Index] = ((Tuple<double, double, double, double, double, double>)val.Item2).Item5;

                        ((IArrayOf<double>)accz)[0, imu0Index] = ((Tuple<double, double, double, double, double, double>)val.Item2).Item6;

                        imu0Index++;
                    }
                }
            }

            if (_MatlabFormat.HasFlag(MatlabFormat.Imu0))
            {
                time = ResizeArray<double>(time, 1, imu0Index);
                gyrox = ResizeArray<double>(gyrox, 1, imu0Index);
                gyroy = ResizeArray<double>(gyroy, 1, imu0Index);
                gyroz = ResizeArray<double>(gyroz, 1, imu0Index);
                accx = ResizeArray<double>(accx, 1, imu0Index);
                accy = ResizeArray<double>(accy, 1, imu0Index);
                accz = ResizeArray<double>(accz, 1, imu0Index);
            }

            ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["time", 0] = time;
            ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyrox", 0] = gyrox;
            ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyroy", 0] = gyroy;
            ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["gyroz", 0] = gyroz;

            ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accx", 0] = accx;
            ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accy", 0] = accy;
            ((_DataStruct["raw", 0] as IStructureArray)["imu0", 0] as IStructureArray)["accz", 0] = accz;
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
        }

        private IStructureArray AddFieldToStructureArray(IArray structureArray, string fieldName, params int[] dimensions)
        {
            IStructureArray result;
            if (structureArray == null || structureArray.IsEmpty || !(structureArray is IStructureArray))
            {
                result = _DataBuilder.NewStructureArray(new List<string> { fieldName }, dimensions);
            }
            else
            {
                result = _DataBuilder.NewStructureArray(new List<string> { fieldName }.Union((structureArray as IStructureArray).FieldNames), dimensions);
                foreach (string field in (structureArray as IStructureArray).FieldNames)
                {
                    for (int i = 0; i < (structureArray as IStructureArray).Count && i < result.Count; i++)
                    {
                        result[field, i] = (structureArray as IStructureArray)[field, i];
                    }
                }
            }

            return result;
        }

        private IArray ResizeArray<T>(IArray array, params int[] dimensions) where T : struct
        {
            IArrayOf<T> result;

            if (array == null || array.IsEmpty || !(array is IArrayOf<T>))
            {
                result = _DataBuilder.NewArray<T>(dimensions);
            }
            else
            {
                result = _DataBuilder.NewArray<T>(dimensions);

                for (int i = 0; i < array.Count && i < result.Count; i++)
                {
                    result[i] = (array as IArrayOf<T>)[i];
                }
            }

            return result;
        }
    }
}