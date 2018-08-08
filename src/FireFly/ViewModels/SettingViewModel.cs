using FireFly.Command;
using FireFly.CustomDialogs;
using FireFly.Settings;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using FireFly.Utilities;
using System.Linq;

namespace FireFly.ViewModels
{
    public class SettingViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty FileLocationsProperty =
            DependencyProperty.Register("FileLocations", typeof(RangeObservableCollection<FileLocation>), typeof(SettingViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty IpAddressProperty =
            DependencyProperty.Register("IpAddress", typeof(string), typeof(SettingViewModel), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(int), typeof(SettingViewModel), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnPropertyChanged)));

        public SettingViewModel(MainViewModel parent) : base(parent)
        {
            FileLocations = new RangeObservableCollection<FileLocation>();
            FileLocations.CollectionChanged += FileLocations_CollectionChanged;
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

        public int Port
        {
            get { return (int)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();

            IpAddress = Parent.SettingContainer.Settings.ConnectionSettings.IpAddress;
            Port = Parent.SettingContainer.Settings.ConnectionSettings.Port;
            var firstNotSecond = FileLocations.Except(Parent.SettingContainer.Settings.GeneralSettings.FileLocations).ToList();
            var secondNotFirst = Parent.SettingContainer.Settings.GeneralSettings.FileLocations.Except(FileLocations).ToList();

            bool changed = firstNotSecond.Any() || secondNotFirst.Any();

            if (changed)
            {
                FileLocations.Clear();
                FileLocations.AddRange(Parent.SettingContainer.Settings.GeneralSettings.FileLocations);
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

                case "IpAddress":
                    changed = svm.Parent.SettingContainer.Settings.ConnectionSettings.IpAddress != svm.IpAddress;
                    svm.Parent.SettingContainer.Settings.ConnectionSettings.IpAddress = svm.IpAddress;
                    connectionSettingsChanged = true;
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
    }
}