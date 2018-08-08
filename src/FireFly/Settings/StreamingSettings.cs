using System.Reflection;

namespace FireFly.Settings
{
    public class StreamingSettings : AbstractSettings
    {
        private bool _CameraRawStreamEnabled;

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