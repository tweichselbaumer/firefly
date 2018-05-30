using Emgu.CV;
using LinkUp.Node;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FireFly.ViewModels
{
    public class CameraViewModel : DependencyObject
    {
        public static readonly DependencyProperty ImageSouceProperty =
            DependencyProperty.Register("ImageSource", typeof(BitmapSource), typeof(CameraViewModel), new PropertyMetadata(null));

        private readonly SynchronizationContext _SyncContext;
        private MainViewModel _Parent;

        public CameraViewModel(MainViewModel parent)
        {
            _Parent = parent;
            _SyncContext = SynchronizationContext.Current;
        }

        public BitmapSource ImageSource
        {
            get { return (BitmapSource)GetValue(ImageSouceProperty); }
            set { SetValue(ImageSouceProperty, value); }
        }

        public MainViewModel Parent
        {
            get
            {
                return _Parent;
            }
        }

        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap();

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr);
                return bs;
            }
        }

        internal void UpdateLinkUpBindings()
        {
            if (Parent.Node != null)
            {
                LinkUpEventLabel eventLabel = Parent.Node.GetLabelByName<LinkUpEventLabel>("firefly/test/label_event");
                eventLabel.Subscribe();
                eventLabel.Fired += EventLabel_Fired;
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        private void EventLabel_Fired(LinkUpEventLabel label, byte[] data)
        {
            Mat mat = new Mat();
            CvInvoke.Imdecode(data, Emgu.CV.CvEnum.ImreadModes.Grayscale, mat);
            _SyncContext.Post(o =>
            {
                ImageSource = ToBitmapSource(mat);
            }
            , null);
        }
    }
}