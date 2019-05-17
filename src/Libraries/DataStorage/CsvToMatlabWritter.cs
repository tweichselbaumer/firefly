using System.Collections.Generic;
using System.Globalization;

namespace FireFly.Data.Storage
{
    public class CsvToMatlabWritter
    {
        private GenericToMatlabWritter _GenericToMatlabWritter;

        public CsvToMatlabWritter(string filename)
        {
            _GenericToMatlabWritter = new GenericToMatlabWritter(filename);
        }

        public void Save()
        {
            _GenericToMatlabWritter.Save();
        }

        public void Write(Dictionary<string, string> data, string name)
        {
            Dictionary<string, List<List<double>>> dataNew = new Dictionary<string, List<List<double>>>();

            foreach (string var in data.Keys)
            {
                dataNew.Add(var, ConvertToDouble(data[var]));
            }
            _GenericToMatlabWritter.Write(dataNew, name);
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
    }
}