namespace FireFly.Settings
{
    public class StreamingSettings
    {
        private int _Quality;

        private bool _Enabled;

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

        public bool Enabled
        {
            get
            {
                return _Enabled;
            }

            set
            {
                _Enabled = value;
            }
        }
    }
}