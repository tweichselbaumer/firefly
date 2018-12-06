using Emgu.CV;
using System;
using System.Collections.Generic;

namespace FireFly.Data.Storage
{
    public class MemoryImageExporter
    {
        private List<Mat> _Images = new List<Mat>();

        public MemoryImageExporter()
        {
        }

        public List<Mat> Images
        {
            get
            {
                return _Images;
            }
        }

        public void AddFromReader(RawDataReader reader, Action<double> progress = null)
        {
            int count = reader.Count;

            int i = 0;
            int j = 0;

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
                    if (val.Item1 == RawReaderMode.Camera0)
                    {
                        Tuple<double, byte[]> item = (Tuple<double, byte[]>)val.Item2;
                        Mat m = new Mat();
                        CvInvoke.Imdecode(item.Item2, Emgu.CV.CvEnum.ImreadModes.Grayscale, m);
                        _Images.Add(m);
                    }
                }
            }
        }
    }
}