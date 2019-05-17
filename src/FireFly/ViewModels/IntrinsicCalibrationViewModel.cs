using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using FireFly.Command;
using FireFly.CustomDialogs;
using FireFly.Data.Storage;
using FireFly.Data.Storage.Model;
using FireFly.Models;
using FireFly.Utilities;
using FireFly.VI.Calibration;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using static Emgu.CV.Aruco.Dictionary;

namespace FireFly.ViewModels
{
    public class IntrinsicCalibrationViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty AlphaProperty =
            DependencyProperty.Register("Alpha", typeof(double), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty AutoSnapshotProperty =
                    DependencyProperty.Register("AutoSnapshot", typeof(bool), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(false));

        public static readonly DependencyProperty ChAruCoBoardProperty =
            DependencyProperty.Register("ChAruCoBoard", typeof(CvImageContainer), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty CxProperty =
            DependencyProperty.Register("Cx", typeof(double), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty CyProperty =
            DependencyProperty.Register("Cy", typeof(double), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty FxProperty =
            DependencyProperty.Register("Fx", typeof(double), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty FyProperty =
            DependencyProperty.Register("Fy", typeof(double), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty ImagesProperty =
            DependencyProperty.Register("Images", typeof(ObservableCollection<ChArUcoImageContainer>), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(new ObservableCollection<ChArUcoImageContainer>()));

        public static readonly DependencyProperty K1Property =
            DependencyProperty.Register("K1", typeof(double), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty K2Property =
            DependencyProperty.Register("K2", typeof(double), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty K3Property =
            DependencyProperty.Register("K3", typeof(double), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty K4Property =
            DependencyProperty.Register("K4", typeof(double), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty ResultControlVisibilityProperty =
            DependencyProperty.Register("ResultControlVisibility", typeof(Visibility), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty TakeSnapshotControlVisibilityProperty =
            DependencyProperty.Register("TakeSnapshotControlVisibility", typeof(Visibility), typeof(IntrinsicCalibrationViewModel), new PropertyMetadata(Visibility.Collapsed));

        private List<double> _DistCoeffs = new List<double>();

        private Timer _Timer;

        public IntrinsicCalibrationViewModel(MainViewModel parent) : base(parent)
        {
            _Timer = new Timer(500);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
        }

        public double Alpha
        {
            get { return (double)GetValue(AlphaProperty); }
            set { SetValue(AlphaProperty, value); }
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

        public RelayCommand<object> ClearCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoClear(o);
                    });
            }
        }

        public RelayCommand<object> CloseCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoClose(o);
                    });
            }
        }

        public double Cx
        {
            get { return (double)GetValue(CxProperty); }
            set { SetValue(CxProperty, value); }
        }

        public double Cy
        {
            get { return (double)GetValue(CyProperty); }
            set { SetValue(CyProperty, value); }
        }

        public RelayCommand<object> DeleteCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoDelete(o);
                    });
            }
        }

        public RelayCommand<object> ExportToClipboardCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoExportToClipboard(o);
                    });
            }
        }

        public double Fx
        {
            get { return (double)GetValue(FxProperty); }
            set { SetValue(FxProperty, value); }
        }

        public double Fy
        {
            get { return (double)GetValue(FyProperty); }
            set { SetValue(FyProperty, value); }
        }

        public ObservableCollection<ChArUcoImageContainer> Images
        {
            get { return (ObservableCollection<ChArUcoImageContainer>)GetValue(ImagesProperty); }
            set { SetValue(ImagesProperty, value); }
        }

        public double K1
        {
            get { return (double)GetValue(K1Property); }
            set { SetValue(K1Property, value); }
        }

        public double K2
        {
            get { return (double)GetValue(K2Property); }
            set { SetValue(K2Property, value); }
        }

        public double K3
        {
            get { return (double)GetValue(K3Property); }
            set { SetValue(K3Property, value); }
        }

        public double K4
        {
            get { return (double)GetValue(K4Property); }
            set { SetValue(K4Property, value); }
        }

        public RelayCommand<object> LoadFromFileCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoLoadFromFile(o);
                    });
            }
        }

        public Visibility ResultControlVisibility
        {
            get { return (Visibility)GetValue(ResultControlVisibilityProperty); }
            set { SetValue(ResultControlVisibilityProperty, value); }
        }

        public RelayCommand<object> StartCalibrationAprilGridCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoStartCalibrationAprilGrid(o);
                    });
            }
        }

        public RelayCommand<object> StartCalibrationChArucoCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoStartCalibrationChAruco(o);
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

        public RelayCommand<object> UndistortImageCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoUndistortImage(o);
                    });
            }
        }

        public RelayCommand<object> ValidateCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoValidate(o);
                    });
            }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();

            bool changed = false;
            changed |= Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fx != Fx;
            changed |= Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fy != Fy;
            changed |= Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cx != Cx;
            changed |= Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cy != Cy;
            changed |= Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Alpha != Alpha;

            var firstNotSecond = _DistCoeffs.Except(Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs).ToList();
            var secondNotFirst = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs.Except(_DistCoeffs).ToList();

            changed |= firstNotSecond.Any() || secondNotFirst.Any();

            if (changed)
            {
                Fx = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fx;
                Fy = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fy;
                Cx = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cx;
                Cy = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cy;
                Alpha = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Alpha;

                _DistCoeffs = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs.ToList();

                if (_DistCoeffs.Count == 4)
                {
                    K1 = _DistCoeffs[0];
                    K2 = _DistCoeffs[1];
                    K3 = _DistCoeffs[2];
                    K4 = _DistCoeffs[3];
                }

                Mat board = ChArUcoCalibration.DrawBoard(5, 5, 0.04f, 0.02f, new System.Drawing.Size(Parent.CameraViewModel.ImageWidth, Parent.CameraViewModel.ImageHeight), 10, PredefinedDictionaryName.Dict6X6_250);
                Mat boardDist = board.Clone();

                Mat cameraMatrix = new Mat(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1);
                Mat distCoeffs = new Mat(1, Parent.CameraViewModel.FishEyeCalibration ? 4 : _DistCoeffs.Count, Emgu.CV.CvEnum.DepthType.Cv64F, 1);

                cameraMatrix.SetValue(0, 0, Fx);
                cameraMatrix.SetValue(1, 1, Fy);
                cameraMatrix.SetValue(0, 1, Fx * Alpha);
                cameraMatrix.SetValue(0, 2, Cx);
                cameraMatrix.SetValue(1, 2, Cy);
                cameraMatrix.SetValue(2, 2, 1.0f);

                Mat newK = new Mat(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1);

                for (int i = 0; i < distCoeffs.Cols && (Parent.CameraViewModel.FishEyeCalibration ? i < 4 : true); i++)
                {
                    distCoeffs.SetValue(0, i, _DistCoeffs[i]);
                }

                if (Parent.CameraViewModel.FishEyeCalibration)
                {
                    Fisheye.EstimateNewCameraMatrixForUndistorRectify(cameraMatrix, distCoeffs, new System.Drawing.Size(Parent.CameraViewModel.ImageWidth, Parent.CameraViewModel.ImageHeight), Mat.Eye(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1), newK, 0, new System.Drawing.Size(Parent.CameraViewModel.ImageWidth, Parent.CameraViewModel.ImageHeight), 0.3);
                    Mat map1 = new Mat();
                    Mat map2 = new Mat();
                    Fisheye.InitUndistorRectifyMap(cameraMatrix, distCoeffs, Mat.Eye(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1), newK, new System.Drawing.Size(Parent.CameraViewModel.ImageWidth, Parent.CameraViewModel.ImageHeight), Emgu.CV.CvEnum.DepthType.Cv32F, map1, map2);
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

        private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Parent.SyncContext.Post(o =>
            {
                if (AutoSnapshot)
                {
                    int squaresX = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresX;
                    int squaresY = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresY;
                    float squareLength = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquareLength;
                    float markerLength = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.MarkerLength;
                    PredefinedDictionaryName dictionary = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.Dictionary;
                    ChArUcoImageContainer cauic = new ChArUcoImageContainer(squaresX, squaresY, squareLength, markerLength, dictionary);
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
                VectorOfInt charucoCounterPerFrame = new VectorOfInt();
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

                    squaresX = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresX;
                    squaresY = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresY;
                    squareLength = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquareLength;
                    markerLength = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.MarkerLength;
                    size = Parent.CameraViewModel.Image.CvImage.Size;
                    dictionary = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.Dictionary;
                    foreach (ChArUcoImageContainer image in Images)
                    {
                        if (image.MarkerCorners != null && image.CharucoCorners.Size > 4)
                        {
                            allIds.Push(image.MarkerIds);
                            allCorners.Push(image.MarkerCorners);
                            allCharucoIds.Push(image.CharucoIds);
                            allCharucoCorners.Push(image.CharucoCorners);
                            markerCounterPerFrame.Push(new int[] { image.MarkerCorners.Size });
                            charucoCounterPerFrame.Push(new int[] { image.CharucoCorners.Size });
                        }
                    }
                }, null);

                if (markerCounterPerFrame.Size > 0)
                {
                    MetroDialogSettings settings = new MetroDialogSettings()
                    {
                        AnimateShow = true,
                        AnimateHide = true
                    };

                    var controller = await Parent.DialogCoordinator.ShowProgressAsync(Parent, "Please wait...", "Calculating calibration parameter now!", settings: settings);
                    controller.SetIndeterminate();
                    controller.SetCancelable(false);

                    bool error = false;
                    (Mat cameraMatrix, Mat distCoeffs, double rms) result = (null, null, 0.0);
                    try
                    {
                        result = ChArUcoCalibration.CalibrateCharuco(squaresX, squaresY, squareLength, markerLength, dictionary, size, allCharucoIds, allCharucoCorners, charucoCounterPerFrame, fisheye, delegate (byte[] input)
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
                        var con = await Parent.DialogCoordinator.ShowMessageAsync(Parent, "Result", string.Format("RMS: {0}\nDo you want to save?", result.rms), MessageDialogStyle.AffirmativeAndNegative, null);
                        if (con == MessageDialogResult.Affirmative)
                        {
                            Parent.SyncContext.Post(async c =>
                            {
                                Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fx = result.cameraMatrix.GetValue(0, 0);
                                Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fy = result.cameraMatrix.GetValue(1, 1);
                                Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cx = result.cameraMatrix.GetValue(0, 2);
                                Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cy = result.cameraMatrix.GetValue(1, 2);
                                Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Alpha = result.cameraMatrix.GetValue(0, 1) / result.cameraMatrix.GetValue(0, 0); ;

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

        private Task DoClear(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    Images.Clear();
                }, null);
            });
        }

        private Task DoClose(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    TakeSnapshotControlVisibility = Visibility.Collapsed;
                    ResultControlVisibility = Visibility.Visible;
                }, null);
            });
        }

        private Task DoDelete(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    if (o != null && o is ChArUcoImageContainer)
                        Images.Remove(o as ChArUcoImageContainer);
                }, null);
            });
        }

        private Task DoExportToClipboard(object o)
        {
            return Task.Run(() =>
            {
                Parent.SyncContext.Send(f =>
                {
                    Clipboard.SetText(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t", Fx, Fy, Cx, Cy, K1, K2, K3, K4));
                }, null);
            });
        }

        private Task DoLoadFromFile(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    CustomDialog customDialog = new CustomDialog() { Title = "Select calibration file" };

                    var dataContext = new ReplaySelectDialogModel(obj =>
                    {
                        Parent.SyncContext.Post(d =>
                        {
                            Task.Factory.StartNew(async () =>
                            {
                                ReplaySelectDialogModel replaySelectDialogModel = obj as ReplaySelectDialogModel;
                                if (replaySelectDialogModel.SelectedFile != null)
                                {
                                    MetroDialogSettings settings = new MetroDialogSettings()
                                    {
                                        AnimateShow = false,
                                        AnimateHide = false
                                    };
                                    var controller = await Parent.DialogCoordinator.ShowProgressAsync(Parent, "Please wait...", "Loading calibration images!", settings: Parent.MetroDialogSettings);
                                    controller.SetCancelable(false);
                                    controller.SetIndeterminate();

                                    string file = null;
                                    string outputPath = null;

                                    Parent.SyncContext.Send(async d2 =>
                                    {
                                        file = replaySelectDialogModel.SelectedFile.FullPath;
                                        outputPath = Path.Combine(Path.GetTempPath(), "firefly", Guid.NewGuid().ToString());
                                    }, null);

                                    RawDataReader reader = new RawDataReader(file, RawReaderMode.Camera0);
                                    reader.Open();
                                    MemoryImageExporter exporter = new MemoryImageExporter();
                                    exporter.AddFromReader(reader, delegate (double percent)
                                    {
                                        double value = percent;
                                        value = value > 1 ? 1 : value;
                                        controller.SetProgress(value);
                                    });

                                    reader.Close();

                                    Parent.SyncContext.Post(d2 =>
                                    {
                                        int i = 0;
                                        int count = exporter.Images.Count;
                                        foreach (Mat m in exporter.Images)
                                        {
                                            //double value = 0.5 + i / count * 0.5;
                                            //value = value > 1 ? 1 : value;
                                            //controller.SetProgress(value);

                                            if (i++ % 20 == 0)
                                            {
                                                int squaresX = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresX;
                                                int squaresY = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresY;
                                                float squareLength = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquareLength;
                                                float markerLength = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.MarkerLength;
                                                PredefinedDictionaryName dictionary = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.Dictionary;

                                                ChArUcoImageContainer cauic = new ChArUcoImageContainer(squaresX, squaresY, squareLength, markerLength, dictionary);
                                                CvImageContainer container = new CvImageContainer();
                                                container.CvImage = m;
                                                cauic.OriginalImage = container;
                                                Images.Add(cauic);
                                            }
                                        }

                                        controller.CloseAsync();
                                    }, null);
                                }
                            }, TaskCreationOptions.LongRunning);
                            Parent.DialogCoordinator.HideMetroDialogAsync(Parent, customDialog);
                        }, null);
                    });
                    Parent.ReplayViewModel.Refresh();
                    dataContext.FilesForReplay.AddRange(Parent.ReplayViewModel.FilesForReplay.Where(x => !x.Item1.IsRemote));
                    customDialog.Content = new ReplaySelectDialog { DataContext = dataContext };

                    Parent.DialogCoordinator.ShowMetroDialogAsync(Parent, customDialog);
                }, null);
            });
        }

        private Task DoStartCalibrationAprilGrid(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(f =>
                {
                    CustomDialog customDialog;

                    customDialog = new CustomDialog() { Title = "Select extrinsic calibration file" };

                    var dataContext = new ReplaySelectDialogModel(OnReplaySelectDialogClose(customDialog));
                    Parent.ReplayViewModel.Refresh();
                    dataContext.FilesForReplay.AddRange(Parent.ReplayViewModel.FilesForReplay.Where(x => !x.Item1.IsRemote));
                    customDialog.Content = new ReplaySelectDialog
                    {
                        DataContext = dataContext
                    };

                    Parent.DialogCoordinator.ShowMetroDialogAsync(Parent, customDialog);
                }, null);
            });
        }

        private Task DoStartCalibrationChAruco(object o)
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
                    int squaresX = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresX;
                    int squaresY = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresY;
                    float squareLength = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquareLength;
                    float markerLength = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.MarkerLength;
                    PredefinedDictionaryName dictionary = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.Dictionary;

                    ChArUcoImageContainer cauic = new ChArUcoImageContainer(squaresX, squaresY, squareLength, markerLength, dictionary);
                    cauic.OriginalImage = Parent.CameraViewModel.Image;
                    Images.Add(cauic);
                }, null);
            });
        }

        private Task DoUndistortImage(object o)
        {
            return Task.Factory.StartNew(async () =>
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = null;
                System.Windows.Forms.SaveFileDialog saveFileDialog = null;
                bool open = false;
                bool save = false;

                CameraViewModel cvm = null;

                Parent.SyncContext.Send(c =>
                {
                    cvm = Parent.CameraViewModel;
                    openFileDialog = new System.Windows.Forms.OpenFileDialog();
                    openFileDialog.Filter = "Image (*.png) | *.png";
                    open = openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;

                    saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                    saveFileDialog.Filter = "Image (*.png) | *.png";
                    save = saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;
                }, null);

                if (open && save)
                {
                    MetroDialogSettings settings = new MetroDialogSettings()
                    {
                        AnimateShow = false,
                        AnimateHide = false
                    };

                    var controller = await Parent.DialogCoordinator.ShowProgressAsync(Parent, "Please wait...", "Export undistort image!", settings: Parent.MetroDialogSettings);

                    controller.SetCancelable(false);

                    Mat rawImage = CvInvoke.Imread(openFileDialog.FileName, Emgu.CV.CvEnum.ImreadModes.Grayscale);
                    Mat rawImageUndist = new Mat();

                    Mat map1 = new Mat();
                    Mat map2 = new Mat();

                    Fisheye.InitUndistorRectifyMap(cvm.OrginalCameraMatrix, cvm.DistortionCoefficients, Mat.Eye(3, 3, DepthType.Cv64F, 1), cvm.CenteredCameraMatrix, new System.Drawing.Size(512, 512), DepthType.Cv32F, map1, map2);

                    CvInvoke.Remap(rawImage, rawImageUndist, map1, map2, Inter.Linear, BorderType.Constant);

                    CvInvoke.Imwrite(saveFileDialog.FileName, rawImageUndist);

                    await controller.CloseAsync();
                }
            });
        }

        private Task DoValidate(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(async c =>
                {
                    VectorOfInt allIds = new VectorOfInt();
                    VectorOfVectorOfPointF allCorners = new VectorOfVectorOfPointF();
                    VectorOfInt allCharucoIds = new VectorOfInt();
                    VectorOfPointF allCharucoCorners = new VectorOfPointF();
                    VectorOfInt markerCounterPerFrame = new VectorOfInt();
                    VectorOfInt charucoCounterPerFrame = new VectorOfInt();
                    int squaresX = 0;
                    int squaresY = 0;
                    float squareLength = 0f;
                    float markerLength = 0f;
                    PredefinedDictionaryName dictionary = PredefinedDictionaryName.Dict4X4_50;
                    System.Drawing.Size size = new System.Drawing.Size();

                    bool fisheye = false;

                    Parent.SyncContext.Send(async d =>
                    {
                        fisheye = Parent.CameraViewModel.FishEyeCalibration;

                        squaresX = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresX;
                        squaresY = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresY;
                        squareLength = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquareLength;
                        markerLength = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.MarkerLength;
                        dictionary = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.Dictionary;
                        size = new System.Drawing.Size(Parent.CameraViewModel.ImageWidth, Parent.CameraViewModel.ImageHeight);

                        foreach (ChArUcoImageContainer image in Images)
                        {
                            if (image.MarkerCorners != null && image.CharucoCorners.Size > 4)
                            {
                                allIds.Push(image.MarkerIds);
                                allCorners.Push(image.MarkerCorners);
                                allCharucoIds.Push(image.CharucoIds);
                                allCharucoCorners.Push(image.CharucoCorners);
                                markerCounterPerFrame.Push(new int[] { image.MarkerCorners.Size });
                                charucoCounterPerFrame.Push(new int[] { image.CharucoCorners.Size });
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

                        var controller = await Parent.DialogCoordinator.ShowProgressAsync(Parent, "Please wait...", "Validate parameter now!", settings: Parent.MetroDialogSettings);
                        controller.SetIndeterminate();
                        controller.SetCancelable(false);

                        bool error = false;
                        double rms = 0.0;
                        try
                        {
                            Mat cameraMatrix = new Mat(3, 3, Emgu.CV.CvEnum.DepthType.Cv64F, 1);
                            Mat distCoeffs = new Mat(1, Parent.CameraViewModel.FishEyeCalibration ? 4 : _DistCoeffs.Count, Emgu.CV.CvEnum.DepthType.Cv64F, 1);

                            cameraMatrix.SetValue(0, 0, Fx);
                            cameraMatrix.SetValue(1, 1, Fy);
                            cameraMatrix.SetValue(0, 1, Fx * Alpha);
                            cameraMatrix.SetValue(0, 2, Cx);
                            cameraMatrix.SetValue(1, 2, Cy);
                            cameraMatrix.SetValue(2, 2, 1.0f);

                            for (int i = 0; i < distCoeffs.Cols && (Parent.CameraViewModel.FishEyeCalibration ? i < 4 : true); i++)
                            {
                                distCoeffs.SetValue(0, i, _DistCoeffs[i]);
                            }

                            rms = ChArUcoCalibration.ValidateCharuco(squaresX, squaresY, squareLength, markerLength, dictionary, size, allCharucoIds, allCharucoCorners, charucoCounterPerFrame, fisheye, delegate (byte[] input)
                            {
                                return Parent.IOProxy.GetRemoteChessboardCorner(input);
                            }, cameraMatrix, distCoeffs);
                        }
                        catch (Exception ex)
                        {
                            error = true;
                        }

                        await controller.CloseAsync();
                        if (!error)
                        {
                            var con = await Parent.DialogCoordinator.ShowMessageAsync(Parent, "Result", string.Format("RMS: {0}", rms), MessageDialogStyle.Affirmative, null);
                        }
                        else
                        {
                            await Parent.DialogCoordinator.ShowMessageAsync(Parent, "Error", "Error during validation!");
                        }
                    }
                    else
                    {
                        await Parent.DialogCoordinator.ShowMessageAsync(Parent, "Error", "Not enough valide input frames available!");
                    }
                }, null);
            });
        }

        private Action<ReplaySelectDialogModel> OnReplaySelectDialogClose(CustomDialog customDialog)
        {
            return obj =>
            {
                Parent.SyncContext.Send(d =>
                {
                    Parent.DialogCoordinator.HideMetroDialogAsync(Parent, customDialog);
                }, null);

                Parent.SyncContext.Post(d =>
                {
                    Task.Factory.StartNew(async () =>
                    {
                        ReplaySelectDialogModel replaySelectDialogModel = obj as ReplaySelectDialogModel;
                        if (replaySelectDialogModel.SelectedFile != null)
                        {
                            RemoteDataStore remoteDataStore = new RemoteDataStore(Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.IpAddress, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Username, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Password);

                            string guid = Guid.NewGuid().ToString();
                            string localFile = "";
                            string remoteFile = "";
                            string remoteFolder = string.Format(@"/var/tmp/firefly/{0}", guid);
                            string expactString = string.Format("{0}@{1}:.{{0,}}[$]", Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Username, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Hostname);

                            Parent.SyncContext.Send(c =>
                            {
                                localFile = replaySelectDialogModel.SelectedFile.FullPath;
                                remoteFile = string.Format(@"/var/tmp/firefly/{0}/{1}", guid, Path.GetFileName(localFile));
                            }, null);

                            var controller = await Parent.DialogCoordinator.ShowProgressAsync(Parent, "Please wait...", "Calculating calibration parameter now!", settings: Parent.MetroDialogSettings);
                            controller.SetIndeterminate();
                            controller.SetCancelable(false);

                            remoteDataStore.ExecuteCommands(new List<string>() { string.Format("mkdir -p {0}", remoteFolder) }, expactString);

                            remoteDataStore.UploadFile(remoteFile, localFile);

                            remoteDataStore.UploadContentToFile(string.Format(@"{0}/target.yaml", remoteFolder), YamlTranslator.ConvertToYaml(new CalibrationTarget()
                            {
                                TargetType = CalibrationTargetType.Aprilgrid,
                                TagSize = Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagSize,
                                TagSpacing = Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagSpacingFactor,
                                TagCols = Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagsX,
                                TagRows = Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagsY
                            }));

                            remoteDataStore.ExecuteCommands(new List<string>()
                            {
                                string.Format(@"cd {0}",remoteFolder),
                                string.Format(@"unzip {0} -d {1}",Path.GetFileName(localFile),Path.GetFileNameWithoutExtension(localFile)),
                                @"source ~/kalibr_workspace/devel/setup.bash",
                                string.Format(@"kalibr_bagcreater --folder {0} --output-bag {0}.bag", Path.GetFileNameWithoutExtension(localFile)),
                                string.Format(@"kalibr_calibrate_cameras --bag {0}.bag --topics /cam0/image_raw --models pinhole-equi --target target.yaml --dont-show-report",Path.GetFileNameWithoutExtension(localFile)),
                                string.Format("pdftoppm report-cam-{0}.pdf result -png",Path.GetFileNameWithoutExtension(localFile))
                            }, expactString);

                            CameraChain cameraChain = YamlTranslator.ConvertFromYaml<CameraChain>(remoteDataStore.DownloadFileToMemory(string.Format("{0}/camchain-{1}.yaml", remoteFolder, Path.GetFileNameWithoutExtension(localFile))));

                            string outputPath = Path.Combine(Path.GetTempPath(), "firefly", guid);

                            if (!Directory.Exists(outputPath))
                            {
                                Directory.CreateDirectory(outputPath);
                            }

                            foreach (string file in remoteDataStore.GetAllFileNames(remoteFolder))
                            {
                                if (!file.Contains(".bag") && !file.Contains(".ffc"))
                                    remoteDataStore.DownloadFile(string.Format("{0}/{1}", remoteFolder, file), Path.Combine(outputPath, file));
                            }

                            ShowResults(outputPath, cameraChain);

                            remoteDataStore.ExecuteCommands(new List<string>()
                                    {
                                        string.Format(@"rm -r {0}",remoteFolder)
                                     }, expactString);

                            await controller.CloseAsync();
                        }
                    });
                }, null);
            };
        }

        private Action<ImageResultViewerDialogModel> OnShowResultDialogClose(CustomDialog customDialog, string path)
        {
            return obj =>
            {
                Parent.SyncContext.Send(d =>
                {
                    Parent.DialogCoordinator.HideMetroDialogAsync(Parent, customDialog);
                    Directory.Delete(path, true);
                }, null);
            };
        }

        private Action<ImageResultViewerDialogModel> OnShowResultDialogExport(CustomDialog customDialog, string path)
        {
            return obj =>
            {
                Parent.SyncContext.Send(d =>
                {
                    System.Windows.Forms.SaveFileDialog saveFileDialog = saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                    saveFileDialog.Filter = "Archive (*.zip) | *.zip";
                    if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        ZipFile.CreateFromDirectory(path, saveFileDialog.FileName, CompressionLevel.Optimal, false);
                    }
                }, null);
            };
        }

        private Action<ImageResultViewerDialogModel> OnShowResultDialogSave(CustomDialog customDialog, CameraChain cameraChain)
        {
            return obj =>
            {
                Parent.SyncContext.Send(d =>
                {
                    Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cx = cameraChain.Cam0.Cx;
                    Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cy = cameraChain.Cam0.Cy;
                    Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fx = cameraChain.Cam0.Fx;
                    Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fy = cameraChain.Cam0.Fy;

                    Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs = cameraChain.Cam0.DistortionCoefficients.ToList();

                    Parent.UpdateSettings(false);
                }, null);
            };
        }

        private void ShowResults(string path, CameraChain cameraChain)
        {
            Parent.SyncContext.Send(f =>
            {
                CustomDialog customDialog;

                customDialog = new CustomDialog() { Title = "Calibration results" };

                var dataContext = new ImageResultViewerDialogModel(OnShowResultDialogClose(customDialog, path), OnShowResultDialogSave(customDialog, cameraChain), OnShowResultDialogExport(customDialog, path), path, "result-*.png");
                Parent.ReplayViewModel.Refresh();
                customDialog.Content = new ImageResultViewerDialog
                {
                    DataContext = dataContext
                };

                Parent.DialogCoordinator.ShowMetroDialogAsync(Parent, customDialog);
            }, null);
        }
    }
}