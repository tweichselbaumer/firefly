using Emgu.CV;
using FireFly.Models;
using FireFly.Proxy;
using FireFly.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;

namespace FireFly.ViewModels
{
    public class CameraViewModel : AbstractViewModel, IProxyEventSubscriber
    {
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(CameraViewModel), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty ExposureSweepProperty =
            DependencyProperty.Register("ExposureSweep", typeof(bool), typeof(CameraViewModel), new PropertyMetadata(false));

        public static readonly DependencyProperty ExposureTimeProperty =
            DependencyProperty.Register("ExposureTime", typeof(double), typeof(CameraViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty ExposureTimeSettingProperty =
                    DependencyProperty.Register("ExposureTimeSetting", typeof(short), typeof(CameraViewModel), new FrameworkPropertyMetadata((short)-1, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty FPSProperty =
            DependencyProperty.Register("FPS", typeof(int), typeof(CameraViewModel), new PropertyMetadata(0));

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(CvImageContainer), typeof(CameraViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty UndistortProperty =
            DependencyProperty.Register("Undistort", typeof(bool), typeof(CameraViewModel), new PropertyMetadata(false));

        private double _Alpha;
        private double _Cx;

        private double _Cy;

        private List<double> _DistCoeffs = new List<double>();

        private bool _FishEyeCalibration = true;
        private FPSCounter _FPSCounter = new FPSCounter();

        private double _Fx;

        private double _Fy;
        private Timer _Timer;

        public CameraViewModel(MainViewModel parent) : base(parent)
        {
            _Timer = new Timer(100);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
        }

        public Mat DistortionCoefficients
        {
            get
            {
                Mat distCoeffs = new Mat(1, FishEyeCalibration ? 4 : _DistCoeffs.Count, Emgu.CV.CvEnum.DepthType.Cv64F, 1);
                if ((FishEyeCalibration ? 4 : _DistCoeffs.Count) <= _DistCoeffs.Count)
                {
                    for (int i = 0; i < distCoeffs.Cols && (FishEyeCalibration ? i < 4 : true); i++)
                    {
                        distCoeffs.SetValue(0, i, _DistCoeffs[i]);
                    }
                }

                return distCoeffs;
            }
        }

        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        public bool ExposureSweep
        {
            get { return (bool)GetValue(ExposureSweepProperty); }
            set { SetValue(ExposureSweepProperty, value); }
        }

        public double ExposureTime
        {
            get { return (double)GetValue(ExposureTimeProperty); }
            set { SetValue(ExposureTimeProperty, value); }
        }

        public short ExposureTimeSetting
        {
            get { return (short)GetValue(ExposureTimeSettingProperty); }
            set { SetValue(ExposureTimeSettingProperty, value); }
        }

        public bool FishEyeCalibration
        {
            get
            {
                return _FishEyeCalibration;
            }
        }

        public int FPS
        {
            get { return (int)GetValue(FPSProperty); }
            set { SetValue(FPSProperty, value); }
        }

        public CvImageContainer Image
        {
            get { return (CvImageContainer)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public int ImageHeight
        {
            get
            {
                return 512;
            }
        }

        public int ImageWidth
        {
            get
            {
                return 512;
            }
        }

        public int MaxExposureTime
        {
            get
            {
                return 1213;
            }
        }

        public Mat NewCameraMatrix
        {
            get
            {
                if (FishEyeCalibration)
                {
                    Mat newCameraMatrix = new Mat();
                    Fisheye.EstimateNewCameraMatrixForUndistorRectify(OrginalCameraMatrix, DistortionCoefficients, new System.Drawing.Size(ImageWidth, ImageHeight), Mat.Eye(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1), newCameraMatrix, 0, new System.Drawing.Size(ImageWidth, ImageHeight), 0.3);
                    newCameraMatrix.SetValue(0, 2, ImageWidth / 2);
                    newCameraMatrix.SetValue(1, 2, ImageHeight / 2);
                    return newCameraMatrix;
                }
                else
                {
                    return OrginalCameraMatrix;
                }
            }
        }

        public Mat OrginalCameraMatrix
        {
            get
            {
                Mat cameraMatrix = new Mat(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1);

                cameraMatrix.SetValue(0, 0, _Fx);
                cameraMatrix.SetValue(1, 1, _Fy);
                cameraMatrix.SetValue(0, 1, _Fx * _Alpha);
                cameraMatrix.SetValue(0, 2, _Cx);
                cameraMatrix.SetValue(1, 2, _Cy);
                cameraMatrix.SetValue(2, 2, 1.0f);

                return cameraMatrix;
            }
        }

        public bool Undistort
        {
            get { return (bool)GetValue(UndistortProperty); }
            set { SetValue(UndistortProperty, value); }
        }

        public void Fired(IOProxy proxy, List<AbstractProxyEventData> eventData)
        {
            if (eventData.Count == 1 && eventData[0] is CameraEventData)
            {
                Mat matUndist = new Mat(ImageWidth, ImageHeight, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                Mat mat = (eventData[0] as CameraEventData).Image;

                if (FishEyeCalibration)
                {
                    Mat map1 = new Mat();
                    Mat map2 = new Mat();
                    Fisheye.InitUndistorRectifyMap(OrginalCameraMatrix, DistortionCoefficients, Mat.Eye(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1), NewCameraMatrix, new System.Drawing.Size(ImageWidth, ImageHeight), Emgu.CV.CvEnum.DepthType.Cv32F, map1, map2);
                    CvInvoke.Remap(mat, matUndist, map1, map2, Emgu.CV.CvEnum.Inter.Linear, Emgu.CV.CvEnum.BorderType.Constant);
                }
                else
                {
                    CvInvoke.Undistort(mat, matUndist, NewCameraMatrix, DistortionCoefficients);
                }

                _FPSCounter.CountFrame();
                Parent.SyncContext.Post(o =>
                {
                    ExposureTime = (eventData[0] as CameraEventData).ExposureTime;
                    Image = new CvImageContainer();
                    if (Undistort)
                        Image.CvImage = matUndist;
                    else
                        Image.CvImage = mat;
                }
                , null);
            }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();

            Enabled = Parent.SettingContainer.Settings.StreamingSettings.CameraRawStreamEnabled;

            _Fx = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fx;
            _Fy = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fy;
            _Cx = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cx;
            _Cy = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cy;
            _Alpha = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Alpha;
            _DistCoeffs = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs.ToList();
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CameraViewModel cvm = (d as CameraViewModel);
            bool changed = false;
            switch (e.Property.Name)
            {
                case "Enabled":
                    changed = cvm.Parent.SettingContainer.Settings.StreamingSettings.CameraRawStreamEnabled != cvm.Enabled;
                    cvm.Parent.SettingContainer.Settings.StreamingSettings.CameraRawStreamEnabled = cvm.Enabled;
                    try
                    {
                        if (cvm.Enabled)
                            cvm.Parent.IOProxy.Subscribe(cvm, ProxyEventType.CameraEvent);
                        else
                            cvm.Parent.IOProxy.Unsubscribe(cvm, ProxyEventType.CameraEvent);
                    }
                    catch (Exception) { }
                    break;

                case "ExposureTimeSetting":
                    cvm.Parent.IOProxy.SetExposure(cvm.ExposureTimeSetting);
                    break;

                default:
                    break;
            }
            if (changed)
            {
                cvm.Parent.UpdateSettings(false);
            }
        }

        private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Parent.SyncContext.Post(o =>
            {
                if (ExposureSweep)
                {
                    ExposureTimeSetting++;

                    if (ExposureTimeSetting > MaxExposureTime)
                    {
                        ExposureTimeSetting = 0;
                    }
                }
                FPS = (int)_FPSCounter.FramesPerSecond;
            }, null);
        }
    }
}