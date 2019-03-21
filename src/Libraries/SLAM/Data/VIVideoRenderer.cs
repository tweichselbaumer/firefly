using Emgu.CV;
using Emgu.CV.CvEnum;
using FireFly.Data.Storage;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace FireFly.VI.SLAM.Data
{
    public class VIVideoRenderer
    {
        private string _FileName;
        private int _VideoHeight;
        private int _VideoWidth;
        private VideoWriter _VideoWriter;

        public VIVideoRenderer(string fileName, int width, int height)
        {
            _FileName = fileName;
            _VideoWidth = width;
            _VideoHeight = height;
        }

        public void Close()
        {
            if (_VideoWriter != null)
            {
                _VideoWriter.Dispose();
                _VideoWriter = null;
            }
        }

        public void Open()
        {
            _VideoWriter = new VideoWriter(_FileName, VideoWriter.Fourcc('M', 'P', '4', 'V'), 20, new Size(_VideoWidth, _VideoHeight), true);
        }

        public void Render(VIMatlabImporter matlabImporter, RawDataReader reader, Action<double> progress = null)
        {
            int count = reader.Count;

            int i = 0;

            List<KeyFrame> keyFrames = matlabImporter.GetKeyFrames();

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
                        Mat rawImage = new Mat();
                        CvInvoke.Imdecode(((Tuple<double, byte[]>)val.Item2).Item2, ImreadModes.Grayscale, rawImage);
                        WriteFrame(rawImage);
                    }
                }
            }
        }

        private void WriteFrame(Mat rawImage)
        {
            Image<Emgu.CV.Structure.Bgr, byte> imageResult = new Image<Emgu.CV.Structure.Bgr, byte>(_VideoWidth, _VideoHeight);
            imageResult.ROI = new Rectangle(0, 0, 512, 512);

            Mat rawImageColor = new Mat();

            CvInvoke.CvtColor(rawImage, rawImageColor, ColorConversion.Gray2Bgr);

            rawImageColor.CopyTo(imageResult);

            imageResult.ROI = Rectangle.Empty;

            _VideoWriter.Write(imageResult.Mat);
        }
    }
}