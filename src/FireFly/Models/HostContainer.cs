using System.Windows;

namespace FireFly.Models
{
    public class HostContainer : DependencyObject
    {
        public static readonly DependencyProperty HostNameProperty =
            DependencyProperty.Register("HostName", typeof(string), typeof(HostContainer), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty IpAddressProperty =
            DependencyProperty.Register("IpAddress", typeof(string), typeof(HostContainer), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(HostContainer), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(int), typeof(HostContainer), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(HostContainer), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));

        public string HostName
        {
            get { return (string)GetValue(HostNameProperty); }
            set { SetValue(HostNameProperty, value); }
        }

        public string IpAddress
        {
            get { return (string)GetValue(IpAddressProperty); }
            set { SetValue(IpAddressProperty, value); }
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

        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //SettingViewModel svm = (d as SettingViewModel);
            //bool changed = false;
            //bool connectionSettingsChanged = false;
            //switch (e.Property.Name)
            //{
            //    case "Port":
            //        changed = svm.Parent.SettingContainer.Settings.ConnectionSettings.Port != svm.Port;
            //        svm.Parent.SettingContainer.Settings.ConnectionSettings.Port = svm.Port;
            //        connectionSettingsChanged = true;
            //        break;
            //
            //    case "HostName":
            //
            //    case "Username":
            //        changed = svm.Parent.SettingContainer.Settings.ConnectionSettings.Username != svm.Username;
            //        svm.Parent.SettingContainer.Settings.ConnectionSettings.Username = svm.Username;
            //        connectionSettingsChanged = false;
            //        break;

            //    case "Password":
            //        changed = svm.Parent.SettingContainer.Settings.ConnectionSettings.Password != svm.Password;
            //        svm.Parent.SettingContainer.Settings.ConnectionSettings.Password = svm.Password;
            //        connectionSettingsChanged = false;
            //        break;

            //    case "IpAddress":
            //        changed = svm.Parent.SettingContainer.Settings.ConnectionSettings.IpAddress != svm.IpAddress;
            //        svm.Parent.SettingContainer.Settings.ConnectionSettings.IpAddress = svm.IpAddress;
            //        connectionSettingsChanged = true;
            //        break;

            //    default:
            //        break;
            //}
            //if (changed)
            //{
            //    svm.Parent.UpdateSettings(connectionSettingsChanged);
            //}
        }
    }
}