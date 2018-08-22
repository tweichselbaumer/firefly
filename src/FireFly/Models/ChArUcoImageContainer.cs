using Emgu.CV.Util;
using FireFly.VI.Calibration;
using System.Windows;

namespace FireFly.Models
{
    public class ChArUcoImageContainer : DependencyObject
    {
        public static readonly DependencyProperty OriginalImageProperty =
            DependencyProperty.Register("OriginalImage", typeof(CvImageContainer), typeof(ChArUcoImageContainer), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty ProgressedImageProperty =
            DependencyProperty.Register("ProgressedImage", typeof(CvImageContainer), typeof(ChArUcoImageContainer), new PropertyMetadata(null));

        private VectorOfPointF _CharucoCorners;
        private VectorOfInt _CharucoIds;
        private VectorOfVectorOfPointF _MarkerCorners;
        private VectorOfInt _MarkerIds;
        private float _MarkerLength;
        private float _SquareLength;
        private int _SquaresX;
        private int _SquaresY;

        public ChArUcoImageContainer(int squaresX, int squaresY, float squareLength, float markerLength)
        {
            _SquaresX = squaresX;
            _SquaresY = squaresY;
            _SquareLength = squareLength;
            _MarkerLength = markerLength;
        }

        public VectorOfPointF CharucoCorners
        {
            get
            {
                return _CharucoCorners;
            }

            set
            {
                _CharucoCorners = value;
            }
        }

        public VectorOfInt CharucoIds
        {
            get
            {
                return _CharucoIds;
            }

            set
            {
                _CharucoIds = value;
            }
        }

        public VectorOfVectorOfPointF MarkerCorners
        {
            get
            {
                return _MarkerCorners;
            }

            set
            {
                _MarkerCorners = value;
            }
        }

        public VectorOfInt MarkerIds
        {
            get
            {
                return _MarkerIds;
            }

            set
            {
                _MarkerIds = value;
            }
        }

        public float MarkerLength
        {
            get
            {
                return _MarkerLength;
            }
        }

        public CvImageContainer OriginalImage
        {
            get { return (CvImageContainer)GetValue(OriginalImageProperty); }
            set { SetValue(OriginalImageProperty, value); }
        }

        public CvImageContainer ProgressedImage
        {
            get { return (CvImageContainer)GetValue(ProgressedImageProperty); }
            set { SetValue(ProgressedImageProperty, value); }
        }

        public float SquareLength
        {
            get
            {
                return _SquareLength;
            }
        }

        public int SquaresX
        {
            get
            {
                return _SquaresX;
            }
        }

        public int SquaresY
        {
            get
            {
                return _SquaresY;
            }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChArUcoImageContainer)
            {
                ChArUcoImageContainer cauic = d as ChArUcoImageContainer;
                (VectorOfInt markerIds, VectorOfVectorOfPointF markerCorners, VectorOfInt charucoIds, VectorOfPointF charucoCorners) result = ChArUcoCalibration.Detect(cauic.OriginalImage.CvImage, cauic.SquaresX, cauic.SquaresY, cauic.SquareLength, cauic.MarkerLength);
                cauic.MarkerIds = result.markerIds;
                cauic.MarkerCorners = result.markerCorners;
                cauic.CharucoIds = result.charucoIds;
                cauic.CharucoCorners = result.charucoCorners;
                cauic.ProgressedImage = new CvImageContainer();
                cauic.ProgressedImage.CvImage = ChArUcoCalibration.Draw(cauic.OriginalImage.CvImage, result.markerIds, result.markerCorners, result.charucoIds, result.charucoCorners);
            }
        }
    }
}