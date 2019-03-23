﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using FireFly.Data.Storage;
using FireFly.VI.SLAM.Sophus;
using FireFly.VI.SLAM.Visualisation;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace FireFly.VI.SLAM.Data
{
    public class VIVideoRenderer
    {
        private const double CAMERA_RADIUS = 5;
        private const double CAMERA_ROTATION_SPEED = 0.01;
        private const double CAMERA_Z = 2.5;
        private CoordinateSystemVisual3D _CameraPosition;
        private string _FileName;
        private ModelVisual3D _ModelVisual3d;
        private SlamModel3D _SlamModel3D;
        private SynchronizationContext _SyncContext;
        private LinesVisual3D _TrajectoryFrame;
        private LinesVisual3D _TrajectoryKeyFrame;
        private int _VideoHeight;
        private int _VideoWidth;
        private VideoWriter _VideoWriter;
        private HelixViewport3D _Viewport3d;

        public VIVideoRenderer(string fileName, int width, int height, SynchronizationContext syncContext)
        {
            _FileName = fileName;
            _VideoWidth = width;
            _VideoHeight = height;
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

        public void MoveCamera(double x0, double y0, double t)
        {
            double x = x0 + CAMERA_RADIUS * Math.Cos(2 * Math.PI * t * CAMERA_ROTATION_SPEED);
            double y = y0 + CAMERA_RADIUS * Math.Sin(2 * Math.PI * t * CAMERA_ROTATION_SPEED);
            _SyncContext.Send(d =>
            {
                _Viewport3d.Camera.Position = new Point3D(x, y, CAMERA_Z);
                _Viewport3d.Camera.LookDirection = new Vector3D(x0 - x, y0 - y, -CAMERA_Z);
            }, null);
        }

        public void Open()
        {
            _VideoWriter = new VideoWriter(_FileName, VideoWriter.Fourcc('M', 'P', '4', 'V'), 20, new System.Drawing.Size(_VideoWidth, _VideoHeight), true);
            CreateViewport();
        }

        public void Render(VIMatlabImporter matlabImporter, RawDataReader reader, Action<double> progress = null)
        {
            int count = reader.Count;
            double startTime = 0;

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
                double time = (double)res.Item1 / (1000 * 1000 * 1000);
                if (i == 1)
                    startTime = time;

                foreach (Tuple<RawReaderMode, object> val in res.Item2)
                {
                    if (val.Item1 == RawReaderMode.Camera0)
                    {
                        Mat rawImage = new Mat();
                        CvInvoke.Imdecode(((Tuple<double, byte[]>)val.Item2).Item2, ImreadModes.Grayscale, rawImage);

                        List<KeyFrame> kfs = keyFrames.Where(c => c.Frame.Time <= time).ToList();

                        foreach (KeyFrame keyFrame in kfs)
                        {
                            _SlamModel3D.AddNewKeyFrame(keyFrame);
                            keyFrames.Remove(keyFrame);
                        }

                        List<Frame> fs = frames.Where(c => c.Time <= time).ToList();

                        foreach (Frame frame in fs)
                        {
                            _SlamModel3D.AddNewFrame(frame);
                            frames.Remove(frame);
                        }

                        _SlamModel3D.Render();

                        SE3 Twc_last = _SlamModel3D.LastTransformation();

                        MoveCamera(Twc_last.Translation.X, Twc_last.Translation.Y, time - startTime);
                        WriteFrame(rawImage, RenderViewport());
                    }
                }
            }
        }

        private void CreateViewport()
        {
            _SyncContext.Send(d =>
            {
                _SlamModel3D = new SlamModel3D(_SyncContext, false);

                _Viewport3d = new HelixViewport3D();
                _Viewport3d.Background = System.Windows.Media.Brushes.White;

                _Viewport3d.Camera = new PerspectiveCamera(new Point3D(0, 1, 0.3), new Vector3D(0, -1, -0.3), new Vector3D(0, 0, 1), 45);

                _Viewport3d.Children.Add(new DefaultLights());

                _Viewport3d.Children.Add(new CoordinateSystemVisual3D() { ArrowLengths = 0.1 });

                _ModelVisual3d = new ModelVisual3D();
                _Viewport3d.Children.Add(_ModelVisual3d);

                _CameraPosition = new CoordinateSystemVisual3D();
                _CameraPosition.ArrowLengths = 0.1;
                _Viewport3d.Children.Add(_CameraPosition);

                _TrajectoryKeyFrame = new LinesVisual3D();
                _TrajectoryKeyFrame.Color = System.Windows.Media.Colors.Gold;
                _TrajectoryKeyFrame.Thickness = 3;
                _Viewport3d.Children.Add(_TrajectoryKeyFrame);

                _TrajectoryFrame = new LinesVisual3D();
                _TrajectoryFrame.Color = System.Windows.Media.Colors.OrangeRed;
                _TrajectoryFrame.Thickness = 2;
                _Viewport3d.Children.Add(_TrajectoryFrame);
            }, null);
        }

        private Mat RenderViewport()
        {
            Mat image = new Mat();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                _SyncContext.Send(d =>
                {
                    _ModelVisual3d.Content = _SlamModel3D.Model;

                    _CameraPosition.Transform = _SlamModel3D.CameraPosition;
                    _TrajectoryFrame.Points = _SlamModel3D.TrajectoryFrame;
                    _TrajectoryKeyFrame.Points = _SlamModel3D.TrajectoryKeyFrame;

                    _Viewport3d.Viewport.Width = _VideoWidth - 512;
                    _Viewport3d.Viewport.Height = 2 * 512;
                    _Viewport3d.Viewport.ApplyTemplate();
                    _Viewport3d.Viewport.Measure(new System.Windows.Size(_VideoWidth - 512, 2 * 512));
                    _Viewport3d.Viewport.Arrange(new Rect(0, 0, _VideoWidth - 512, 2 * 512));
                    _Viewport3d.Viewport.UpdateLayout();

                    var renderTargetBitmap = _Viewport3d.Viewport.RenderBitmap(System.Windows.Media.Brushes.White, 1);

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