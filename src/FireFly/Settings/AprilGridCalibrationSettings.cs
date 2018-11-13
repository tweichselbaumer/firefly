namespace FireFly.Settings
{
    public class AprilGridCalibrationSettings : AbstractSettings
    {
        private double _TagSize;
        private double _TagSpacingFactor;
        private int _TagsX;
        private int _TagsY;

        public double TagSize
        {
            get
            {
                return _TagSize;
            }

            set
            {
                _TagSize = value;
            }
        }

        public double TagSpacingFactor
        {
            get
            {
                return _TagSpacingFactor;
            }

            set
            {
                _TagSpacingFactor = value;
            }
        }

        public int TagsX
        {
            get
            {
                return _TagsX;
            }

            set
            {
                _TagsX = value;
            }
        }

        public int TagsY
        {
            get
            {
                return _TagsY;
            }

            set
            {
                _TagsY = value;
            }
        }
    }
}