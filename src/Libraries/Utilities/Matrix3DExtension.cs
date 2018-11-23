using System.Windows.Media.Media3D;

namespace FireFly.Utilities
{
    public static class Matrix3DFactory
    {
        public static Matrix3D CreateMatrix(double[][] m)
        {
            return new Matrix3D(m[0][0], m[1][0], m[2][0], m[3][0], m[0][1], m[1][1], m[1][2], m[1][3], m[0][2], m[1][2], m[2][2], m[3][2], m[0][3], m[1][3], m[2][3], m[3][3]);
        }

        public static Matrix3D CreateMatrix(double[,] m)
        {
            return new Matrix3D(m[0, 0], m[1, 0], m[2, 0], m[3, 0], m[0, 1], m[1, 1], m[2, 1], m[1, 3], m[0, 2], m[1, 2], m[2, 2], m[3, 2], m[0, 3], m[1, 3], m[2, 3], m[3, 3]);
        }
    }
}