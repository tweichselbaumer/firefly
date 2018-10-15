using YamlDotNet.Serialization;

namespace FireFly.Data.Storage.Model
{
    public class ImuTarget
    {
        private double _AccelerometerNoiseDensity;
        private double _AccelerometerRandomWalk;
        private double _GyroscopeNoiseDensity;
        private double _GyroscopeRandomWalk;
        private string _RosTopic;
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
    }
}