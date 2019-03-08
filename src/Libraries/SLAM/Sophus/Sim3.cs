using System.Windows.Media.Media3D;
using FireFly.Utilities;
using MathNet.Numerics.LinearAlgebra;

namespace FireFly.VI.SLAM.Sophus
{
    public class Sim3
    {
        private double _Scale = 1;
        private SE3 _SE3 = new SE3();

        public Sim3(double scale, double tx, double ty, double tz, double q1, double q2, double q3, double q4) : this(scale, new Vector3(tx, ty, tz), new Quaternion(q1, q2, q3, q4))
        {

        }

        public Sim3()
        {
        }

        public Sim3(double scale, Vector3 translation, SO3 so3) : this(scale, new SE3(translation, so3))
        {
        }

        public Sim3(double scale, Vector3 translation, Quaternion quaternion) : this(scale, new SE3(translation, new SO3(quaternion)))
        {
        }

        public Sim3(double scale, SE3 se3)
        {
            Scale = scale;
            _SE3 = se3;
        }

        public Sim3(double[][] matrix)
        {
            Scale = matrix[3][3];
            _SE3 = new SE3(matrix);
        }

        public Sim3(double[,] matrix)
        {
            Scale = matrix[3, 3];
            _SE3 = new SE3(matrix);
        }

        public double Scale
        {
            get
            {
                return _Scale;
            }

            set
            {
                _Scale = value;
            }
        }

        public SE3 SE3
        {
            get
            {
                return _SE3;
            }

            set
            {
                _SE3 = value;
            }
        }

        /// <summary>
        /// Matrix*x
        /// </summary>
        public Matrix<double> Matrix
        {
            get
            {
                Matrix<double> result = SE3.Matrix;
                result[3, 3] = _Scale;
                return result;
            }
        }

        /// <summary>
        /// x'*Matrix3D
        /// </summary>
        public Matrix3D Matrix3D
        {
            get
            {
                return Matrix3DFactory.CreateMatrix(Matrix.ToArray());
            }
        }

        public Sim3 Inverse()
        {
            Sim3 result = new Sim3();

            result.SE3 = SE3.Inverse();
            result.Scale = 1 / Scale;
            result.SE3.Translation = new Vector3(result.Scale * result.SE3.Translation.Vector);

            return result;
        }
    }
}