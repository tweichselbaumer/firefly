using MathNet.Numerics.LinearAlgebra;
using System;

namespace FireFly.VI.SLAM.Sophus
{
    public class Quaternion
    {
        private double _Q1 = 1.0;
        private double _Q2 = 0.0;
        private double _Q3 = 0.0;
        private double _Q4 = 0.0;

        public Quaternion(double q1, double q2, double q3, double q4)
        {
            _Q1 = q1;
            _Q2 = q2;
            _Q3 = q3;
            _Q4 = q4;
            Normalize();
        }

        public Quaternion()
        {
        }

        public Matrix<double> Matrix
        {
            get
            {
                Matrix<double> tilde = Tilde(Q2, Q3, Q4);
                return Matrix<double>.Build.DenseIdentity(3, 3) + 2 * (tilde * Q1 + tilde * tilde);
            }
        }

        public double Q1
        {
            get
            {
                return _Q1;
            }
        }

        public double Q2
        {
            get
            {
                return _Q2;
            }
        }

        public double Q3
        {
            get
            {
                return _Q3;
            }
        }

        public double Q4
        {
            get
            {
                return _Q4;
            }
        }

        public Quaternion Inverse()
        {
            Quaternion result = new Quaternion();
            result._Q1 = _Q1;
            result._Q2 = -_Q2;
            result._Q3 = -_Q3;
            result._Q4 = -_Q4;
            return result;
        }

        private static Matrix<double> Tilde(double x, double y, double z)
        {
            Matrix<double> result = Matrix<double>.Build.Dense(3, 3);

            result[0, 0] = 0;
            result[0, 1] = -z;
            result[0, 2] = y;

            result[1, 0] = z;
            result[1, 1] = 0;
            result[1, 2] = -x;

            result[2, 0] = -y;
            result[2, 1] = x;
            result[2, 2] = 0;

            return result;
        }

        private double Norm()
        {
            return Math.Sqrt(Q1 * Q1 + Q2 * Q2 + Q3 * Q3 + Q4 * Q4);
        }

        private void Normalize()
        {
            double norm = Norm();
            _Q1 /= norm;
            _Q2 /= norm;
            _Q3 /= norm;
            _Q4 /= norm;
        }
    }
}