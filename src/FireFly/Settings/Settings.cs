using System.Reflection;

namespace FireFly.Settings
{
    public class Settings : AbstractSettings
    {
        private CalibrationSettings _CalibrationSettings = new CalibrationSettings();
        private ConnectionSettings _ConnectionSettings = new ConnectionSettings();
        private GeneralSettings _GeneralSettings = new GeneralSettings();
        private ImuSettings _ImuSettings = new ImuSettings();
        private StreamingSettings _StreamingSettings = new StreamingSettings();

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

        public GeneralSettings GeneralSettings
        {
            get
            {
                return _GeneralSettings;
            }

            set
            {
                _GeneralSettings = value;
            }
        }

        public ImuSettings ImuSettings
        {
            get
            {
                return _ImuSettings;
            }

            set
            {
                _ImuSettings = value;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
        }
    }
}