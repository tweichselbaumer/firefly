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

        private VectorOfVectorOfPointF _MarkerCorners;
        private VectorOfInt _MarkerIds;

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

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChArUcoImageContainer)
            {
                ChArUcoImageContainer cauic = d as ChArUcoImageContainer;
                (VectorOfInt markerIds, VectorOfVectorOfPointF markerCorners) result = ChArUcoDetector.Detect(cauic.OriginalImage.CvImage);
                cauic.MarkerIds = result.markerIds;
                cauic.MarkerCorners = result.markerCorners;
                cauic.ProgressedImage = new CvImageContainer();
                cauic.ProgressedImage.CvImage = ChArUcoDetector.Draw(cauic.OriginalImage.CvImage, result.markerIds, result.markerCorners);
            }
        }
    }
}