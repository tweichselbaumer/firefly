using FireFly.Data.Storage.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FireFly.Settings
{
    public class ImuCalibrationSettings : AbstractSettings
    {
        private double _AccelerometerNoiseDensity;
        private double _AccelerometerNoiseDensitySafetyScale;
        private double _AccelerometerRandomWalk;
        private double _AccelerometerRandomWalkSafetyScale;
        private double _GyroscopeNoiseDensity;
        private double _GyroscopeNoiseDensitySafetyScale;
        private double _GyroscopeRandomWalk;
        private double _GyroscopeRandomWalkSafetyScale;
        private ImuModel _ImuModel;

        public double AccelerometerNoiseDensity
        {
            get
            {
                return _AccelerometerNoiseDensity;
            }

            set
            {
                _AccelerometerNoiseDensity = value;
            }
        }

        public double AccelerometerNoiseDensitySafetyScale
        {
            get
            {
                return _AccelerometerNoiseDensitySafetyScale;
            }

            set
            {
                _AccelerometerNoiseDensitySafetyScale = value;
            }
        }

        public double AccelerometerRandomWalk
        {
            get
            {
                return _AccelerometerRandomWalk;
            }

            set
            {
                _AccelerometerRandomWalk = value;
            }
        }

        public double AccelerometerRandomWalkSafetyScale
        {
            get
            {
                return _AccelerometerRandomWalkSafetyScale;
            }

            set
            {
                _AccelerometerRandomWalkSafetyScale = value;
            }
        }

        public double GyroscopeNoiseDensity
        {
            get
            {
                return _GyroscopeNoiseDensity;
            }

            set
            {
                _GyroscopeNoiseDensity = value;
            }
        }

        public double GyroscopeNoiseDensitySafetyScale
        {
            get
            {
                return _GyroscopeNoiseDensitySafetyScale;
            }

            set
            {
                _GyroscopeNoiseDensitySafetyScale = value;
            }
        }

        public double GyroscopeRandomWalk
        {
            get
            {
                return _GyroscopeRandomWalk;
            }

            set
            {
                _GyroscopeRandomWalk = value;
            }
        }

        public double GyroscopeRandomWalkSafetyScale
        {
            get
            {
                return _GyroscopeRandomWalkSafetyScale;
            }

            set
            {
                _GyroscopeRandomWalkSafetyScale = value;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public ImuModel ImuModel
        {
            get
            {
                return _ImuModel;
            }

            set
            {
                _ImuModel = value;
            }
        }
    }
}