using Emgu.CV;
using Emgu.CV.CvEnum;
using FireFly.Data.Storage;
using FireFly.VI.SLAM.Visualisation;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace FireFly.VI.SLAM.Data
{
    public class VIVideoRenderer
    {
        private string _FileName;
        private SlamModel3D _SlamModel3D;
        private SynchronizationContext _SyncContext;
        private int _VideoHeight;
        private int _VideoWidth;
        private VideoWriter _VideoWriter;
        private HelixViewport3D _Viewport3d;

        public VIVideoRenderer(string fileName, int width, int height, SynchronizationContext syncContext)
        {
            _FileName = fileName;
            _VideoWidth = width;
            _VideoHeight = height;
            _SlamModel3D = new SlamModel3D(syncContext, false);
            _SyncContext = syncContext;
        }

        public void Close()
        {
            if (_VideoWriter != null)
            {
                _VideoWriter.Dispose();
                _VideoWriter = null;
            }
        }

        public void Open()
        {
            _VideoWriter = new VideoWriter(_FileName, VideoWriter.Fourcc('M', 'P', '4', 'V'), 20, new System.Drawing.Size(_VideoWidth, _VideoHeight), true);
            CreateViewport();
        }

        public void Render(VIMatlabImporter matlabImporter, RawDataReader reader, Action<double> progress = null)
        {
            int count = reader.Count;

            int i = 0;

            List<KeyFrame> keyFrames = matlabImporter.GetKeyFrames();
            List<Frame> frames = matlabImporter.GetFrames();

            while (reader.HasNext())
            {
                i++;
                if (i % 100 == 0)
                {
                    progress?.Invoke((double)i / count);
                }
                Tuple<long, List<Tuple<RawReaderMode, object>>> res = reader.Next();

                foreach (Tuple<RawReaderMode, object> val in res.Item2)
                {
                    if (val.Item1 == RawReaderMode.Camera0)
                    {
                        Mat rawImage = new Mat();
                        CvInvoke.Imdecode(((Tuple<double, byte[]>)val.Item2).Item2, ImreadModes.Grayscale, rawImage);
                        WriteFrame(rawImage, RenderViewport());
                    }
                }
            }
        }

        private void CreateViewport()
        {
            _SyncContext.Send(d =>
            {
                _Viewport3d = new HelixViewport3D();
                _Viewport3d.Background = System.Windows.Media.Brushes.White;

                _Viewport3d.ClipToBounds = false;
                _Viewport3d.IsHitTestVisible = true;
                _Viewport3d.CameraMode = CameraMode.Inspect;
                _Viewport3d.CameraRotationMode = CameraRotationMode.Trackball;

                _Viewport3d.DefaultCamera = new PerspectiveCamera(new Point3D(0, 1, 0.3), new Vector3D(0, 1, -0.3), new Vector3D(0, 0, 1), 45);

                _Viewport3d.Children.Add(new DefaultLights());

                _Viewport3d.Children.Add(new CoordinateSystemVisual3D() { ArrowLengths = 0.1 });
            }, null);
        }

        private Mat RenderViewport()
        {
            Mat image = new Mat();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                _SyncContext.Send(d =>
                {
                    _Viewport3d.Viewport.Width = _VideoWidth - 512;
                    _Viewport3d.Viewport.Height = 2 * 512;
                    _Viewport3d.Viewport.ApplyTemplate();
                    _Viewport3d.Viewport.Measure(new System.Windows.Size(_VideoWidth - 512, 2 * 512));
                    _Viewport3d.Viewport.Arrange(new Rect(0, 0, _VideoWidth - 512, 2 * 512));
                    _Viewport3d.Viewport.UpdateLayout();
                    _Viewport3d.DefaultCamera.FitView(_Viewport3d.Viewport);

                    var renderTargetBitmap = _Viewport3d.Viewport.RenderBitmap(System.Windows.Media.Brushes.White, 2);

                    BitmapEncoder bitmapEncoder = new PngBitmapEncoder();
                    bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                    bitmapEncoder.Save(memoryStream);
                }, null);
                byte[] imageRaw = memoryStream.ToArray();

                CvInvoke.Imdecode(imageRaw, ImreadModes.Color, image);
            }
            return image;
        }

        private void WriteFrame(Mat rawImage, Mat viewport3dImage)
        {
            Image<Emgu.CV.Structure.Bgr, byte> imageResult = new Image<Emgu.CV.Structure.Bgr, byte>(_VideoWidth, _VideoHeight);
            imageResult.ROI = new Rectangle(0, 0, 512, 512);

            Mat rawImageColor = new Mat();

            CvInvoke.CvtColor(rawImage, rawImageColor, ColorConversion.Gray2Bgr);

            rawImageColor.CopyTo(imageResult);

            imageResult.ROI = new Rectangle(512, 0, _VideoWidth - 512, 2 * 512);

            viewport3dImage.CopyTo(imageResult);

            imageResult.ROI = Rectangle.Empty;

            _VideoWriter.Write(imageResult.Mat);
        }
    }
}