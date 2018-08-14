using Emgu.CV;
using FireFly.Models;
using FireFly.Proxy;
using FireFly.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FireFly.ViewModels
{
    public class CameraViewModel : AbstractViewModel, IProxyEventSubscriber
    {
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(CameraViewModel), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

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

        private double _Cx;

        private double _Cy;

        private List<double> _DistCoeffs = new List<double>();

        private FPSCounter _FPSCounter = new FPSCounter();

        private double _Fx;

        private double _Fy;

        private System.Timers.Timer _Timer;

        public CameraViewModel(MainViewModel parent) : base(parent)
        {
            _Timer = new System.Timers.Timer(1000);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
        }

        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
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

        public bool Undistort
        {
            get { return (bool)GetValue(UndistortProperty); }
            set { SetValue(UndistortProperty, value); }
        }

        public void Fired(IOProxy proxy, List<AbstractProxyEventData> eventData)
        {
            if (eventData.Count == 1 && eventData[0] is CameraEventData)
            {
                Mat matUndist = new Mat();
                Mat mat = (eventData[0] as CameraEventData).Image;

                Mat cameraMatrix = new Mat(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1);
                Mat distCoeffs = new Mat(1, _DistCoeffs.Count, Emgu.CV.CvEnum.DepthType.Cv64F, 1);

                cameraMatrix.SetValue(0, 0, _Fx);
                cameraMatrix.SetValue(1, 1, _Fy);
                cameraMatrix.SetValue(0, 2, _Cy);
                cameraMatrix.SetValue(1, 2, _Cy);
                cameraMatrix.SetValue(2, 2, 1.0f);

                for (int i = 0; i < distCoeffs.Cols; i++)
                {
                    distCoeffs.SetValue(0, i, _DistCoeffs[i]);
                }

                CvInvoke.Undistort(mat, matUndist, cameraMatrix, distCoeffs);

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

        private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Parent.SyncContext.Post(o =>
            {
                FPS = (int)_FPSCounter.FramesPerSecond;
            }, null);
        }
    }
}