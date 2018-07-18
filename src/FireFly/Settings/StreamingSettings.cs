namespace FireFly.Settings
{
    public class StreamingSettings
    {
        private int _Quality;

        private bool _CameraRawStreamEnabled;

        private bool _ImuRawStreamEnabled;

        public int Quality
        {
            get
            {
                return _Quality;
            }

            set
            {
                _Quality = value;
            }
        }

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
    }
}