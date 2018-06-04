namespace FireFly.Settings
{
    public class IntrinsicCalibrationSettings
    {
        private float _MarkerLength;
        private float _SquareLength;
        private int _SquaresX;
        private int _SquaresY;

        public float MarkerLength
        {
            get
            {
                return _MarkerLength;
            }

            set
            {
                _MarkerLength = value;
            }
        }

        public float SquareLength
        {
            get
            {
                return _SquareLength;
            }

            set
            {
                _SquareLength = value;
            }
        }

        public int SquaresX
        {
            get
            {
                return _SquaresX;
            }

            set
            {
                _SquaresX = value;
            }
        }

        public int SquaresY
        {
            get
            {
                return _SquaresY;
            }

            set
            {
                _SquaresY = value;
            }
        }
    }
}