namespace FireFly.VI.SLAM
{
    public class Point
    {
        private byte[] _Colors = new byte[8];
        private double _InverseDepth;
        private double _U;
        private double _V;

        public byte[] Colors
        {
            get
            {
                return _Colors;
            }

            set
            {
                _Colors = value;
            }
        }

        public double InverseDepth
        {
            get
            {
                return _InverseDepth;
            }

            set
            {
                _InverseDepth = value;
            }
        }

        public double U
        {
            get
            {
                return _U;
            }

            set
            {
                _U = value;
            }
        }

        public double V
        {
            get
            {
                return _V;
            }

            set
            {
                _V = value;
            }
        }
    }
}