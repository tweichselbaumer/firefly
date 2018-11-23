using MathNet.Numerics.LinearAlgebra;
using System;

namespace FireFly.VI.SLAM.Sophus
{
    public class SO3
    {
        private Quaternion _Quaternion = new Quaternion();

        public Quaternion Quaternion
        {
            get
            {
                return _Quaternion;
            }

            set
            {
                _Quaternion = value;
            }
        }

        public SO3()
        {
        }

        public SO3(Quaternion q)
        {
            Quaternion = q;
        }

        public SO3(double[][] matrix)
        {
            double q1 = 0.5 * Math.Sqrt(matrix[0][0] + matrix[1][1] + matrix[2][2] + 1);
            double q2 = 0.5 * Math.Sign(matrix[2][1] - matrix[1][2]) * Math.Sqrt(matrix[0][0] - matrix[1][1] - matrix[2][2] + 1);
            double q3 = 0.5 * Math.Sign(matrix[0][2] - matrix[2][0]) * Math.Sqrt(-matrix[0][0] + matrix[1][1] - matrix[2][2] + 1);
            double q4 = 0.5 * Math.Sign(matrix[1][0] - matrix[0][1]) * Math.Sqrt(-matrix[0][0] - matrix[1][1] + matrix[2][2] + 1);
            _Quaternion = new Quaternion(q1, q2, q3, q4);
        }

        public Matrix<double> Matrix
        {
            get
            {
                return Quaternion.Matrix;
            }
        }

        public SO3 Inverse()
        {
            SO3 result = new SO3();
            result.Quaternion = Quaternion.Inverse();
            return result;
        }

        public Vector<double> TransformVector(Vector<double> translation)
        {
            return Matrix.Multiply(translation);
        }
    }
}