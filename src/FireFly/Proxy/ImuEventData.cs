using System;

namespace FireFly.Proxy
{
    public class ImuEventData : AbstractProxyEventData
    {
        private double _AccelX;
        private double _AccelY;
        private double _AccelZ;
        private double _GyroX;
        private double _GyroY;
        private double _GyroZ;
        private bool _HasCameraImage;
        private double _Temperatur;
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

        public bool HasCameraImage
        {
            get
            {
                return _HasCameraImage;
            }
        }

        public int RawSize
        {
            get
            {
                return 23;
            }
        }

        public double Temperatur
        {
            get
            {
                return _Temperatur;
            }
        }

        public double Time
        {
            get
            {
                return _Time;
            }
        }

        public long TimeNanoSeconds
        {
            get
            {
                return (long)(_Time * 1000 * 1000 * 1000);
            }
        }

        public byte[] GetRaw(double gyroScale, double accScale, double tempScale, double tempOffset)
        {
            byte[] data = new byte[RawSize];

            Array.Copy(BitConverter.GetBytes((UInt32)(TimeNanoSeconds / (1000 * 1000))), 0, data, 0, sizeof(UInt32));
            Array.Copy(BitConverter.GetBytes((UInt32)(TimeNanoSeconds / (1000))), 0, data, 4, sizeof(UInt32));

            Array.Copy(BitConverter.GetBytes((UInt16)(GyroX * gyroScale)), 0, data, 8, sizeof(UInt16));
            Array.Copy(BitConverter.GetBytes((UInt16)(GyroY * gyroScale)), 0, data, 10, sizeof(UInt16));
            Array.Copy(BitConverter.GetBytes((UInt16)(GyroZ * gyroScale)), 0, data, 12, sizeof(UInt16));

            Array.Copy(BitConverter.GetBytes((UInt16)(AccelX * accScale)), 0, data, 14, sizeof(UInt16));
            Array.Copy(BitConverter.GetBytes((UInt16)(AccelY * accScale)), 0, data, 16, sizeof(UInt16));
            Array.Copy(BitConverter.GetBytes((UInt16)(AccelZ * accScale)), 0, data, 18, sizeof(UInt16));

            Array.Copy(BitConverter.GetBytes((UInt16)((Temperatur - tempOffset) * tempScale)), 0, data, 20, sizeof(UInt16));

            Array.Copy(BitConverter.GetBytes(HasCameraImage), 0, data, 22, sizeof(bool));

            return data;
        }

        internal static ImuEventData Parse(byte[] data, int offset, double gyroScale, double accScale, double tempScale, double tempOffset)
        {
            ImuEventData obj = new ImuEventData();
            UInt32 time_ms = BitConverter.ToUInt32(data, 0);
            UInt32 time_us = BitConverter.ToUInt32(data, 4);

            int multi = (int)(time_ms / ((Math.Pow(2, 32)) / 1000));

            obj._Time = (time_us + multi * Math.Pow(2, 32)) / (1000 * 1000);

            obj._GyroX = ((double)BitConverter.ToInt16(data, 8)) / gyroScale;
            obj._GyroY = ((double)BitConverter.ToInt16(data, 10)) / gyroScale;
            obj._GyroZ = ((double)BitConverter.ToInt16(data, 12)) / gyroScale;

            obj._AccelX = ((double)BitConverter.ToInt16(data, 14)) / accScale;
            obj._AccelY = ((double)BitConverter.ToInt16(data, 16)) / accScale;
            obj._AccelZ = ((double)BitConverter.ToInt16(data, 18)) / accScale;

            obj._Temperatur = ((double)BitConverter.ToInt16(data, 20)) / tempScale + tempOffset;

            obj._HasCameraImage = BitConverter.ToBoolean(data, 22);

            return obj;
        }

        internal static ImuEventData Parse(long timestamp, Tuple<double, double, double, double, double, double> item, bool hasCamera)
        {
            ImuEventData obj = new ImuEventData();
            obj._Time = (double)timestamp / (1000 * 1000 * 1000);

            obj._GyroX = item.Item1;
            obj._GyroY = item.Item2;
            obj._GyroZ = item.Item3;

            obj._AccelX = item.Item4;
            obj._AccelY = item.Item5;
            obj._AccelZ = item.Item6;

            //TODO:
            obj._Temperatur = 0;

            obj._HasCameraImage = hasCamera;

            return obj;
        }
    }
}