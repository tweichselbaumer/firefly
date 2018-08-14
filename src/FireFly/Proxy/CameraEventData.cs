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

        internal static CameraEventData Parse(byte[] data, int offset)
        {
            CameraEventData obj = new CameraEventData();

            byte[] temp = new byte[data.Length - (offset + sizeof(double))];

            obj._ExposureTime = BitConverter.ToDouble(data, offset);

            Array.Copy(data, (offset + sizeof(double)), temp, 0, data.Length - (offset + sizeof(double)));
            CvInvoke.Imdecode(temp, Emgu.CV.CvEnum.ImreadModes.Grayscale, obj._Image);
            return obj;
        }
    }
}
