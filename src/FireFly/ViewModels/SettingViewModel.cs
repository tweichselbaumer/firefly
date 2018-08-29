using FireFly.Command;
using FireFly.CustomDialogs;
using FireFly.Settings;
using FireFly.Utilities;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FireFly.ViewModels
{
    public class SettingViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty AccelerometerScaleProperty =
            DependencyProperty.Register("AccelerometerScale", typeof(double), typeof(SettingViewModel), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty FileLocationsProperty =
            DependencyProperty.Register("FileLocations", typeof(RangeObservableCollection<FileLocation>), typeof(SettingViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty GyroscopeScaleProperty =
            DependencyProperty.Register("GyroscopeScale", typeof(double), typeof(SettingViewModel), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty IpAddressProperty =
            DependencyProperty.Register("IpAddress", typeof(string), typeof(SettingViewModel), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(SettingViewModel), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(int), typeof(SettingViewModel), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnPropertyChanged)));

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

        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();

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

        private void FileLocations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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