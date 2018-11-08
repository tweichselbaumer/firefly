namespace FireFly.Settings
{
    public class CameraSettings : AbstractSettings
    {
        private int _Height;
        private int _Width;

        public int Height
        {
            get
            {
                return _Height;
            }

            set
            {
                _Height = value;
            }
        }

        public int Width
        {
            get
            {
                return _Width;
            }

            set
            {
                _Width = value;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Width = 512;
            Height = 512;
        }
    }
}