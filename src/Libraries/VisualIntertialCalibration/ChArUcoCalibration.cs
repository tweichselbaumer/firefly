using Emgu.CV;
using Emgu.CV.Aruco;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;

namespace FireFly.VI.Calibration
{
    public static class ChArUcoCalibration
    {
        public static (Mat cameraMatrix, Mat distCoeffs, double rms) CalibrateAruco(int squaresX, int squaresY, float squareLength, float markerLength, Size imageSize, VectorOfInt allIds, VectorOfVectorOfPointF allCorners, VectorOfInt markerCounterPerFrame, bool fisheye)
        {
            Mat cameraMatrix = new Mat();
            Mat distCoeffs = new Mat();
            double rms = 0.0;

            CharucoBoard board = CreateBoard(squaresX, squaresY, squareLength, markerLength);

            if (!fisheye)
            {
                rms = ArucoInvoke.CalibrateCameraAruco(allCorners, allIds, markerCounterPerFrame, board, imageSize, cameraMatrix, distCoeffs, null, null, CalibType.Default, new MCvTermCriteria(30, double.Epsilon));
            }
            else
            {
                VectorOfMat processedObjectPoints = new VectorOfMat();
                VectorOfMat processedImagePoints = new VectorOfMat();

                int nFrames = markerCounterPerFrame.Size;

                int markerCounter = 0;
                for (int frame = 0; frame < nFrames; frame++)
                {
                    int nMarkersInThisFrame = markerCounterPerFrame[frame];
                    VectorOfVectorOfPointF thisFrameCorners = new VectorOfVectorOfPointF();
                    VectorOfInt thisFrameIds = new VectorOfInt();

                    for (int j = markerCounter; j < markerCounter + nMarkersInThisFrame; j++)
                    {
                        thisFrameCorners.Push(allCorners[j]);
                        thisFrameIds.Push(new int[] { allIds[j] });
                    }
                    markerCounter += nMarkersInThisFrame;

                    Mat currentObjPoints = new Mat();
                    Mat currentImgPoints = new Mat();

                    getBoardObjectAndImagePoints(board, thisFrameCorners, thisFrameIds, currentObjPoints, currentImgPoints, new Size((int)(squaresX * 10000 * squareLength), (int)(squaresY * 10000 * squareLength)));

                    if ((int)currentImgPoints.Total > 0 && (int)currentObjPoints.Total > 0)
                    {
                        processedImagePoints.Push(currentImgPoints);
                        processedObjectPoints.Push(currentObjPoints);
                    }
                }

                Fisheye.Calibrate(processedObjectPoints, processedImagePoints, imageSize, cameraMatrix, distCoeffs, new Mat(), new Mat(), Fisheye.CalibrationFlag.Default, new MCvTermCriteria(30, double.Epsilon));
            }

            return (cameraMatrix, distCoeffs, rms);
        }

        public static (Mat cameraMatrix, Mat distCoeffs, double rms) CalibrateCharuco(int squaresX, int squaresY, float squareLength, float markerLength, Size imageSize, VectorOfInt charucoIds, VectorOfPointF charucoCorners, VectorOfInt markerCounterPerFrame, bool fisheye, Func<byte[], byte[]> GetRemoteChessboardCorner)
        {
            Mat cameraMatrix = new Mat(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1);
            Mat distCoeffs = new Mat(1, 4, Emgu.CV.CvEnum.DepthType.Cv64F, 1);
            double rms = 0.0;

            VectorOfVectorOfPoint3D32F processedObjectPoints = new VectorOfVectorOfPoint3D32F();
            VectorOfVectorOfPointF processedImagePoints = new VectorOfVectorOfPointF();

            int k = 0;

            for (int i = 0; i < markerCounterPerFrame.Size; i++)
            {
                int nMarkersInThisFrame = markerCounterPerFrame[i];
                VectorOfPointF currentImgPoints = new VectorOfPointF();
                VectorOfPoint3D32F currentObjPoints = new VectorOfPoint3D32F();

                for (int j = 0; j < nMarkersInThisFrame; j++)
                {
                    currentImgPoints.Push(new PointF[] { charucoCorners[k] });
                    currentObjPoints.Push(new MCvPoint3D32f[] { GetChessboardCorner(squaresX, squaresY, squareLength, markerLength, charucoIds[k], GetRemoteChessboardCorner) });
                    k++;
                }

                processedImagePoints.Push(currentImgPoints);
                processedObjectPoints.Push(currentObjPoints);
            }

            VectorOfPoint3D32F rvecs = new VectorOfPoint3D32F();
            VectorOfPoint3D32F tvecs = new VectorOfPoint3D32F();

            if (fisheye)
            {
                Fisheye.Calibrate(processedObjectPoints, processedImagePoints, imageSize, cameraMatrix, distCoeffs, rvecs, tvecs, Fisheye.CalibrationFlag.FixSkew | Fisheye.CalibrationFlag.RecomputeExtrinsic, new MCvTermCriteria(400, double.Epsilon));
            }
            else
            {
                CvInvoke.CalibrateCamera(processedObjectPoints, processedImagePoints, imageSize, cameraMatrix, distCoeffs, new Mat(), new Mat(), CalibType.FixK3, new MCvTermCriteria(30, 1e-6));
            }

            rms = Validate(processedObjectPoints, processedImagePoints, cameraMatrix, distCoeffs, rvecs, tvecs, fisheye);

            return (cameraMatrix, distCoeffs, rms);
        }

        private static double Validate(VectorOfVectorOfPoint3D32F processedObjectPoints, VectorOfVectorOfPointF processedImagePoints, Mat cameraMatrix, Mat distCoeffs, VectorOfPoint3D32F rvecs, VectorOfPoint3D32F tvecs, bool fisheye)
        {
            double error = 0;
            int totalpoints = 0;
            if (fisheye)
            {
                for (int i = 0; i < processedObjectPoints.Size; i++)
                {
                    VectorOfPoint3D32F objectFramePoints = processedObjectPoints[i];
                    VectorOfPointF imageFramePoints = processedImagePoints[i];
                    RotationVector3D tvec = new RotationVector3D(new double[] { tvecs[i].X, tvecs[i].Y, tvecs[i].Z });
                    RotationVector3D rvec = new RotationVector3D(new double[] { rvecs[i].X, rvecs[i].Y, rvecs[i].Z });

                    VectorOfPointF newImageFramePoints = new VectorOfPointF();

                    Fisheye.ProjectPoints(objectFramePoints, newImageFramePoints, rvec, tvec, cameraMatrix, distCoeffs);

                    for (int j = 0; j < newImageFramePoints.Size; j++)
                    {
                        PointF x1 = newImageFramePoints[j];
                        PointF x2 = imageFramePoints[j];
                        totalpoints++;
                        error += Math.Pow(x1.X - x2.X, 2) + Math.Pow(x1.Y - x2.Y, 2);
                    }
                }

            }
            return Math.Sqrt(error / totalpoints);
        }

        private static MCvPoint3D32f GetChessboardCorner(int squaresX, int squaresY, float squareLength, float markerLength, int markerId, Func<byte[], byte[]> getRemoteChessboardCorner)
        {
            MCvPoint3D32f result = new MCvPoint3D32f();

            byte[] inputData = new byte[5 * 4];

            Array.Copy(BitConverter.GetBytes(squaresX), 0, inputData, 0, sizeof(int));
            Array.Copy(BitConverter.GetBytes(squaresY), 0, inputData, 4, sizeof(int));
            Array.Copy(BitConverter.GetBytes(squareLength), 0, inputData, 8, sizeof(float));
            Array.Copy(BitConverter.GetBytes(markerLength), 0, inputData, 12, sizeof(float));
            Array.Copy(BitConverter.GetBytes(markerId), 0, inputData, 16, sizeof(int));

            byte[] outputData = getRemoteChessboardCorner(inputData);

            result.X = BitConverter.ToSingle(outputData, 0);
            result.Y = BitConverter.ToSingle(outputData, 4);
            result.Z = BitConverter.ToSingle(outputData, 8);

            return result;
        }

        public static CharucoBoard CreateBoard(int squaresX, int squaresY, float squareLength, float markerLength)
        {
            Dictionary dictionary = new Dictionary(Dictionary.PredefinedDictionaryName.Dict6X6_250);

            return new CharucoBoard(squaresX, squaresY, squareLength, markerLength, dictionary);
        }

        public static (VectorOfInt markerIds, VectorOfVectorOfPointF markerCorners, VectorOfInt charucoIds, VectorOfPointF charucoCorners) Detect(Mat image, int squaresX, int squaresY, float squareLength, float markerLength)
        {
            Dictionary dictionary = new Dictionary(Dictionary.PredefinedDictionaryName.Dict6X6_250);

            VectorOfInt markerIds = new VectorOfInt();
            VectorOfVectorOfPointF markerCorners = new VectorOfVectorOfPointF();
            VectorOfInt charucoIds = new VectorOfInt();
            VectorOfPointF charucoCorners = new VectorOfPointF();
            VectorOfVectorOfPointF rejectedMarkerCorners = new VectorOfVectorOfPointF();

            DetectorParameters decParameters = DetectorParameters.GetDefault();

            ArucoInvoke.DetectMarkers(image, dictionary, markerCorners, markerIds, decParameters, rejectedMarkerCorners);

            ArucoInvoke.RefineDetectedMarkers(image, CreateBoard(squaresX, squaresY, squareLength, markerLength), markerCorners, markerIds, rejectedMarkerCorners, null, null, 10, 3, true, null, decParameters);

            if (markerIds.Size > 0)
                ArucoInvoke.InterpolateCornersCharuco(markerCorners, markerIds, image, CreateBoard(squaresX, squaresY, squareLength, markerLength), charucoCorners, charucoIds, null, null, 2);

            return (markerIds, markerCorners, charucoIds, charucoCorners);
        }

        public static Mat Draw(Mat image, VectorOfInt markerIds, VectorOfVectorOfPointF markerCorners, VectorOfInt charucoIds, VectorOfPointF charucoCorners)
        {
            Mat result = image.ToImage<Rgb, byte>().Mat;

            ArucoInvoke.DrawDetectedMarkers(result, markerCorners, markerIds, new MCvScalar(255, 0, 0));
            ArucoInvoke.DrawDetectedCornersCharuco(result, charucoCorners, charucoIds, new MCvScalar(255, 255, 0));

            return result;
        }

        public static Mat DrawBoard(int squaresX, int squaresY, float squareLength, float markerLength, Size imageSize)
        {
            return DrawBoard(CreateBoard(squaresX, squaresY, squareLength, markerLength), imageSize);
        }

        public static Mat DrawBoard(CharucoBoard board, Size imageSize)
        {
            Image<Gray, byte> boardImage = new Image<Gray, byte>(imageSize);
            board.Draw(imageSize, boardImage, 10, 1);

            return boardImage.Mat;
        }

        private static void getBoardObjectAndImagePoints(CharucoBoard board, VectorOfVectorOfPointF detectedCorners, VectorOfInt detectedIds, Mat objPoints, Mat imgPoints, Size size)
        {
            //int nDetectedMarkers = detectedIds.Size;

            //VectorOfPoint3D32F objPnts = new VectorOfPoint3D32F();

            //VectorOfPointF imgPnts = new VectorOfPointF();

            //for (int i = 0; i < nDetectedMarkers; i++)
            //{
            //    int currentId = detectedIds[i];
            //    for (int j = 0; j < markerIds.Size; j++)
            //    {
            //        if (currentId == markerIds[j])
            //        {
            //            for (int p = 0; p < 4; p++)
            //            {
            //                //objPnts.Push(new MCvPoint3D32f[] { new MCvPoint3D32f(markerCorners[j][p].X / 100000, markerCorners[j][p].Y / 100000, 0) });
            //                imgPnts.Push(new PointF[] { detectedCorners[i][p] });
            //            }
            //        }
            //    }
            //}

            //objPnts.GetInputOutputArray().CopyTo(objPoints);
            //imgPnts.GetInputOutputArray().CopyTo(imgPoints);
        }
    }
}