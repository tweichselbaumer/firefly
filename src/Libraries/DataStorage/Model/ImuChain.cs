using YamlDotNet.Serialization;

namespace FireFly.Data.Storage.Model
{
    public enum ImuModel
    {
        Calibrated,
        ScaleMisalignment,
        ScaleMisalignmentSizeEffect
    }

    public class Accelerometer
    {
        private double[][] _M;
        private double[] _Rx;
        private double[] _Ry;
        private double[] _Rz;

        [YamlMember(Alias = "M", ApplyNamingConventions = false)]
        public double[][] M
        {
            get
            {
                return _M;
            }

            set
            {
                _M = value;
            }
        }

        [YamlMember(Alias = "rx_i", ApplyNamingConventions = false)]
        public double[] Rx
        {
            get
            {
                return _Rx;
            }

            set
            {
                _Rx = value;
            }
        }

        [YamlMember(Alias = "ry_i", ApplyNamingConventions = false)]
        public double[] Ry
        {
            get
            {
                return _Ry;
            }

            set
            {
                _Ry = value;
            }
        }

        [YamlMember(Alias = "rz_i", ApplyNamingConventions = false)]
        public double[] Rz
        {
            get
            {
                return _Rz;
            }

            set
            {
                _Rz = value;
            }
        }
    }

    public class Gyroscope
    {
        private double[][] _A;
        private double[][] _C;
        private double[][] _M;

        [YamlMember(Alias = "A", ApplyNamingConventions = false)]
        public double[][] A
        {
            get
            {
                return _A;
            }

            set
            {
                _A = value;
            }
        }

        [YamlMember(Alias = "C_gyro_i", ApplyNamingConventions = false)]
        public double[][] C
        {
            get
            {
                return _C;
            }

            set
            {
                _C = value;
            }
        }

        [YamlMember(Alias = "M", ApplyNamingConventions = false)]
        public double[][] M
        {
            get
            {
                return _M;
            }

            set
            {
                _M = value;
            }
        }
    }

    public class Imu
    {
        private double _AccelerometerNoiseDensity;
        private double _AccelerometerRandomWalk;
        private Accelerometer _Accelerometers;
        private double _GyroscopeNoiseDensity;
        private double _GyroscopeRandomWalk;
        private Gyroscope _Gyroscopes;
        private ImuModel _ImuModel;
        private string _RosTopic;
        private double _TimeOffset;
        private double[][] _TImuBaseline;
        private double _UpdateRate;

        [YamlMember(Alias = "accelerometer_noise_density", ApplyNamingConventions = false)]
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

        [YamlMember(Alias = "accelerometer_random_walk", ApplyNamingConventions = false)]
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

        public Accelerometer Accelerometers
        {
            get
            {
                return _Accelerometers;
            }

            set
            {
                _Accelerometers = value;
            }
        }

        [YamlMember(Alias = "gyroscope_noise_density", ApplyNamingConventions = false)]
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

        [YamlMember(Alias = "gyroscope_random_walk", ApplyNamingConventions = false)]
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

        public Gyroscope Gyroscopes
        {
            get
            {
                return _Gyroscopes;
            }

            set
            {
                _Gyroscopes = value;
            }
        }

        [YamlIgnore]
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

        [YamlMember(Alias = "model", ApplyNamingConventions = false)]
        public string ImuModelString
        {
            get
            {
                return ConvertImuModelToString(_ImuModel);
            }

            set
            {
                _ImuModel = ConvertStringToImuModel(value);
            }
        }

        [YamlMember(Alias = "rostopic", ApplyNamingConventions = false)]
        public string RosTopic
        {
            get
            {
                return _RosTopic;
            }

            set
            {
                _RosTopic = value;
            }
        }

        [YamlMember(Alias = "time_offset", ApplyNamingConventions = false)]
        public double TimeOffset
        {
            get
            {
                return _TimeOffset;
            }

            set
            {
                _TimeOffset = value;
            }
        }

        [YamlMember(Alias = "T_i_b", ApplyNamingConventions = false)]
        public double[][] TImuBaseline
        {
            get
            {
                return _TImuBaseline;
            }

            set
            {
                _TImuBaseline = value;
            }
        }

        [YamlMember(Alias = "update_rate", ApplyNamingConventions = false)]
        public double UpdateRate
        {
            get
            {
                return _UpdateRate;
            }

            set
            {
                _UpdateRate = value;
            }
        }

        public static string ConvertImuModelToString(ImuModel model)
        {
            switch (model)
            {
                case ImuModel.Calibrated:
                    return "calibrated";

                case ImuModel.ScaleMisalignment:
                    return "scale-misalignment";

                case ImuModel.ScaleMisalignmentSizeEffect:
                    return "scale-misalignment-size-effect";

                default:
                    return "calibrated";
            }
        }

        public static ImuModel ConvertStringToImuModel(string model)
        {
            switch (model)
            {
                case "calibrated":
                    return ImuModel.Calibrated;

                case "scale-misalignment":
                    return ImuModel.ScaleMisalignment;

                case "scale-misalignment-size-effect":
                    return ImuModel.ScaleMisalignmentSizeEffect;

                default:
                    return ImuModel.Calibrated;
            }
        }
    }

    public class ImuCain
    {
        private Imu _Imu0;

        public Imu Imu0
        {
            get
            {
                return _Imu0;
            }

            set
            {
                _Imu0 = value;
            }
        }
    }
}