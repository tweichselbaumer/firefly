using System.Collections.Generic;
using System.Linq;

namespace FireFly.VI.SLAM
{
    public class KeyFrame
    {
        private double _Cx;
        private double _Cy;
        private Frame _Frame;
        private double _Fx;
        private double _Fy;
        private uint _Id;
        private List<Point> _Points = new List<Point>();

        public KeyFrame(uint id, double fx, double fy, double cx, double cy, int points, Frame frame)
        {
            Id = id;
            Fx = fx;
            Fy = fy;
            Cx = cx;
            Cy = cy;
            Points.AddRange(Enumerable.Range(1, points).Select(c => (Point)null));
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

        public Frame Frame
        {
            get
            {
                return _Frame;
            }

            set
            {
                _Frame = value;
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

        public uint Id
        {
            get
            {
                return _Id;
            }

            set
            {
                _Id = value;
            }
        }

        public List<Point> Points
        {
            get
            {
                return _Points;
            }

            set
            {
                _Points = value;
            }
        }
    }
}