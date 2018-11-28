using FireFly.Command;
using FireFly.Data.Storage;
using FireFly.Models;
using FireFly.Proxy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace FireFly.ViewModels
{
    public class DebugViewModel : AbstractViewModel, IProxyEventSubscriber
    {
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(DebugViewModel), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        private LineSeriesContainer _AccX;

        private LineSeriesContainer _AccY;

        private LineSeriesContainer _AccZ;

        private LineSeriesContainer _GyroX;

        private LineSeriesContainer _GyroY;

        private LineSeriesContainer _GyroZ;
        private Timer _Timer;

        public DebugViewModel(MainViewModel parent) : base(parent)
        {
            GyroX = new LineSeriesContainer(this, "X", "[°/s²]");
            GyroY = new LineSeriesContainer(this, "Y", "[°/s²]");
            GyroZ = new LineSeriesContainer(this, "Z", "[°/s²]");
            AccX = new LineSeriesContainer(this, "X", "[m/s³]");
            AccY = new LineSeriesContainer(this, "Y", "[m/s³]");
            AccZ = new LineSeriesContainer(this, "Z", "[m/s³]");

            _Timer = new Timer(500);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
        }

        public LineSeriesContainer AccX
        {
            get
            {
                return _AccX;
            }

            set
            {
                _AccX = value;
            }
        }

        public LineSeriesContainer AccY
        {
            get
            {
                return _AccY;
            }

            set
            {
                _AccY = value;
            }
        }

        public LineSeriesContainer AccZ
        {
            get
            {
                return _AccZ;
            }

            set
            {
                _AccZ = value;
            }
        }

        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        public LineSeriesContainer GyroX
        {
            get
            {
                return _GyroX;
            }

            set
            {
                _GyroX = value;
            }
        }

        public LineSeriesContainer GyroY
        {
            get
            {
                return _GyroY;
            }

            set
            {
                _GyroY = value;
            }
        }

        public LineSeriesContainer GyroZ
        {
            get
            {
                return _GyroZ;
            }

            set
            {
                _GyroZ = value;
            }
        }

        public RelayCommand<object> ShutdownCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoShutdown(o);
                    });
            }
        }

        public RelayCommand<object> StartCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoStart(o);
                    });
            }
        }

        public RelayCommand<object> StopCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoStop(o);
                    });
            }
        }

        public void Fired(IOProxy proxy, List<AbstractProxyEventData> eventData)
        {
            if (eventData.Count == 1 && eventData[0] is ImuDerivedEventData)
            {
                ImuDerivedEventData data = eventData[0] as ImuDerivedEventData;
                GyroX.AddDataPoint(data.Time, data.GyroX);
                GyroY.AddDataPoint(data.Time, data.GyroY);
                GyroZ.AddDataPoint(data.Time, data.GyroZ);
                AccX.AddDataPoint(data.Time, data.AccelX);
                AccY.AddDataPoint(data.Time, data.AccelY);
                AccZ.AddDataPoint(data.Time, data.AccelZ);
            }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();
            Enabled = Parent.SettingContainer.Settings.StreamingSettings.ImuDerivedStreamEnabled;
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DebugViewModel dvm = (d as DebugViewModel);
            bool changed = false;
            switch (e.Property.Name)
            {
                case "Enabled":
                    changed = dvm.Parent.SettingContainer.Settings.StreamingSettings.ImuDerivedStreamEnabled != dvm.Enabled;
                    dvm.Parent.SettingContainer.Settings.StreamingSettings.ImuDerivedStreamEnabled = dvm.Enabled;
                    try
                    {
                        if (dvm.Enabled)
                            dvm.Parent.IOProxy.Subscribe(dvm, ProxyEventType.ImuDerivedEvent);
                        else
                            dvm.Parent.IOProxy.Unsubscribe(dvm, ProxyEventType.ImuDerivedEvent);
                    }
                    catch (Exception) { }
                    break;

                default:
                    break;
            }
            if (changed)
            {
                dvm.Parent.UpdateSettings(false);
            }
        }

        private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            GyroX.DrawPoints();
            GyroY.DrawPoints();
            GyroZ.DrawPoints();
            AccX.DrawPoints();
            AccY.DrawPoints();
            AccZ.DrawPoints();
        }

        private Task DoShutdown(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                string expactString = string.Format("{0}@{1}:.{{0,}}[$]", Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Username, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Hostname);

                RemoteDataStore remoteDataStore = new RemoteDataStore(Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.IpAddress, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Username, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Password);
                remoteDataStore.ExecuteCommands(new List<string>() { string.Format("sudo shutdown -t 0\n{0}\n", Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Password) }, expactString);
            });
        }

        private Task DoStart(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                string expactString = string.Format("{0}@{1}:.{{0,}}[$]", Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Username, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Hostname);

                RemoteDataStore remoteDataStore = new RemoteDataStore(Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.IpAddress, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Username, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Password);
                remoteDataStore.ExecuteCommands(new List<string>() { "nohup /opt/firefly/computer-vision </dev/null >/opt/firefly/log.log 2>&1 &" }, expactString);
            });
        }

        private Task DoStop(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                string expactString = string.Format("{0}@{1}:.{{0,}}[$]", Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Username, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Hostname);

                RemoteDataStore remoteDataStore = new RemoteDataStore(Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.IpAddress, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Username, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Password);
                remoteDataStore.ExecuteCommands(new List<string>() { "killall -9 computer-vision" }, expactString);
            });
        }
    }
}