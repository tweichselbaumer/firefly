using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireFly.Proxy
{
    public class CameraEventData : AbstractProxyEventData
    {
        private Mat _Image = new Mat();
        private double _ExposureTime;

        public Mat Image
        {
            get
            {
                return _Image;
            }
        }

        public double ExposureTime
        {
            get
            {
                return _ExposureTime;
            }
        }

        public int RawSize
        {
            get
            {
                return sizeof(double) + _Image.Cols * _Image.Rows;
            }
        }

        internal static CameraEventData Parse(byte[] data, int offset, bool hasExposureTime, double exposureTime = 0)
        {
            CameraEventData obj = new CameraEventData();

            int calcOffset = offset;
            if (hasExposureTime)
                calcOffset += sizeof(double);

            byte[] temp = new byte[data.Length - calcOffset];

            if (hasExposureTime)
                obj._ExposureTime = BitConverter.ToDouble(data, offset);

            if (exposureTime != 0)
                obj._ExposureTime = exposureTime;

            Array.Copy(data, calcOffset, temp, 0, data.Length - calcOffset);
            CvInvoke.Imdecode(temp, Emgu.CV.CvEnum.ImreadModes.Grayscale, obj._Image);
            return obj;
        }
    }
}
