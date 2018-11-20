using FireFly.Data.Storage.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace FireFly.Settings
{
    public class ImuCalibrationSettings : AbstractSettings
    {
        private double _AccelerometerNoiseDensity;
        private double _AccelerometerNoiseDensitySafetyScale;
        private double _AccelerometerRandomWalk;
        private double _AccelerometerRandomWalkSafetyScale;
        private List<double> _AllanDeviationAccelerometerX = new List<double>();
        private List<double> _AllanDeviationAccelerometerY = new List<double>();
        private List<double> _AllanDeviationAccelerometerZ = new List<double>();
        private List<double> _AllanDeviationGyroscopeX = new List<double>();
        private List<double> _AllanDeviationGyroscopeY = new List<double>();
        private List<double> _AllanDeviationGyroscopeZ = new List<double>();
        private List<double> _AllanDeviationTime = new List<double>();
        private double _GyroscopeNoiseDensity;
        private double _GyroscopeNoiseDensitySafetyScale;
        private double _GyroscopeRandomWalk;
        private double _GyroscopeRandomWalkSafetyScale;
        private ImuModel _ImuModel;
        private double _SampleTime;

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

        public List<double> AllanDeviationAccelerometerX
        {
            get
            {
                return _AllanDeviationAccelerometerX;
            }

            set
            {
                _AllanDeviationAccelerometerX = value;
            }
        }

        public List<double> AllanDeviationAccelerometerY
        {
            get
            {
                return _AllanDeviationAccelerometerY;
            }

            set
            {
                _AllanDeviationAccelerometerY = value;
            }
        }

        public List<double> AllanDeviationAccelerometerZ
        {
            get
            {
                return _AllanDeviationAccelerometerZ;
            }

            set
            {
                _AllanDeviationAccelerometerZ = value;
            }
        }

        public List<double> AllanDeviationGyroscopeX
        {
            get
            {
                return _AllanDeviationGyroscopeX;
            }

            set
            {
                _AllanDeviationGyroscopeX = value;
            }
        }

        public List<double> AllanDeviationGyroscopeY
        {
            get
            {
                return _AllanDeviationGyroscopeY;
            }

            set
            {
                _AllanDeviationGyroscopeY = value;
            }
        }

        public List<double> AllanDeviationGyroscopeZ
        {
            get
            {
                return _AllanDeviationGyroscopeZ;
            }

            set
            {
                _AllanDeviationGyroscopeZ = value;
            }
        }

        public List<double> AllanDeviationTime
        {
            get
            {
                return _AllanDeviationTime;
            }

            set
            {
                _AllanDeviationTime = value;
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

        public double SampleTime
        {
            get
            {
                return _SampleTime;
            }

            set
            {
                _SampleTime = value;
            }
        }
    }
}