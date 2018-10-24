using System.Collections.Generic;

namespace FireFly.Settings
{
    public class IntrinsicCalibrationSettings : AbstractSettings
    {
        private double _Alpha;
        private double _Cx;

        private double _Cy;

        private List<double> _DistCoeffs = new List<double>();

        private double _FOVScale;
        private double _Fx;

        private double _Fy;

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

        public override void SetDefaults()
        {
            base.SetDefaults();
        }
    }
}