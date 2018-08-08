using System.Reflection;

namespace FireFly.Settings
{
    public class CalibrationSettings : AbstractSettings
    {
        private IntrinsicCalibrationSettings _IntrinsicCalibrationSettings = new IntrinsicCalibrationSettings();

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

        public override void SetDefaults()
        {
            base.SetDefaults();
        }
    }
}