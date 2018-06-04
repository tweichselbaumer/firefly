using Emgu.CV;
using Emgu.CV.Aruco;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace FireFly.VI.Calibration
{
    public static class ChArUcoDetector
    {
        public static (VectorOfInt markerIds, VectorOfVectorOfPointF markerCorners) Detect(Mat image)
        {
            Dictionary dictionary = new Dictionary(Dictionary.PredefinedDictionaryName.Dict6X6_250);

            VectorOfInt markerIds = new VectorOfInt();
            VectorOfVectorOfPointF markerCorners = new VectorOfVectorOfPointF();

            DetectorParameters decParameters = DetectorParameters.GetDefault();

            ArucoInvoke.DetectMarkers(image, dictionary, markerCorners, markerIds, decParameters);

            return (markerIds, markerCorners);
        }

        public static Mat Draw(Mat image, VectorOfInt markerIds, VectorOfVectorOfPointF markerCorners)
        {
            Mat result = image.ToImage<Rgb, byte>().Mat;

            ArucoInvoke.DrawDetectedMarkers(result, markerCorners, markerIds, new MCvScalar(255, 0, 0));

            return result;
        }

        public static (Mat cameraMatrix, Mat distCoeffs) Calibrate(int squaresX, int squaresY, float squareLength, float markerLength, Size imageSize, VectorOfInt allIds, VectorOfVectorOfPointF allCorners, VectorOfInt markerCounterPerFrame)
        {

            Mat cameraMatrix = new Mat();
            Mat distCoeffs = new Mat();

            ArucoInvoke.CalibrateCameraAruco(allCorners, allIds, markerCounterPerFrame, CreateBoard(squaresX, squaresY, squareLength, markerLength), imageSize, cameraMatrix, distCoeffs, null, null, CalibType.Default, new MCvTermCriteria(30, double.Epsilon));

            return (cameraMatrix, distCoeffs);
        }

        public static CharucoBoard CreateBoard(int squaresX, int squaresY, float squareLength, float markerLength)
        {
            Dictionary dictionary = new Dictionary(Dictionary.PredefinedDictionaryName.Dict6X6_250);

            return new CharucoBoard(squaresX, squaresY, squareLength, markerLength, dictionary);
        }

        public static Mat DrawBoard(int squaresX, int squaresY, float squareLength, float markerLength, Size imageSize)
        {
            CharucoBoard board = CreateBoard(squaresX, squaresY, squareLength, markerLength);
            Image<Gray, byte> boardImage = new Image<Gray, byte>(imageSize);
            board.Draw(imageSize, boardImage, 10, 1);

            return boardImage.Mat;
        }


    }
}
