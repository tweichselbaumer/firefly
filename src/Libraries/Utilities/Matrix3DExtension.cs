using System.Windows.Media.Media3D;

namespace FireFly.Utilities
{
    public static class Matrix3DFactory
    {
        public static Matrix3D CreateMatrix(double[][] m)
        {
            return new Matrix3D(m[0][0], m[0][1], m[0][2], m[3][0], m[1][0], m[1][1], m[1][2], m[3][1], m[2][0], m[2][1], m[2][2], m[3][2], m[0][3], m[1][3], m[2][3], m[3][3]);
        }

        public static Matrix3D CreateMatrixRotationOnly(double[][] m)
        {
            Matrix3D matrix = CreateMatrix(m);
            matrix.OffsetX = 0;
            matrix.OffsetY = 0;
            matrix.OffsetZ = 0;
            matrix.M14 = 0;
            matrix.M24 = 0;
            matrix.M34 = 0;
            return matrix;
        }

        public static Matrix3D CreateMatrixTranslationOnly(double[][] m)
        {
            Matrix3D matrix = CreateMatrix(m);
            matrix.M11 = 1;
            matrix.M12 = 0;
            matrix.M13 = 0;
            matrix.M21 = 0;
            matrix.M22 = 1;
            matrix.M23 = 0;
            matrix.M31 = 0;
            matrix.M32 = 0;
            matrix.M33 = 1;
            return matrix;
        }
    }
}