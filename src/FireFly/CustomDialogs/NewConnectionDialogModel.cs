using FireFly.Command;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FireFly.CustomDialogs
{
    public class NewConnectionDialogModel : DependencyObject
    {
        public static readonly DependencyProperty ExecutablePathProperty =
            DependencyProperty.Register("ExecutablePath", typeof(string), typeof(NewConnectionDialogModel), new PropertyMetadata(""));

        public static readonly DependencyProperty HostnameProperty =
            DependencyProperty.Register("Hostname", typeof(string), typeof(NewConnectionDialogModel), new PropertyMetadata(""));

        public static readonly DependencyProperty IpAddressProperty =
            DependencyProperty.Register("IpAddress", typeof(string), typeof(NewConnectionDialogModel), new PropertyMetadata(""));

        public static readonly DependencyProperty IsLocalProperty =
            DependencyProperty.Register("IsLocal", typeof(bool), typeof(NewConnectionDialogModel), new PropertyMetadata(false));

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(NewConnectionDialogModel), new PropertyMetadata(""));

        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(int), typeof(NewConnectionDialogModel), new PropertyMetadata(3000));

        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(NewConnectionDialogModel), new PropertyMetadata(""));

        private readonly RelayCommand<object> _CloseCommand;
        private readonly RelayCommand<object> _SaveCommand;

        public NewConnectionDialogModel(Action<NewConnectionDialogModel> closeHandle, Action<NewConnectionDialogModel> saveHandle)
        {
            _CloseCommand = new RelayCommand<object>(
                async (object o) =>
                {
                    await Task.Factory.StartNew(() =>
                    {
                        closeHandle(this);
                    });
                });

            _SaveCommand = new RelayCommand<object>(
                async (object o) =>
                {
                    await Task.Factory.StartNew(() =>
                    {
                        saveHandle(this);
                    });
                });
        }

        public RelayCommand<object> CloseCommand
        {
            get
            {
                return _CloseCommand;
            }
        }

        public string ExecutablePath
        {
            get { return (string)GetValue(ExecutablePathProperty); }
            set { SetValue(ExecutablePathProperty, value); }
        }

        public string Hostname
        {
            get { return (string)GetValue(HostnameProperty); }
            set { SetValue(HostnameProperty, value); }
        }

        public string IpAddress
        {
            get { return (string)GetValue(IpAddressProperty); }
            set { SetValue(IpAddressProperty, value); }
        }

        public bool IsLocal
        {
            get { return (bool)GetValue(IsLocalProperty); }
            set { SetValue(IsLocalProperty, value); }
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

        public RelayCommand<object> SaveCommand
        {
            get
            {
                return _SaveCommand;
            }
        }

        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }
    }
}