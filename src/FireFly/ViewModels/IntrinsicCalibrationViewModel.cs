using Emgu.CV;
using Emgu.CV.Util;
using FireFly.Command;
using FireFly.Models;
using FireFly.Utilities;
using FireFly.VI.Calibration;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FireFly.ViewModels
{
    public class IntrinsicCalibrationViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty ChAruCoBoardProperty =
            DependencyProperty.Register("ChAruCoBoard", typeof(CvImageContainer), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty ImagesProperty =
            DependencyProperty.Register("Images", typeof(ObservableCollection<ChArUcoImageContainer>), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(new ObservableCollection<ChArUcoImageContainer>()));

        public static readonly DependencyProperty MarkerLengthProperty =
            DependencyProperty.Register("MarkerLength", typeof(float), typeof(IntrinsicCalibrationViewModel), new FrameworkPropertyMetadata(0.0f, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty ResultControlVisibilityProperty =
            DependencyProperty.Register("ResultControlVisibility", typeof(Visibility), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty SquareLengthProperty =
            DependencyProperty.Register("SquareLength", typeof(float), typeof(IntrinsicCalibrationViewModel), new FrameworkPropertyMetadata(0.0f, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty SquaresXProperty =
            DependencyProperty.Register("SquaresX", typeof(int), typeof(IntrinsicCalibrationViewModel), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty SquaresYProperty =
            DependencyProperty.Register("SquaresY", typeof(int), typeof(IntrinsicCalibrationViewModel), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty TakeSnapshotControlVisibilityProperty =
            DependencyProperty.Register("TakeSnapshotControlVisibility", typeof(Visibility), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(Visibility.Collapsed));

        private double _Cx;
        private double _Cy;
        private List<double> _DistCoeffs = new List<double>();
        private double _Fx;
        private double _Fy;

        public IntrinsicCalibrationViewModel(MainViewModel parent) : base(parent)
        {
        }

        public RelayCommand<object> CalibrateCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoCalibrate(o);
                    });
            }
        }

        public CvImageContainer ChAruCoBoard
        {
            get { return (CvImageContainer)GetValue(ChAruCoBoardProperty); }
            set { SetValue(ChAruCoBoardProperty, value); }
        }

        public ObservableCollection<ChArUcoImageContainer> Images
        {
            get { return (ObservableCollection<ChArUcoImageContainer>)GetValue(ImagesProperty); }
            set { SetValue(ImagesProperty, value); }
        }

        public float MarkerLength
        {
            get { return (float)GetValue(MarkerLengthProperty); }
            set { SetValue(MarkerLengthProperty, value); }
        }

        public Visibility ResultControlVisibility
        {
            get { return (Visibility)GetValue(ResultControlVisibilityProperty); }
            set { SetValue(ResultControlVisibilityProperty, value); }
        }

        public float SquareLength
        {
            get { return (float)GetValue(SquareLengthProperty); }
            set { SetValue(SquareLengthProperty, value); }
        }

        public int SquaresX
        {
            get { return (int)GetValue(SquaresXProperty); }
            set { SetValue(SquaresXProperty, value); }
        }

        public int SquaresY
        {
            get { return (int)GetValue(SquaresYProperty); }
            set { SetValue(SquaresYProperty, value); }
        }

        public RelayCommand<object> StartCalibrationCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoStartCalibration(o);
                    });
            }
        }

        public RelayCommand<object> TakeSnapshotCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoTakeSnapshot(o);
                    });
            }
        }

        public Visibility TakeSnapshotControlVisibility
        {
            get { return (Visibility)GetValue(TakeSnapshotControlVisibilityProperty); }
            set { SetValue(TakeSnapshotControlVisibilityProperty, value); }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();

            SquaresX = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.SquaresX;
            SquaresY = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.SquaresY;
            MarkerLength = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.MarkerLength;
            SquareLength = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.SquareLength;

            bool changed = false;
            changed |= Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fx != _Fx;
            changed |= Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fy != _Fy;
            changed |= Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cx != _Cx;
            changed |= Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cy != _Cy;

            var firstNotSecond = _DistCoeffs.Except(Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs).ToList();
            var secondNotFirst = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs.Except(_DistCoeffs).ToList();

            changed |= firstNotSecond.Any() || secondNotFirst.Any();

            if (changed)
            {
                _Fx = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fx;
                _Fy = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fy;
                _Cx = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cx;
                _Cy = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cy;
                _DistCoeffs = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs.ToList();

                Mat board = ChArUcoDetector.DrawBoard(5, 5, 0.04f, 0.02f, new System.Drawing.Size(512, 512));
                Mat boardDist = board.Clone();

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

                CvInvoke.Undistort(board, boardDist, cameraMatrix, distCoeffs);
                Parent.SyncContext.Post(c =>
                {
                    ChAruCoBoard = new CvImageContainer();
                    ChAruCoBoard.CvImage = boardDist;
                }, null);
            }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IntrinsicCalibrationViewModel icvm = (d as IntrinsicCalibrationViewModel);
            bool changed = false;
            switch (e.Property.Name)
            {
                case "SquaresX":
                    changed = icvm.Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.SquaresX != icvm.SquaresX;
                    icvm.Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.SquaresX = icvm.SquaresX;
                    break;

                case "SquaresY":
                    changed = icvm.Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.SquaresY != icvm.SquaresY;
                    icvm.Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.SquaresY = icvm.SquaresY;
                    break;

                case "SquareLength":
                    changed = icvm.Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.SquareLength != icvm.SquareLength;
                    icvm.Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.SquareLength = icvm.SquareLength;
                    break;

                case "MarkerLength":
                    changed = icvm.Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.MarkerLength != icvm.MarkerLength;
                    icvm.Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.MarkerLength = icvm.MarkerLength;
                    break;

                default:
                    break;
            }
            if (changed)
            {
                icvm.Parent.UpdateSettings(false);
            }
        }

        private Task DoCalibrate(object o)
        {
            return Task.Factory.StartNew(async () =>
            {
                ChArUcoImageContainer cauic = new ChArUcoImageContainer();

                VectorOfInt allIds = new VectorOfInt();
                VectorOfVectorOfPointF allCorners = new VectorOfVectorOfPointF();
                VectorOfInt markerCounterPerFrame = new VectorOfInt();
                int squaresX = 0;
                int squaresY = 0;
                float squareLength = 0f;
                float markerLength = 0f;
                System.Drawing.Size size = new System.Drawing.Size();

                Parent.SyncContext.Send(async c =>
                {
                    squaresX = SquaresX;
                    squaresY = SquaresY;
                    squareLength = SquareLength;
                    markerLength = MarkerLength;
                    size = Parent.CameraViewModel.Image.CvImage.Size;
                    foreach (ChArUcoImageContainer image in Images)
                    {
                        if (image.MarkerCorners != null && image.MarkerCorners.Size > 0)
                        {
                            allIds.Push(image.MarkerIds);
                            allCorners.Push(image.MarkerCorners);
                            markerCounterPerFrame.Push(new int[] { image.MarkerCorners.Size });
                        }
                    }
                }, null);

                if (markerCounterPerFrame.Size > 0)
                {
                    MetroDialogSettings settings = new MetroDialogSettings()
                    {
                        AnimateShow = false,
                        AnimateHide = false
                    };

                    var controller = await Parent.DialogCoordinator.ShowProgressAsync(Parent, "Please wait...", "Calculating calibration parameter now!", settings: settings);
                    controller.SetIndeterminate();
                    controller.SetCancelable(false);

                    (Mat cameraMatrix, Mat distCoeffs) result = ChArUcoDetector.Calibrate(squaresX, squaresY, squareLength, markerLength, size, allIds, allCorners, markerCounterPerFrame);

                    await controller.CloseAsync();

                    Parent.SyncContext.Post(async c =>
                    {
                        Images.Clear();

                        Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fx = result.cameraMatrix.GetValue(0, 0);
                        Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fy = result.cameraMatrix.GetValue(1, 1);
                        Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cx = result.cameraMatrix.GetValue(0, 2);
                        Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cy = result.cameraMatrix.GetValue(1, 2);

                        Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs.Clear();
                        for (int i = 0; i < result.distCoeffs.Cols && i < 8; i++)
                        {
                            Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs.Add(result.distCoeffs.GetValue(0, i));
                        }

                        TakeSnapshotControlVisibility = Visibility.Collapsed;
                        ResultControlVisibility = Visibility.Visible;

                        Parent.UpdateSettings(false);
                    }, null);
                }
                else
                {
                    await Parent.DialogCoordinator.ShowMessageAsync(Parent, "Error", "Not enough valide input frames available!");
                }
            });
        }

        private Task DoStartCalibration(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    TakeSnapshotControlVisibility = Visibility.Visible;
                    ResultControlVisibility = Visibility.Collapsed;
                }, null);
            });
        }

        private Task DoTakeSnapshot(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    ChArUcoImageContainer cauic = new ChArUcoImageContainer();
                    cauic.OriginalImage = Parent.CameraViewModel.Image;
                    Images.Add(cauic);
                }, null);
            });
        }
    }
}