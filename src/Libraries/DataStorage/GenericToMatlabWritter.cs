using FireFly.Utilities;
using MatFileHandler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FireFly.Data.Storage
{
    public class GenericToMatlabWritter
    {
        private DataBuilder _DataBuilder;
        private IStructureArray _DataStruct;
        private string _FileName;

        public GenericToMatlabWritter(string filename)
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

        public void Write(Dictionary<string, List<List<double>>> data, string name)
        {
            _DataBuilder = new DataBuilder();
            _DataStruct = _DataBuilder.NewStructureArray(new[] { name }, 1);

            foreach (string var in data.Keys)
            {
                List<List<double>> values = data[var];

                int n = values.Count;
                int m = values.Max(c => c.Count);

                _DataStruct[name, 0] = MatlabUtilities.AddFieldToStructureArray(_DataBuilder, _DataStruct[name, 0], var, 1);
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
    }
}