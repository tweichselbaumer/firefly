using FireFly.VI.SLAM;
using System;

namespace FireFly.Proxy
{
    public class SlamEventData : AbstractProxyEventData
    {
        private Frame _Frame;

        public Frame Frame
        {
            get
            {
                return _Frame;
            }

            set
            {
                _Frame = value;
            }
        }

        internal static SlamEventData Parse(byte[] data)
        {
            SlamEventData obj = new SlamEventData();
            int index = 1;
            uint id = BitConverter.ToUInt32(data, index);
            index += 4;
            double tx = BitConverter.ToDouble(data, index);
            index += 8;
            double ty = BitConverter.ToDouble(data, index);
            index += 8;
            double tz = BitConverter.ToDouble(data, index);
            index += 8;
            double q1 = BitConverter.ToDouble(data, index);
            index += 8;
            double q2 = BitConverter.ToDouble(data, index);
            index += 8;
            double q3 = BitConverter.ToDouble(data, index);
            index += 8;
            double q4 = BitConverter.ToDouble(data, index);
            index += 8;
            double s = BitConverter.ToDouble(data, index);

            obj._Frame = Frame.CreateFrame(id, tx, ty, tz, q1, q2, q3, q4, s);

            return obj;
        }
    }
}