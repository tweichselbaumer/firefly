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
        private long _TimeMicroSeconds;

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

        public long TimeMicroSeconds
        {
            get
            {
                return _TimeMicroSeconds;
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
                //TODO
                return (long)(_Time * 1000 * 1000 * 1000);
            }
        }

        internal static ImuEventData Parse(byte[] data, int offset)
        {
            ImuEventData obj = new ImuEventData();
            obj._Time = ((double)BitConverter.ToUInt32(data, 0)) / 1000;
            obj._TimeMicroSeconds = BitConverter.ToUInt32(data, 4);

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
    }
}