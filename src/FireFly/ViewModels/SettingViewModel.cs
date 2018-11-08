using Emgu.CV;
using FireFly.Command;
using FireFly.CustomDialogs;
using FireFly.Data.Storage;
using FireFly.Settings;
using FireFly.Utilities;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static Emgu.CV.Aruco.Dictionary;

namespace FireFly.ViewModels
{
    public class SettingViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty AccelerometerScaleProperty =
            DependencyProperty.Register("AccelerometerScale", typeof(double), typeof(SettingViewModel), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty DictionaryProperty =
            DependencyProperty.Register("Dictionary", typeof(PredefinedDictionaryName), typeof(SettingViewModel), new FrameworkPropertyMetadata(PredefinedDictionaryName.Dict4X4_50, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty FileLocationsProperty =
            DependencyProperty.Register("FileLocations", typeof(RangeObservableCollection<FileLocation>), typeof(SettingViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty GyroscopeScaleProperty =
            DependencyProperty.Register("GyroscopeScale", typeof(double), typeof(SettingViewModel), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty IpAddressProperty =
            DependencyProperty.Register("IpAddress", typeof(string), typeof(SettingViewModel), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty MarkerLengthProperty =
           DependencyProperty.Register("MarkerLength", typeof(float), typeof(SettingViewModel), new FrameworkPropertyMetadata(0.0f, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(SettingViewModel), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(int), typeof(SettingViewModel), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty SquareLengthProperty =
            DependencyProperty.Register("SquareLength", typeof(float), typeof(SettingViewModel), new FrameworkPropertyMetadata(0.0f, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty SquaresXProperty =
            DependencyProperty.Register("SquaresX", typeof(int), typeof(SettingViewModel), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty SquaresYProperty =
            DependencyProperty.Register("SquaresY", typeof(int), typeof(SettingViewModel), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty TagSizeProperty =
            DependencyProperty.Register("TagSize", typeof(double), typeof(SettingViewModel), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty TagSpacingFactorProperty =
            DependencyProperty.Register("TagSpacingFactor", typeof(double), typeof(SettingViewModel), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty TagsXProperty =
            DependencyProperty.Register("TagsX", typeof(int), typeof(SettingViewModel), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty TagsYProperty =
            DependencyProperty.Register("TagsY", typeof(int), typeof(SettingViewModel), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty TemperatureOffsetProperty =
            DependencyProperty.Register("TemperatureOffset", typeof(double), typeof(SettingViewModel), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty TemperatureScaleProperty =
            DependencyProperty.Register("TemperatureScale", typeof(double), typeof(SettingViewModel), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(SettingViewModel), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));

        public SettingViewModel(MainViewModel parent) : base(parent)
        {
            FileLocations = new RangeObservableCollection<FileLocation>();
            FileLocations.CollectionChanged += FileLocations_CollectionChanged;
        }

        public double AccelerometerScale
        {
            get { return (double)GetValue(AccelerometerScaleProperty); }
            set { SetValue(AccelerometerScaleProperty, value); }
        }

        public RelayCommand<object> DeleteFileLocationCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoDeleteFileLocation(o);
                    });
            }
        }

        public PredefinedDictionaryName Dictionary
        {
            get { return (PredefinedDictionaryName)GetValue(DictionaryProperty); }
            set { SetValue(DictionaryProperty, value); }
        }

        public RangeObservableCollection<FileLocation> FileLocations
        {
            get { return (RangeObservableCollection<FileLocation>)GetValue(FileLocationsProperty); }
            set { SetValue(FileLocationsProperty, value); }
        }

        public double GyroscopeScale
        {
            get { return (double)GetValue(GyroscopeScaleProperty); }
            set { SetValue(GyroscopeScaleProperty, value); }
        }

        public string IpAddress
        {
            get { return (string)GetValue(IpAddressProperty); }
            set { SetValue(IpAddressProperty, value); }
        }

        public float MarkerLength
        {
            get { return (float)GetValue(MarkerLengthProperty); }
            set { SetValue(MarkerLengthProperty, value); }
        }

        public RelayCommand<object> NewFileLocationCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoNewFileLocation(o);
                    });
            }
        }

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public int Port
        {
            get { return (int)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
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

        public double TagSize
        {
            get { return (double)GetValue(TagSizeProperty); }
            set { SetValue(TagSizeProperty, value); }
        }

        public double TagSpacingFactor
        {
            get { return (double)GetValue(TagSpacingFactorProperty); }
            set { SetValue(TagSpacingFactorProperty, value); }
        }

        public int TagsX
        {
            get { return (int)GetValue(TagsXProperty); }
            set { SetValue(TagsXProperty, value); }
        }

        public int TagsY
        {
            get { return (int)GetValue(TagsYProperty); }
            set { SetValue(TagsYProperty, value); }
        }

        public double TemperatureOffset
        {
            get { return (double)GetValue(TemperatureOffsetProperty); }
            set { SetValue(TemperatureOffsetProperty, value); }
        }

        public double TemperatureScale
        {
            get { return (double)GetValue(TemperatureScaleProperty); }
            set { SetValue(TemperatureScaleProperty, value); }
        }

        public RelayCommand<object> UpdateSettingsRemoteCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoUpdateSettingsRemote(o);
                    });
            }
        }

        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();

            SquaresX = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresX;
            SquaresY = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresY;
            MarkerLength = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.MarkerLength;
            SquareLength = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquareLength;
            Dictionary = Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.Dictionary;

            TagSize = Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagSize;
            TagSpacingFactor = Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagSpacingFactor;
            TagsX = Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagsX;
            TagsY = Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagsY;

            IpAddress = Parent.SettingContainer.Settings.ConnectionSettings.IpAddress;
            Port = Parent.SettingContainer.Settings.ConnectionSettings.Port;

            Password = Parent.SettingContainer.Settings.ConnectionSettings.Password;
            Username = Parent.SettingContainer.Settings.ConnectionSettings.Username;

            AccelerometerScale = Parent.SettingContainer.Settings.ImuSettings.AccelerometerScale;
            GyroscopeScale = Parent.SettingContainer.Settings.ImuSettings.GyroscopeScale;
            TemperatureScale = Parent.SettingContainer.Settings.ImuSettings.TemperatureScale;
            TemperatureOffset = Parent.SettingContainer.Settings.ImuSettings.TemperatureOffset;

            var firstNotSecond = FileLocations.Except(Parent.SettingContainer.Settings.GeneralSettings.FileLocations).ToList();
            var secondNotFirst = Parent.SettingContainer.Settings.GeneralSettings.FileLocations.Except(FileLocations).ToList();

            bool changed = firstNotSecond.Any() || secondNotFirst.Any();

            if (changed)
            {
                List<FileLocation> l = Parent.SettingContainer.Settings.GeneralSettings.FileLocations.ToList();
                FileLocations.Clear();
                FileLocations.AddRange(l);
            }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingViewModel svm = (d as SettingViewModel);
            bool changed = false;
            bool connectionSettingsChanged = false;
            switch (e.Property.Name)
            {
                case "Port":
                    changed = svm.Parent.SettingContainer.Settings.ConnectionSettings.Port != svm.Port;
                    svm.Parent.SettingContainer.Settings.ConnectionSettings.Port = svm.Port;
                    connectionSettingsChanged = true;
                    break;

                case "Username":
                    changed = svm.Parent.SettingContainer.Settings.ConnectionSettings.Username != svm.Username;
                    svm.Parent.SettingContainer.Settings.ConnectionSettings.Username = svm.Username;
                    connectionSettingsChanged = false;
                    break;

                case "Password":
                    changed = svm.Parent.SettingContainer.Settings.ConnectionSettings.Password != svm.Password;
                    svm.Parent.SettingContainer.Settings.ConnectionSettings.Password = svm.Password;
                    connectionSettingsChanged = false;
                    break;

                case "IpAddress":
                    changed = svm.Parent.SettingContainer.Settings.ConnectionSettings.IpAddress != svm.IpAddress;
                    svm.Parent.SettingContainer.Settings.ConnectionSettings.IpAddress = svm.IpAddress;
                    connectionSettingsChanged = true;
                    break;

                case "GyroscopeScale":
                    changed = svm.Parent.SettingContainer.Settings.ImuSettings.GyroscopeScale != svm.GyroscopeScale;
                    svm.Parent.SettingContainer.Settings.ImuSettings.GyroscopeScale = svm.GyroscopeScale;
                    connectionSettingsChanged = false;
                    break;

                case "AccelerometerScale":
                    changed = svm.Parent.SettingContainer.Settings.ImuSettings.AccelerometerScale != svm.AccelerometerScale;
                    svm.Parent.SettingContainer.Settings.ImuSettings.AccelerometerScale = svm.AccelerometerScale;
                    connectionSettingsChanged = false;
                    break;

                case "TemperatureOffset":
                    changed = svm.Parent.SettingContainer.Settings.ImuSettings.TemperatureOffset != svm.TemperatureOffset;
                    svm.Parent.SettingContainer.Settings.ImuSettings.TemperatureOffset = svm.TemperatureOffset;
                    connectionSettingsChanged = false;
                    break;

                case "TemperatureScale":
                    changed = svm.Parent.SettingContainer.Settings.ImuSettings.TemperatureScale != svm.TemperatureScale;
                    svm.Parent.SettingContainer.Settings.ImuSettings.TemperatureScale = svm.TemperatureScale;
                    connectionSettingsChanged = false;
                    break;

                case "SquaresX":
                    changed = svm.Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresX != svm.SquaresX;
                    svm.Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresX = svm.SquaresX;
                    break;

                case "SquaresY":
                    changed = svm.Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresY != svm.SquaresY;
                    svm.Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquaresY = svm.SquaresY;
                    break;

                case "SquareLength":
                    changed = svm.Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquareLength != svm.SquareLength;
                    svm.Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.SquareLength = svm.SquareLength;
                    break;

                case "MarkerLength":
                    changed = svm.Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.MarkerLength != svm.MarkerLength;
                    svm.Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.MarkerLength = svm.MarkerLength;
                    break;

                case "Dictionary":
                    changed = svm.Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.Dictionary != svm.Dictionary;
                    svm.Parent.SettingContainer.Settings.CalibrationSettings.ChArucoCalibrationSettings.Dictionary = svm.Dictionary;
                    break;

                case "TagSize":
                    changed = svm.Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagSize != svm.TagSize;
                    svm.Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagSize = svm.TagSize;
                    connectionSettingsChanged = false;
                    break;

                case "TagSpacingFactor":
                    changed = svm.Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagSpacingFactor != svm.TagSpacingFactor;
                    svm.Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagSpacingFactor = svm.TagSpacingFactor;
                    connectionSettingsChanged = false;
                    break;

                case "TagsY":
                    changed = svm.Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagsY != svm.TagsY;
                    svm.Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagsY = svm.TagsY;
                    connectionSettingsChanged = false;
                    break;

                case "TagsX":
                    changed = svm.Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagsX != svm.TagsX;
                    svm.Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagsX = svm.TagsX;
                    connectionSettingsChanged = false;
                    break;

                default:
                    break;
            }
            if (changed)
            {
                svm.Parent.UpdateSettings(connectionSettingsChanged);
            }
        }

        private Task DoDeleteFileLocation(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    FileLocations.Remove((FileLocation)o);
                }, null);
            });
        }

        private Task DoNewFileLocation(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    CustomDialog customDialog = new CustomDialog() { Title = "New Location" };

                    var dataContext = new NewFileLocationDialogModel(obj =>
                    {
                        Parent.DialogCoordinator.HideMetroDialogAsync(Parent, customDialog);
                        Parent.SyncContext.Post(d =>
                        {
                            FileLocation location = new FileLocation()
                            {
                                Name = obj.Name,
                                Path = obj.Path
                            };
                            FileLocations.Add(location);
                        }, null);
                    });
                    customDialog.Content = new NewFileLocationDialog { DataContext = dataContext };

                    Parent.DialogCoordinator.ShowMetroDialogAsync(Parent, customDialog);
                }, null);
            });
        }

        private Task DoPrintBoard(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    if (o is string && !string.IsNullOrEmpty(o as string))
                    {
                        switch (o as string)
                        {
                            case "AprilGrid":
                                {
                                    CustomDialog customDialog = new CustomDialog() { Title = "Board Properties" };

                                    var dataContext = new PrintAprilGridDialogModel(obj =>
                                    {
                                        Parent.SyncContext.Post(d =>
                                        {
                                            Parent.DialogCoordinator.HideMetroDialogAsync(Parent, customDialog);

                                            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                                            saveFileDialog.Filter = "Portable Document Format (*.pdf) | *.pdf";

                                            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                            {
                                                RemoteDataStore remoteDataStore = new RemoteDataStore(Parent.SettingContainer.Settings.ConnectionSettings.IpAddress, Parent.SettingContainer.Settings.ConnectionSettings.Username, Parent.SettingContainer.Settings.ConnectionSettings.Password);

                                                string expactString = string.Format("{0}@{1}:.{{0,}}[$]", Parent.SettingContainer.Settings.ConnectionSettings.Username, Parent.SettingContainer.Settings.ConnectionSettings.Hostname);
                                                string guid = Guid.NewGuid().ToString();
                                                string remoteFolder = string.Format(@"/var/tmp/firefly/{0}", guid);

                                                remoteDataStore.ExecuteCommands(new List<string>() { string.Format("mkdir -p {0}", remoteFolder) }, expactString);

                                                string res = remoteDataStore.ExecuteCommands(new List<string>()
                                                {
                                                    string.Format(@"cd {0}",remoteFolder),
                                                    @"source ~/kalibr_workspace/devel/setup.bash",
                                                    string.Format(CultureInfo.InvariantCulture, @"kalibr_create_target_pdf --type apriltag --nx {2} --ny {3} --tsize {0} --tspace {1} {4}/{5}", obj.TagSize,obj.TagSpacingFactor,obj.TagsX, obj.TagsY, remoteFolder, Path.GetFileName(saveFileDialog.FileName)),
                                                }, expactString);

                                                remoteDataStore.DownloadFile(string.Format("{0}/{1}", remoteFolder, Path.GetFileName(saveFileDialog.FileName)), saveFileDialog.FileName);

                                                remoteDataStore.ExecuteCommands(new List<string>()
                                                {
                                                    string.Format(@"rm -r {0}",remoteFolder)
                                                }, expactString);
                                            }
                                        }, null);
                                    },
                                    obj =>
                                    {
                                        Parent.SyncContext.Post(d =>
                                        {
                                            Parent.DialogCoordinator.HideMetroDialogAsync(Parent, customDialog);
                                        }, null);
                                    });

                                    dataContext.TagSize = TagSize;
                                    dataContext.TagSpacingFactor = TagSpacingFactor;
                                    dataContext.TagsX = TagsX;
                                    dataContext.TagsY = TagsY;

                                    customDialog.Content = new PrintAprilGridBoardDialog { DataContext = dataContext };

                                    Parent.DialogCoordinator.ShowMetroDialogAsync(Parent, customDialog);
                                }
                                break;

                            case "ChAruco":
                                {
                                    CustomDialog customDialog = new CustomDialog() { Title = "Board Properties" };

                                    var dataContext = new PrintCharucoBoardDialogModel(obj =>
                                    {
                                        Parent.SyncContext.Post(d =>
                                        {
                                            Parent.DialogCoordinator.HideMetroDialogAsync(Parent, customDialog);

                                            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                                            saveFileDialog.Filter = "Portable Network Graphics (*.png) | *.png";

                                            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                                CvInvoke.Imwrite(saveFileDialog.FileName, obj.Image.CvImage, new KeyValuePair<Emgu.CV.CvEnum.ImwriteFlags, int>() { });
                                        }, null);
                                    },
                                    obj =>
                                    {
                                        Parent.SyncContext.Post(d =>
                                        {
                                            Parent.DialogCoordinator.HideMetroDialogAsync(Parent, customDialog);
                                        }, null);
                                    });

                                    dataContext.SquareLength = (int)Math.Round(SquareLength / 0.0254 * 100);
                                    dataContext.MarkerLength = (int)Math.Round(MarkerLength / 0.0254 * 100);
                                    dataContext.Dictionary = Dictionary;
                                    dataContext.SquaresX = SquaresX;
                                    dataContext.SquaresY = SquaresY;

                                    customDialog.Content = new PrintCharucoBoardDialog { DataContext = dataContext };

                                    Parent.DialogCoordinator.ShowMetroDialogAsync(Parent, customDialog);
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }, null);
            });
        }

        private Task DoUpdateSettingsRemote(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    double fxO = Parent.CameraViewModel.OrginalCameraMatrix.GetValue(0, 0);
                    double fyO = Parent.CameraViewModel.OrginalCameraMatrix.GetValue(1, 1);
                    double cxO = Parent.CameraViewModel.OrginalCameraMatrix.GetValue(0, 2);
                    double cyO = Parent.CameraViewModel.OrginalCameraMatrix.GetValue(1, 2);

                    double fxN = Parent.CameraViewModel.NewCameraMatrix.GetValue(0, 0);
                    double fyN = Parent.CameraViewModel.NewCameraMatrix.GetValue(1, 1);
                    double cxN = Parent.CameraViewModel.NewCameraMatrix.GetValue(0, 2);
                    double cyN = Parent.CameraViewModel.NewCameraMatrix.GetValue(1, 2);

                    double k1 = Parent.CameraViewModel.DistortionCoefficients.GetValue(0, 0);
                    double k2 = Parent.CameraViewModel.DistortionCoefficients.GetValue(0, 1);
                    double k3 = Parent.CameraViewModel.DistortionCoefficients.GetValue(0, 2);
                    double k4 = Parent.CameraViewModel.DistortionCoefficients.GetValue(0, 3);

                    List<double> responseValues = new List<double>();

                    responseValues.AddRange(Parent.CalibrationViewModel.PhotometricCalibrationViewModel.ResponseValues.Select(f => f.Y));

                    byte[] vignette = Parent.CalibrationViewModel.PhotometricCalibrationViewModel.Vignette.CvImage.ToPNGBinary(0);

                    int width = Parent.CameraViewModel.ImageWidth;
                    int height = Parent.CameraViewModel.ImageHeight;

                    PhotometricCalibratrionExporter.ExporterSettings(fxO, fyO, cxO, cyO, fxN, fyN, cxN, cyN, width, height, k1, k2, k3, k4, @"C:\Users\thoma\source\repos\computer-vision\bin", responseValues, vignette);
                }, null);
            });
        }

        private void FileLocations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
            {
                var firstNotSecond = FileLocations.Except(Parent.SettingContainer.Settings.GeneralSettings.FileLocations).ToList();
                var secondNotFirst = Parent.SettingContainer.Settings.GeneralSettings.FileLocations.Except(FileLocations).ToList();

                bool changed = firstNotSecond.Any() || secondNotFirst.Any();

                if (changed)
                {
                    Parent.SettingContainer.Settings.GeneralSettings.FileLocations = FileLocations.ToList();
                    Parent.UpdateSettings(false);
                }
            }
        }
    }
}