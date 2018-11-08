using Emgu.CV;
using System;

namespace FireFly.Proxy
{
    public class CameraEventData : AbstractProxyEventData
    {
        private double _ExposureTime;
        private Mat _Image = new Mat();
        private int _RawSize;

        public double ExposureTime
        {
            get
            {
                return _ExposureTime;
            }
        }

        public Mat Image
        {
            get
            {
                return _Image;
            }
        }

        public int RawSize
        {
            get
            {
                return sizeof(double) + _RawSize;
            }
        }

        internal static CameraEventData Parse(byte[] data, int offset, bool hasExposureTime, double exposureTime = 0)
        {
            CameraEventData obj = new CameraEventData();
            obj._RawSize = data.Length;
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