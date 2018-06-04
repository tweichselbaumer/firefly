using Emgu.CV;
using FireFly.Utilities;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FireFly.Models
{
    public class CvImageContainer : DependencyObject
    {
        public static readonly DependencyProperty CvImageProperty =
            DependencyProperty.Register("CvImage", typeof(Mat), typeof(CvImageContainer), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(BitmapSource), typeof(CvImageContainer), new PropertyMetadata(null));

        public Mat CvImage
        {
            get { return (Mat)GetValue(CvImageProperty); }
            set { SetValue(CvImageProperty, value); }
        }

        public BitmapSource ImageSource
        {
            get { return (BitmapSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CvImageContainer)
            {
                CvImageContainer cvic = d as CvImageContainer;
                cvic.ImageSource = WpfOpenCvConverter.ToBitmapSource(cvic.CvImage);
            }
        }
    }
}