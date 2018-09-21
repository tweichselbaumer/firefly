using System.Reflection;

namespace FireFly.Settings
{
    public class CalibrationSettings : AbstractSettings
    {
        private IntrinsicCalibrationSettings _IntrinsicCalibrationSettings = new IntrinsicCalibrationSettings();
        private PhotometricCalibrationSettings _PhotometricCalibrationSettings = new PhotometricCalibrationSettings();
        public IntrinsicCalibrationSettings IntrinsicCalibrationSettings
        {
            get
            {
                return _IntrinsicCalibrationSettings;
            }

            set
            {
                _IntrinsicCalibrationSettings = value;
            }
        }

        public PhotometricCalibrationSettings PhotometricCalibrationSettings
        {
            get
            {
                return _PhotometricCalibrationSettings;
            }

            set
            {
                _PhotometricCalibrationSettings = value;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
        }
    }
}