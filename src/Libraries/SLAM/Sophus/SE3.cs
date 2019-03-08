using MathNet.Numerics.LinearAlgebra;

namespace FireFly.VI.SLAM.Sophus
{
    public class SE3
    {
        private SO3 _SO3 = new SO3();
        private Vector3 _Translation = new Vector3();

        public SE3()
        {
        }

        public SE3(double[][] matrix)
        {
            _Translation.X = matrix[0][3];
            _Translation.Y = matrix[1][3];
            _Translation.Z = matrix[2][3];
            _SO3 = new SO3(matrix);
        }

        public SE3(double[,] matrix)
        {
            _Translation.X = matrix[0, 3];
            _Translation.Y = matrix[1, 3];
            _Translation.Z = matrix[2, 3];
            _SO3 = new SO3(matrix);
        }

        public SE3(Vector3 translation, SO3 so3)
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

        public Vector3 Translation
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
                result.SetSubMatrix(0, 3, Translation.Vector.ToColumnMatrix());
                return result;
            }
        }

        public SE3 Inverse()
        {
            SE3 result = new SE3();

            result.SO3 = SO3.Inverse();
            result.Translation = -1 * result.SO3.TransformVector(Translation);

            return result;
        }
    }
}