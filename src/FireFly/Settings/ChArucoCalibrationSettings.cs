using static Emgu.CV.Aruco.Dictionary;

namespace FireFly.Settings
{
    public class ChArucoCalibrationSettings : AbstractSettings
    {
        private PredefinedDictionaryName _Dictionary;
        private float _MarkerLength;

        private float _SquareLength;

        private int _SquaresX;

        private int _SquaresY;

        public PredefinedDictionaryName Dictionary
        {
            get
            {
                return _Dictionary;
            }

            set
            {
                _Dictionary = value;
            }
        }

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