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

        public Mat Image
        {
            get
            {
                return _Image;
            }
        }

        internal static CameraEventData Parse(byte[] data, int offset)
        {
            CameraEventData obj = new CameraEventData();
            byte[] temp = new byte[data.Length - offset];
            Array.Copy(data, offset, temp, 0, data.Length - offset);
            CvInvoke.Imdecode(temp, Emgu.CV.CvEnum.ImreadModes.Grayscale, obj._Image);
            return obj;
        }
    }
}
