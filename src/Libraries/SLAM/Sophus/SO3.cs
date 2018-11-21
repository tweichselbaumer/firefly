using MathNet.Numerics.LinearAlgebra;

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