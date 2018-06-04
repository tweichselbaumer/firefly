using Emgu.CV;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FireFly.Utilities
{
    public static class WpfOpenCvConverter
    {
        public static BitmapSource ToBitmapSource(Mat image)
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

        public static Mat ToMath(BitmapSource bitmapSource)
        {
            byte[] bits = new byte[bitmapSource.PixelWidth * bitmapSource.PixelHeight * bitmapSource.Format.BitsPerPixel / 8];
            int stride = bitmapSource.Format.BitsPerPixel * bitmapSource.PixelWidth / 8;
            bitmapSource.CopyPixels(bits, stride, 0);
            MemoryStream memoryStream = new MemoryStream(bits);
            System.Drawing.Bitmap bitmapData = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(memoryStream);
            return new Image<Emgu.CV.Structure.Gray, byte>(bitmapData).Mat;
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
    }
}