namespace FireFly.Settings
{
    public class StreamingSettings : AbstractSettings
    {
        private bool _CameraRawStreamEnabled;
        private bool _ImuDerivedStreamEnabled;
        private bool _ImuRawStreamEnabled;

        public bool CameraRawStreamEnabled
        {
            get
            {
                return _CameraRawStreamEnabled;
            }

            set
            {
                _CameraRawStreamEnabled = value;
            }
        }

        public bool ImuDerivedStreamEnabled
        {
            get
            {
                return _ImuDerivedStreamEnabled;
            }

            set
            {
                _ImuDerivedStreamEnabled = value;
            }
        }

        public bool ImuRawStreamEnabled
        {
            get
            {
                return _ImuRawStreamEnabled;
            }

            set
            {
                _ImuRawStreamEnabled = value;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
        }
    }
}