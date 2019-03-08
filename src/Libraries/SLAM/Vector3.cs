using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireFly.VI.SLAM
{
    public class Vector3
    {
        private Vector<double> _Vector = Vector<double>.Build.Dense(3);

        public Vector3(double x, double y, double z)
        {
            _Vector[0] = x;
            _Vector[1] = y;
            _Vector[2] = z;
        }

        public Vector3()
        {
            _Vector[0] = 0;
            _Vector[1] = 0;
            _Vector[2] = 0;
        }

        public Vector3(Vector<double> v)
        {
            if (v.Count == 0)
            {
                _Vector[0] = 0;
                _Vector[1] = 0;
                _Vector[2] = 0;
            }
            else if (v.Count == 1)
            {
                _Vector[0] = v[0];
                _Vector[1] = 0;
                _Vector[2] = 0;
            }
            else if (v.Count == 2)
            {
                _Vector[0] = v[0];
                _Vector[1] = v[1];
                _Vector[2] = 0;
            }
            else
            {
                _Vector[0] = v[0];
                _Vector[1] = v[1];
                _Vector[2] = v[2];
            }
        }

        public double X
        {
            get
            {
                return _Vector[0];
            }
            set
            {
                _Vector[0] = value;
            }
        }

        public double Y
        {
            get
            {
                return _Vector[1];
            }
            set
            {
                _Vector[1] = value;
            }
        }

        public double Z
        {
            get
            {
                return _Vector[2];
            }
            set
            {
                _Vector[2] = value;
            }
        }

        public Vector<double> Vector
        {
            get
            {
                return _Vector;
            }

            set
            {
                _Vector = value;
            }
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            Vector3 result = new Vector3();
            result.Vector = a.Vector + b.Vector;
            return result;
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            Vector3 result = new Vector3();
            result.Vector = a.Vector + b.Vector;
            return result;
        }

        public static Vector3 operator *(Vector3 a, double val)
        {
            Vector3 result = new Vector3();
            result.Vector = a.Vector * val;
            return result;
        }

        public static Vector3 operator *(double val, Vector3 a)
        {
            Vector3 result = new Vector3();
            result.Vector = a.Vector * val;
            return result;
        }
    }
}
