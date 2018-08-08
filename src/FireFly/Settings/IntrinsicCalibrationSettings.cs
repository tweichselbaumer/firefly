using System.Collections.Generic;
using System.Reflection;

namespace FireFly.Settings
{
    public class IntrinsicCalibrationSettings : AbstractSettings
    {
        private double _Cx;

        private double _Cy;

        private List<double> _DistCoeffs = new List<double>();

        private double _Fx;

        private double _Fy;

        private float _MarkerLength;

        private float _SquareLength;

        private int _SquaresX;

        private int _SquaresY;

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

        public override void SetDefaults()
        {
            base.SetDefaults();
        }
    }
}