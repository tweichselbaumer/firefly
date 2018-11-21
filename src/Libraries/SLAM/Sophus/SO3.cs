using MathNet.Numerics.LinearAlgebra;

namespace FireFly.VI.SLAM.Sophus
{
    public class SO3
    {
        private Quaternion _Quaternion = new Quaternion();

        public SO3()
        {
        }

        public SO3(Quaternion q)
        {
            _Quaternion = q;
        }

        public SO3 Inverse()
        {
            SO3 result = new SO3();
            result._Quaternion = _Quaternion.Inverse();
            return result;
        }

        public Vector<double> TransformVector(Vector<double> translation)
        {
            return _Quaternion.Matrix.Multiply(translation);
        }
    }
}