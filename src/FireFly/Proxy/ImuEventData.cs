using System;

namespace FireFly.Proxy
{
    public class ImuEventData : AbstractProxyEventData
    {
        private const double ACC_SCALE = 2048 / 9.80665;
        private const double GYRO_SCALE = 16.4;
        private const double TEMP_OFFSET = 21;
        private const double TEMP_SCALE = 333.8;
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

        public int RawSize
        {
            get
            {
                return 23;
            }
        }

        public byte[] Raw
        {
            get
            {
                byte[] data = new byte[23];

                Array.Copy(BitConverter.GetBytes((UInt32)(TimeNanoSeconds / (1000 * 1000))), 0, data, 0, sizeof(UInt32));
                Array.Copy(BitConverter.GetBytes((UInt32)(TimeNanoSeconds / (1000))), 0, data, 4, sizeof(UInt32));

                Array.Copy(BitConverter.GetBytes((UInt16)(GyroX * GYRO_SCALE)), 0, data, 8, sizeof(UInt16));
                Array.Copy(BitConverter.GetBytes((UInt16)(GyroY * GYRO_SCALE)), 0, data, 10, sizeof(UInt16));
                Array.Copy(BitConverter.GetBytes((UInt16)(GyroZ * GYRO_SCALE)), 0, data, 12, sizeof(UInt16));

                Array.Copy(BitConverter.GetBytes((UInt16)(AccelX * ACC_SCALE)), 0, data, 14, sizeof(UInt16));
                Array.Copy(BitConverter.GetBytes((UInt16)(AccelY * ACC_SCALE)), 0, data, 16, sizeof(UInt16));
                Array.Copy(BitConverter.GetBytes((UInt16)(AccelZ * ACC_SCALE)), 0, data, 18, sizeof(UInt16));

                Array.Copy(BitConverter.GetBytes((UInt16)((Temperatur - TEMP_OFFSET) * TEMP_SCALE)), 0, data, 20, sizeof(UInt16));

                Array.Copy(BitConverter.GetBytes(HasCameraImage), 0, data, 22, sizeof(bool));

                return data;
            }
        }

        internal static ImuEventData Parse(byte[] data, int offset)
        {
            ImuEventData obj = new ImuEventData();
            UInt32 time_ms = BitConverter.ToUInt32(data, 0);
            UInt32 time_us = BitConverter.ToUInt32(data, 4);

            int multi = (int)(time_ms / ((Math.Pow(2, 32)) / 1000));

            obj._Time = (time_us + multi * Math.Pow(2, 32)) / (1000 * 1000);

            obj._GyroX = ((double)BitConverter.ToInt16(data, 8)) / GYRO_SCALE;
            obj._GyroY = ((double)BitConverter.ToInt16(data, 10)) / GYRO_SCALE;
            obj._GyroZ = ((double)BitConverter.ToInt16(data, 12)) / GYRO_SCALE;

            obj._AccelX = ((double)BitConverter.ToInt16(data, 14)) / ACC_SCALE;
            obj._AccelY = ((double)BitConverter.ToInt16(data, 16)) / ACC_SCALE;
            obj._AccelZ = ((double)BitConverter.ToInt16(data, 18)) / ACC_SCALE;

            obj._Temperatur = ((double)BitConverter.ToInt16(data, 20)) / TEMP_SCALE + TEMP_OFFSET;

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