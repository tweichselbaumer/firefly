using FireFly.Utilities;
using MatFileHandler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FireFly.Data.Storage
{
    public class CsvToMatlabWritter
    {
        private DataBuilder _DataBuilder;
        private IStructureArray _DataStruct;
        private string _FileName;

        public CsvToMatlabWritter(string filename)
        {
            _FileName = filename;
        }

        public void Save()
        {
            using (Stream stream = new FileStream(_FileName, FileMode.Create))
            {
                var writer = new MatFileWriter(stream);
                writer.Write(_DataBuilder.NewFile(new[] { _DataBuilder.NewVariable("data", _DataStruct) }));
            }
        }

        public void Write(Dictionary<string, string> data, string name)
        {
            _DataBuilder = new DataBuilder();
            _DataStruct = _DataBuilder.NewStructureArray(new[] { name }, 1);

            foreach (string var in data.Keys)
            {
                List<List<double>> values = ConvertToDouble(data[var]);

                int n = values.Count;
                int m = values.Max(c => c.Count);

                _DataStruct[name, 0] = AddFieldToStructureArray(_DataStruct[name, 0], var, 1);
                (_DataStruct[name, 0] as IStructureArray)[var, 0] = _DataBuilder.NewArray<double>(n, m);
                IArrayOf<double> array = (IArrayOf<double>)(_DataStruct[name, 0] as IStructureArray)[var, 0];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        if (values[i] != null && j < values[i].Count)
                            array[i, j] = values[i][j];
                    }
                }
            }
        }

        private IStructureArray AddFieldToStructureArray(IArray structureArray, string fieldName, params int[] dimensions)
        {
            return MatlabUtilities.AddFieldToStructureArray(_DataBuilder, structureArray, fieldName, dimensions);
        }

        private List<List<double>> ConvertToDouble(string data)
        {
            List<List<double>> result = new List<List<double>>();

            if (!string.IsNullOrEmpty(data))
            {
                foreach (string line in data.Split('\n'))
                {
                    if (!string.IsNullOrEmpty(line.Trim()))
                    {
                        List<double> linedata = new List<double>();

                        foreach (string val in line.Split(','))
                        {
                            double v;
                            if (!string.IsNullOrEmpty(val) && double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                            {
                                linedata.Add(v);
                            }
                            else
                            {
                                linedata.Add(double.NaN);
                            }
                        }

                        result.Add(linedata);
                    }
                }
            }

            return result;
        }

        private IArray ResizeArray<T>(IArray array, params int[] dimensions) where T : struct
        {
            return MatlabUtilities.ResizeArray<T>(_DataBuilder, array, dimensions);
        }
    }
}