using System.Collections.Generic;
using static Emgu.CV.Aruco.Dictionary;

namespace FireFly.Settings
{
    public class IntrinsicCalibrationSettings : AbstractSettings
    {
        private double _Alpha;
        private double _Cx;

        private double _Cy;

        private PredefinedDictionaryName _Dictionary;
        private List<double> _DistCoeffs = new List<double>();

        private double _Fx;

        private double _Fy;

        private float _MarkerLength;

        private float _SquareLength;

        private int _SquaresX;

        private int _SquaresY;

        private double _FOVScale;

        public double Cx
        {
            get
            {
                return _Cx;
            }

            set
            {
                _Cx = value;
            }
        }

        public double Cy
        {
            get
            {
                return _Cy;
            }

            set
            {
                _Cy = value;
            }
        }

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

        public List<double> DistCoeffs
        {
            get
            {
                return _DistCoeffs;
            }

            set
            {
                _DistCoeffs = value;
            }
        }

        public double Fx
        {
            get
            {
                return _Fx;
            }

            set
            {
                _Fx = value;
            }
        }

        public double Fy
        {
            get
            {
                return _Fy;
            }

            set
            {
                _Fy = value;
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

        public double Alpha
        {
            get
            {
                return _Alpha;
            }

            set
            {
                _Alpha = value;
            }
        }

        public double FOVScale
        {
            get
            {
                return _FOVScale;
            }

            set
            {
                _FOVScale = value;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
        }
    }
}