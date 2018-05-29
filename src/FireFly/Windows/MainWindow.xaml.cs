using LinkUp.Node;
using LinkUp.Raw;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Emgu.CV;
using System.Runtime.InteropServices;
using System.Threading;

namespace FireFly.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        private readonly SynchronizationContext _syncContext;
        LinkUpTcpClientConnector connector;


        public MainWindow()
        {
            InitializeComponent();

            _syncContext = SynchronizationContext.Current;

            //connector = new LinkUpTcpClientConnector(IPAddress.Parse("127.0.0.1"), 3000);

            //LinkUpNode node = new LinkUpNode();
            //node.Name = "leaf";
            //node.AddSubNode(connector);

            //LinkUpEventLabel eventLabel = node.GetLabelByName<LinkUpEventLabel>("leaf/test/label_event");
            //eventLabel.Subscribe();
            //eventLabel.Fired += Program_Fired;
        }

        private void Program_Fired(LinkUpEventLabel label, byte[] data)
        {
            Mat mat = new Mat();
            CvInvoke.Imdecode(data, Emgu.CV.CvEnum.ImreadModes.Grayscale, mat);
            _syncContext.Post(o =>
            {
                //srcImg.Source = ToBitmapSource(mat);
            }
            , null);
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }
    }
}
