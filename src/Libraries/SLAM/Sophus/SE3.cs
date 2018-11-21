using MathNet.Numerics.LinearAlgebra;

namespace FireFly.VI.SLAM.Sophus
{
    public class SE3
    {
        private SO3 _SO3 = new SO3();
        private Vector<double> _Translation = Vector<double>.Build.Dense(3);

        public SE3()
        {
        }

        public SE3(Vector<double> translation, SO3 so3)
        {
            Translation = translation;
            SO3 = so3;
        }

        public SO3 SO3
        {
            get
            {
                return _SO3;
            }

            set
            {
                _SO3 = value;
            }
        }

        public Vector<double> Translation
        {
            get
            {
                return _Translation;
            }

            set
            {
                _Translation = value;
            }
        }

        public Matrix<double> Matrix
        {

            get
            {
                Matrix<double> result = Matrix<double>.Build.Dense(4, 4);
                result.SetSubMatrix(0, 0, SO3.Matrix);
                result.SetSubMatrix(0, 3, Translation.ToColumnMatrix());
                return result;
            }
        }

        public SE3 Inverse()
        {
            SE3 result = new SE3();

            result.SO3 = SO3.Inverse();
            result.Translation = -SO3.TransformVector(Translation);

            return result;
        }
    }
}