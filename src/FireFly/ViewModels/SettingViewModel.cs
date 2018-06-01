using System.Windows;

namespace FireFly.ViewModels
{
    public class SettingViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty IpAddressProperty =
            DependencyProperty.Register("IpAddress", typeof(string), typeof(SettingViewModel), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(int), typeof(SettingViewModel), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnPropertyChanged)));

        public SettingViewModel(MainViewModel parent) : base(parent)
        {
        }

        public string IpAddress
        {
            get { return (string)GetValue(IpAddressProperty); }
            set { SetValue(IpAddressProperty, value); }
        }

        public int Port
        {
            get { return (int)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        internal override void SettingsUpdated()
        {
            IpAddress = Parent.SettingContainer.Settings.ConnectionSettings.IpAddress;
            Port = Parent.SettingContainer.Settings.ConnectionSettings.Port;
        }

        internal override void UpdateLinkUpBindings()
        {
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
                svm.Parent.SettingsUpdated(connectionSettingsChanged);
            }
        }
    }
}