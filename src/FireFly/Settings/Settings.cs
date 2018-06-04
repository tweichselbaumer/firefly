namespace FireFly.Settings
{
    public class Settings
    {
        private ConnectionSettings _ConnectionSettings = new ConnectionSettings();

        private StreamingSettings _StreamingSettings = new StreamingSettings();

        private CalibrationSettings _CalibrationSettings = new CalibrationSettings();

        public ConnectionSettings ConnectionSettings
        {
            get
            {
                return _ConnectionSettings;
            }

            set
            {
                _ConnectionSettings = value;
            }
        }

        public StreamingSettings StreamingSettings
        {
            get
            {
                return _StreamingSettings;
            }

            set
            {
                _StreamingSettings = value;
            }
        }

        public CalibrationSettings CalibrationSettings
        {
            get
            {
                return _CalibrationSettings;
            }

            set
            {
                _CalibrationSettings = value;
            }
        }
    }
}