using Emgu.CV;
using FireFly.Models;
using FireFly.Utilities;
using LinkUp.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FireFly.ViewModels
{
    public class CameraViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(CameraViewModel), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty FPSProperty =
            DependencyProperty.Register("FPS", typeof(int), typeof(CameraViewModel), new PropertyMetadata(0));

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(CvImageContainer), typeof(CameraViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty QualityProperty =
            DependencyProperty.Register("Quality", typeof(int), typeof(CameraViewModel), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnPropertyChanged)));



        public bool Undistort
        {
            get { return (bool)GetValue(UndistortProperty); }
            set { SetValue(UndistortProperty, value); }
        }

        public static readonly DependencyProperty UndistortProperty =
            DependencyProperty.Register("Undistort", typeof(bool), typeof(CameraViewModel), new PropertyMetadata(false));



        private LinkUpEventLabel _EventLabel;

        private FPSCounter _FPSCounter = new FPSCounter();

        private LinkUpPropertyLabel<byte> _QualityLabel;

        private System.Timers.Timer _Timer;

        private double _Cx;
        private double _Cy;
        private List<double> _DistCoeffs = new List<double>();
        private double _Fx;
        private double _Fy;

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

        public int Quality
        {
            get { return (int)GetValue(QualityProperty); }
            set { SetValue(QualityProperty, value); }
        }

        internal override void SettingsUpdated()
        {
            Quality = Parent.SettingContainer.Settings.StreamingSettings.Quality;
            Enabled = Parent.SettingContainer.Settings.StreamingSettings.CameraRawStreamEnabled;

            _Fx = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fx;
            _Fy = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fy;
            _Cx = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cx;
            _Cy = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cy;
            _DistCoeffs = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs.ToList();
        }

        internal override void UpdateLinkUpBindings()
        {
            if (Parent.Node != null)
            {
                _EventLabel = Parent.Node.GetLabelByName<LinkUpEventLabel>("firefly/test/label_event");
                if (Enabled)
                    _EventLabel.Subscribe();
                else
                    _EventLabel.Unsubscribe();
                _EventLabel.Fired += EventLabel_Fired;

                _QualityLabel = Parent.Node.GetLabelByName<LinkUpPropertyLabel<byte>>("firefly/test/jpeg_quality");
            }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CameraViewModel cvm = (d as CameraViewModel);
            bool changed = false;
            switch (e.Property.Name)
            {
                case "Quality":
                    changed = cvm.Parent.SettingContainer.Settings.StreamingSettings.Quality != cvm.Quality;
                    cvm.Parent.SettingContainer.Settings.StreamingSettings.Quality = cvm.Quality;
                    try
                    {
                        if (changed)
                            cvm._QualityLabel.Value = (byte)cvm.Quality;
                    }
                    catch (Exception) { }
                    break;

                case "Enabled":
                    changed = cvm.Parent.SettingContainer.Settings.StreamingSettings.CameraRawStreamEnabled != cvm.Enabled;
                    cvm.Parent.SettingContainer.Settings.StreamingSettings.CameraRawStreamEnabled = cvm.Enabled;
                    try
                    {
                        if (changed)
                        {
                            if (cvm.Enabled)
                                cvm._EventLabel.Subscribe();
                            else
                                cvm._EventLabel.Unsubscribe();
                        }
                    }
                    catch (Exception) { }
                    break;

                default:
                    break;
            }
            if (changed)
            {
                cvm.Parent.SettingsUpdated(false);
            }
        }

        private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Parent.SyncContext.Post(o =>
            {
                FPS = (int)_FPSCounter.FramesPerSecond;
            }, null);
        }

        private void EventLabel_Fired(LinkUpEventLabel label, byte[] data)
        {
            Mat mat = new Mat();
            Mat matUndist = new Mat();
            CvInvoke.Imdecode(data, Emgu.CV.CvEnum.ImreadModes.Grayscale, mat);

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
                Image = new CvImageContainer();
                if(Undistort)
                    Image.CvImage = matUndist;
                else
                    Image.CvImage = mat;
            }
            , null);
        }
    }
}