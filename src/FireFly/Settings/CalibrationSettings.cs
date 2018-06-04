namespace FireFly.Settings
{
    public class CalibrationSettings
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
    }
}