using Emgu.CV;
using Emgu.CV.CvEnum;
using FireFly.Models;
using FireFly.Proxy;
using FireFly.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public static readonly DependencyProperty FOVScaleProperty =
            DependencyProperty.Register("FOVScale", typeof(double), typeof(CameraViewModel), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty FPSProperty =
            DependencyProperty.Register("FPS", typeof(int), typeof(CameraViewModel), new PropertyMetadata(0));

        public static readonly DependencyProperty GammaCorretionProperty =
            DependencyProperty.Register("GammaCorretion", typeof(bool), typeof(CameraViewModel), new PropertyMetadata(false));

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(CvImageContainer), typeof(CameraViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty UndistortProperty =
            DependencyProperty.Register("Undistort", typeof(bool), typeof(CameraViewModel), new PropertyMetadata(false));

        public static readonly DependencyProperty VignetteCorretionProperty =
            DependencyProperty.Register("VignetteCorretion", typeof(bool), typeof(CameraViewModel), new PropertyMetadata(false));

        private double _Alpha;

        private double _Cx;

        private double _Cy;

        private List<double> _DistCoeffs = new List<double>();

        private double _ExposureTime;

        private bool _FishEyeCalibration = true;

        private FPSCounter _FPSCounter = new FPSCounter();

        private double _Fx;

        private double _Fy;

        private Timer _Timer;

        public CameraViewModel(MainViewModel parent) : base(parent)
        {
            _Timer = new Timer(300);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
        }

        public Mat CenteredCameraMatrix
        {
            get
            {
                Mat newCameraMatrix = NewCameraMatrix;
                Parent.SyncContext.Send(d =>
                {
                    newCameraMatrix.SetValue(0, 2, ImageWidth / 2);
                    newCameraMatrix.SetValue(1, 2, ImageHeight / 2);
                }, null);
                return newCameraMatrix;
            }
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

        public double FOVScale
        {
            get { return (double)GetValue(FOVScaleProperty); }
            set { SetValue(FOVScaleProperty, value); }
        }

        public int FPS
        {
            get { return (int)GetValue(FPSProperty); }
            set { SetValue(FPSProperty, value); }
        }

        public bool GammaCorretion
        {
            get { return (bool)GetValue(GammaCorretionProperty); }
            set { SetValue(GammaCorretionProperty, value); }
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
                return Parent.SettingContainer.Settings.CameraSettings.Height;
            }
        }

        public int ImageWidth
        {
            get
            {
                return Parent.SettingContainer.Settings.CameraSettings.Width;
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
                Mat newCameraMatrix = new Mat();
                Parent.SyncContext.Send(d =>
                {
                    if (FishEyeCalibration)
                    {
                        Fisheye.EstimateNewCameraMatrixForUndistorRectify(OrginalCameraMatrix, DistortionCoefficients, new System.Drawing.Size(ImageWidth, ImageHeight), Mat.Eye(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1), newCameraMatrix, 0, new System.Drawing.Size(ImageWidth, ImageHeight), FOVScale);
                        newCameraMatrix.SetValue(0, 2, ImageWidth / 2);
                        newCameraMatrix.SetValue(1, 2, ImageHeight / 2);
                    }
                    else
                    {
                        newCameraMatrix = OrginalCameraMatrix;
                    }
                }, null);
                return newCameraMatrix;
            }
        }

        public Mat OrginalCameraMatrix
        {
            get
            {
                Mat cameraMatrix = new Mat(3, 3, DepthType.Cv64F, 1);

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

        public bool VignetteCorretion
        {
            get { return (bool)GetValue(VignetteCorretionProperty); }
            set { SetValue(VignetteCorretionProperty, value); }
        }

        public void Fired(IOProxy proxy, List<AbstractProxyEventData> eventData)
        {
            if (eventData.Count == 1 && eventData[0] is CameraEventData)
            {
                Task.Factory.StartNew(() =>
                {
                    Mat mat = (eventData[0] as CameraEventData).Image;

                    bool undistort = false;

                    Parent.SyncContext.Send(o =>
                    {
                        undistort = Undistort;

                        if (GammaCorretion)
                        {
                            Mat result = new Mat(ImageWidth, ImageHeight, DepthType.Cv32F, 1);
                            int i = 0;
                            Mat lut = new Mat(1, 256, DepthType.Cv32F, 1);
                            foreach (double val in Parent.SettingContainer.Settings.CalibrationSettings.PhotometricCalibrationSettings.ResponseValues)
                            {
                                lut.SetValue(0, i++, (byte)val);
                            }
                            CvInvoke.LUT(mat, lut, result);
                            result.ConvertTo(mat, DepthType.Cv8U);
                        }

                        if (VignetteCorretion)
                        {
                            Mat invVignette = new Mat(ImageWidth, ImageHeight, DepthType.Cv32F, 1);
                            Mat result = new Mat(ImageWidth, ImageHeight, DepthType.Cv32F, 1);
                            CvInvoke.Divide(Mat.Ones(ImageWidth, ImageHeight, DepthType.Cv32F, 1), Parent.CalibrationViewModel.PhotometricCalibrationViewModel.Vignette.CvImage, invVignette, 255, DepthType.Cv32F);
                            CvInvoke.Multiply(mat, invVignette, result, 1, DepthType.Cv32F);
                            result.ConvertTo(mat, DepthType.Cv8U);
                        }
                    }
                    , null);

                    if (undistort)
                    {
                        Mat matUndist = new Mat(ImageWidth, ImageHeight, DepthType.Cv8U, 1);

                        if (FishEyeCalibration)
                        {
                            Mat map1 = new Mat();
                            Mat map2 = new Mat();
                            Fisheye.InitUndistorRectifyMap(OrginalCameraMatrix, DistortionCoefficients, Mat.Eye(3, 3, DepthType.Cv64F, 1), CenteredCameraMatrix, new System.Drawing.Size(ImageWidth, ImageHeight), Emgu.CV.CvEnum.DepthType.Cv32F, map1, map2);
                            CvInvoke.Remap(mat, matUndist, map1, map2, Inter.Linear, BorderType.Constant);
                        }
                        else
                        {
                            CvInvoke.Undistort(mat, matUndist, CenteredCameraMatrix, DistortionCoefficients);
                        }
                        mat = matUndist;
                    }

                    _FPSCounter.CountFrame();
                    Parent.SyncContext.Post(o =>
                    {
                        ExposureTime = (eventData[0] as CameraEventData).ExposureTime;
                        Image = new CvImageContainer();

                        Image.CvImage = mat;
                    }
                    , null);
                });
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

            FOVScale = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.FOVScale;
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

                case "FOVScale":
                    changed = cvm.Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.FOVScale != cvm.FOVScale;
                    cvm.Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.FOVScale = cvm.FOVScale;
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
                    _ExposureTime *= 1.05;
                    ExposureTimeSetting = (short)(_ExposureTime - 1);

                    if (ExposureTimeSetting > MaxExposureTime)
                    {
                        ExposureTimeSetting = 0;
                        _ExposureTime = 1;
                    }
                }
                else
                {
                    _ExposureTime = 1;
                }
                FPS = (int)_FPSCounter.FramesPerSecond;
            }, null);
        }
    }
}