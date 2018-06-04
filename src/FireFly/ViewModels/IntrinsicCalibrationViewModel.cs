using Emgu.CV;
using Emgu.CV.Util;
using FireFly.Command;
using FireFly.Models;
using FireFly.VI.Calibration;
using System.Collections.ObjectModel;
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
            SquaresX = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.SquaresX;
            SquaresY = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.SquaresY;
            MarkerLength = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.MarkerLength;
            SquareLength = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.SquareLength;
        }

        internal override void UpdateLinkUpBindings()
        {
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
                icvm.Parent.SettingsUpdated(false);
            }
        }

        private Task DoCalibrate(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    ChArUcoImageContainer cauic = new ChArUcoImageContainer();

                    VectorOfInt allIds = new VectorOfInt();
                    VectorOfVectorOfPointF allCorners = new VectorOfVectorOfPointF();
                    VectorOfInt markerCounterPerFrame = new VectorOfInt();

                    foreach (ChArUcoImageContainer image in Images)
                    {
                        allIds.Push(image.MarkerIds);
                        allCorners.Push(image.MarkerCorners);
                        markerCounterPerFrame.Push(new int[] { image.MarkerCorners.Size });
                    }

                    //TODO: settings

                    (Mat cameraMatrix, Mat distCoeffs) result = ChArUcoDetector.Calibrate(5, 7, 0.04f, 0.02f, Parent.CameraViewModel.Image.CvImage.Size, allIds, allCorners, markerCounterPerFrame);
                    Images.Clear();

                    ChAruCoBoard = new CvImageContainer();
                    Mat board = ChArUcoDetector.DrawBoard(5, 7, 0.04f, 0.02f, Parent.CameraViewModel.Image.CvImage.Size);
                    Mat boardDist = board.Clone();

                    CvInvoke.Undistort(board, boardDist, result.cameraMatrix, result.distCoeffs);

                    ChAruCoBoard.CvImage = boardDist;

                    TakeSnapshotControlVisibility = Visibility.Collapsed;
                    ResultControlVisibility = Visibility.Visible;
                }, null);
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