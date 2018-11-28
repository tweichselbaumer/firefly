using System;

namespace FireFly.Proxy
{
    public class ImuDerivedEventData : AbstractProxyEventData
    {
        private double _AccelX;
        private double _AccelY;
        private double _AccelZ;
        private double _GyroX;
        private double _GyroY;
        private double _GyroZ;
        private double _Time;

        public double AccelX
        {
            get
            {
                return _AccelX;
            }
        }

        public double AccelY
        {
            get
            {
                return _AccelY;
            }
        }

        public double AccelZ
        {
            get
            {
                return _AccelZ;
            }
        }

        public double GyroX
        {
            get
            {
                return _GyroX;
            }
        }

        public double GyroY
        {
            get
            {
                return _GyroY;
            }
        }

        public double GyroZ
        {
            get
            {
                return _GyroZ;
            }
        }

        public int RawSize
        {
            get
            {
                return 8 * 7;
            }
        }

        public double Time
        {
            get
            {
                return _Time;
            }
        }

        public UInt64 TimeNanoSeconds
        {
            get
            {
                return (UInt64)(_Time * 1000 * 1000 * 1000);
            }
        }

        public byte[] GetRaw()
        {
            byte[] data = new byte[RawSize];

            Array.Copy(BitConverter.GetBytes(TimeNanoSeconds), 0, data, 0, sizeof(UInt64));

            Array.Copy(BitConverter.GetBytes(GyroX), 0, data, 8, sizeof(double));
            Array.Copy(BitConverter.GetBytes(GyroY), 0, data, 16, sizeof(double));
            Array.Copy(BitConverter.GetBytes(GyroZ), 0, data, 24, sizeof(double));

            Array.Copy(BitConverter.GetBytes(AccelX), 0, data, 32, sizeof(double));
            Array.Copy(BitConverter.GetBytes(AccelY), 0, data, 40, sizeof(double));
            Array.Copy(BitConverter.GetBytes(AccelZ), 0, data, 48, sizeof(double));

            return data;
        }

        internal static ImuDerivedEventData Parse(byte[] data, int offset)
        {
            ImuDerivedEventData obj = new ImuDerivedEventData();
            UInt64 time_ns = BitConverter.ToUInt64(data, 0);

            obj._Time = (double)time_ns / (1000 * 1000 * 1000);

            obj._GyroX = BitConverter.ToDouble(data, 8);
            obj._GyroY = BitConverter.ToDouble(data, 16);
            obj._GyroZ = BitConverter.ToDouble(data, 24);

            obj._AccelX = BitConverter.ToDouble(data, 32);
            obj._AccelY = BitConverter.ToDouble(data, 40);
            obj._AccelZ = BitConverter.ToDouble(data, 48);

            return obj;
        }
    }
}