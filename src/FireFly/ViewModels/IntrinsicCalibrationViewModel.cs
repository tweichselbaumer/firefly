using Emgu.CV;
using Emgu.CV.Util;
using FireFly.Command;
using FireFly.CustomDialogs;
using FireFly.Models;
using FireFly.Utilities;
using FireFly.VI.Calibration;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using static Emgu.CV.Aruco.Dictionary;

namespace FireFly.ViewModels
{
    public class IntrinsicCalibrationViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty AutoSnapshotProperty =
            DependencyProperty.Register("AutoSnapshot", typeof(bool), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(false));

        public static readonly DependencyProperty ChAruCoBoardProperty =
            DependencyProperty.Register("ChAruCoBoard", typeof(CvImageContainer), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty DictionaryProperty =
            DependencyProperty.Register("Dictionary", typeof(PredefinedDictionaryName), typeof(IntrinsicCalibrationViewModel), new FrameworkPropertyMetadata(PredefinedDictionaryName.Dict4X4_50, new PropertyChangedCallback(OnPropertyChanged)));

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

        private Timer _Timer;

        public IntrinsicCalibrationViewModel(MainViewModel parent) : base(parent)
        {
            _Timer = new Timer(500);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
        }

        public bool AutoSnapshot
        {
            get { return (bool)GetValue(AutoSnapshotProperty); }
            set { SetValue(AutoSnapshotProperty, value); }
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

        public PredefinedDictionaryName Dictionary
        {
            get { return (PredefinedDictionaryName)GetValue(DictionaryProperty); }
            set { SetValue(DictionaryProperty, value); }
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

        public IEnumerable<PredefinedDictionaryName> PredefinedDictionaryNames
        {
            get
            {
                return Enum.GetValues(typeof(PredefinedDictionaryName)).Cast<PredefinedDictionaryName>();
            }
        }

        public RelayCommand<object> PrintBoardCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoPrintBoard(o);
                    });
            }
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
            Dictionary = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Dictionary;

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

                Mat board = ChArUcoCalibration.DrawBoard(5, 5, 0.04f, 0.02f, new System.Drawing.Size(512, 512), 10, Emgu.CV.Aruco.Dictionary.PredefinedDictionaryName.Dict6X6_250);
                Mat boardDist = board.Clone();

                Mat cameraMatrix = new Mat(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1);
                Mat distCoeffs = new Mat(1, Parent.CameraViewModel.FishEyeCalibration ? 4 : _DistCoeffs.Count, Emgu.CV.CvEnum.DepthType.Cv64F, 1);

                cameraMatrix.SetValue(0, 0, _Fx);
                cameraMatrix.SetValue(1, 1, _Fy);
                cameraMatrix.SetValue(0, 2, _Cx);
                cameraMatrix.SetValue(1, 2, _Cy);
                cameraMatrix.SetValue(2, 2, 1.0f);

                Mat newK = new Mat(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1);

                for (int i = 0; i < distCoeffs.Cols && (Parent.CameraViewModel.FishEyeCalibration ? i < 4 : true); i++)
                {
                    distCoeffs.SetValue(0, i, _DistCoeffs[i]);
                }

                if (Parent.CameraViewModel.FishEyeCalibration)
                {
                    Fisheye.EstimateNewCameraMatrixForUndistorRectify(cameraMatrix, distCoeffs, new System.Drawing.Size(512, 512), Mat.Eye(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1), newK, 1, new System.Drawing.Size(512, 512), 1);
                    Mat map1 = new Mat();
                    Mat map2 = new Mat();
                    Fisheye.InitUndistorRectifyMap(cameraMatrix, distCoeffs, Mat.Eye(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1), newK, new System.Drawing.Size(512, 512), Emgu.CV.CvEnum.DepthType.Cv32F, map1, map2);
                    CvInvoke.Remap(board, boardDist, map1, map2, Emgu.CV.CvEnum.Inter.Linear, Emgu.CV.CvEnum.BorderType.Constant);
                }
                else
                {
                    CvInvoke.Undistort(board, boardDist, cameraMatrix, distCoeffs);
                }
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

                case "Dictionary":
                    changed = icvm.Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Dictionary != icvm.Dictionary;
                    icvm.Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Dictionary = icvm.Dictionary;
                    break;

                default:
                    break;
            }
            if (changed)
            {
                icvm.Parent.UpdateSettings(false);
            }
        }

        private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Parent.SyncContext.Post(o =>
            {
                if (AutoSnapshot)
                {
                    ChArUcoImageContainer cauic = new ChArUcoImageContainer(SquaresX, SquaresY, SquareLength, MarkerLength, Dictionary);
                    cauic.OriginalImage = Parent.CameraViewModel.Image;
                    Images.Add(cauic);
                }
            }, null);
        }

        private Task DoCalibrate(object o)
        {
            return Task.Factory.StartNew(async () =>
            {
                VectorOfInt allIds = new VectorOfInt();
                VectorOfVectorOfPointF allCorners = new VectorOfVectorOfPointF();
                VectorOfInt allCharucoIds = new VectorOfInt();
                VectorOfPointF allCharucoCorners = new VectorOfPointF();
                VectorOfInt markerCounterPerFrame = new VectorOfInt();
                int squaresX = 0;
                int squaresY = 0;
                float squareLength = 0f;
                float markerLength = 0f;
                PredefinedDictionaryName dictionary = PredefinedDictionaryName.Dict4X4_50;
                System.Drawing.Size size = new System.Drawing.Size();

                bool fisheye = false;

                Parent.SyncContext.Send(async c =>
                {
                    fisheye = Parent.CameraViewModel.FishEyeCalibration;

                    squaresX = SquaresX;
                    squaresY = SquaresY;
                    squareLength = SquareLength;
                    markerLength = MarkerLength;
                    size = Parent.CameraViewModel.Image.CvImage.Size;
                    dictionary = Dictionary;
                    foreach (ChArUcoImageContainer image in Images)
                    {
                        if (image.MarkerCorners != null && image.CharucoCorners.Size > 4)
                        {
                            allIds.Push(image.MarkerIds);
                            allCorners.Push(image.MarkerCorners);
                            allCharucoIds.Push(image.CharucoIds);
                            allCharucoCorners.Push(image.CharucoCorners);
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

                    bool error = false;
                    (Mat cameraMatrix, Mat distCoeffs, double rms) result = (null, null, 0.0);
                    try
                    {
                        result = ChArUcoCalibration.CalibrateCharuco(squaresX, squaresY, squareLength, markerLength, dictionary, size, allCharucoIds, allCharucoCorners, markerCounterPerFrame, fisheye, delegate (byte[] input)
                        {
                            return Parent.IOProxy.GetRemoteChessboardCorner(input);
                        });
                    }
                    catch (Exception ex)
                    {
                        error = true;
                    }

                    await controller.CloseAsync();
                    if (!error)
                    {
                        var con = await Parent.DialogCoordinator.ShowMessageAsync(Parent, "Result", string.Format("RMS: {0}\nDo you want to save?", result.rms), MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative, null);
                        if (con == MessageDialogResult.Affirmative)
                        {
                            Parent.SyncContext.Post(async c =>
                            {
                                Images.Clear();

                                Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fx = result.cameraMatrix.GetValue(0, 0);
                                Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fy = result.cameraMatrix.GetValue(1, 1);
                                Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cx = result.cameraMatrix.GetValue(0, 2);
                                Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cy = result.cameraMatrix.GetValue(1, 2);

                                Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs.Clear();

                                if (Parent.CameraViewModel.FishEyeCalibration)
                                {
                                    for (int i = 0; i < result.distCoeffs.Rows && i < 8; i++)
                                    {
                                        Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs.Add(result.distCoeffs.GetValue(i, 0));
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < result.distCoeffs.Cols && i < 8; i++)
                                    {
                                        Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs.Add(result.distCoeffs.GetValue(0, i));
                                    }
                                }

                                TakeSnapshotControlVisibility = Visibility.Collapsed;
                                ResultControlVisibility = Visibility.Visible;

                                Parent.UpdateSettings(false);
                            }, null);
                        }
                    }
                    else
                    {
                        await Parent.DialogCoordinator.ShowMessageAsync(Parent, "Error", "Error during calibration!");
                    }
                }
                else
                {
                    await Parent.DialogCoordinator.ShowMessageAsync(Parent, "Error", "Not enough valide input frames available!");
                }
            });
        }

        private Task DoPrintBoard(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    CustomDialog customDialog = new CustomDialog() { Title = "Board Properties" };

                    var dataContext = new PrintCharucoBoardDialogModel(obj =>
                    {
                        Parent.SyncContext.Post(d =>
                        {
                            Parent.DialogCoordinator.HideMetroDialogAsync(Parent, customDialog);

                            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                            saveFileDialog.Filter = "Image (*.png) | *.png";

                            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                CvInvoke.Imwrite(saveFileDialog.FileName, obj.Image.CvImage, new KeyValuePair<Emgu.CV.CvEnum.ImwriteFlags, int>() { });
                        }, null);
                    });
                    customDialog.Content = new PrintCharucoBoardDialog { DataContext = dataContext };

                    Parent.DialogCoordinator.ShowMetroDialogAsync(Parent, customDialog);
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
                    ChArUcoImageContainer cauic = new ChArUcoImageContainer(SquaresX, SquaresY, SquareLength, MarkerLength, Dictionary);
                    cauic.OriginalImage = Parent.CameraViewModel.Image;
                    Images.Add(cauic);
                }, null);
            });
        }
    }
}