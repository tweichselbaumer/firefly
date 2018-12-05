using FireFly.Models;
using FireFly.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;

namespace FireFly.ViewModels
{
    public class DataPlotViewModel : AbstractViewModel, IProxyEventSubscriber
    {
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(DataPlotViewModel), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty RecordRemoteProperty =
            DependencyProperty.Register("RecordRemote", typeof(bool), typeof(DataPlotViewModel), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        private LineSeriesContainer _AccX;

        private LineSeriesContainer _AccY;

        private LineSeriesContainer _AccZ;

        private LineSeriesContainer _GyroX;

        private LineSeriesContainer _GyroY;

        private LineSeriesContainer _GyroZ;

        private Timer _Timer;

        public DataPlotViewModel(MainViewModel parent) : base(parent)
        {
            GyroX = new LineSeriesContainer(this, "X", "[°/s]");
            GyroY = new LineSeriesContainer(this, "Y", "[°/s]");
            GyroZ = new LineSeriesContainer(this, "Z", "[°/s]");
            AccX = new LineSeriesContainer(this, "X", "[m/s²]");
            AccY = new LineSeriesContainer(this, "Y", "[m/s²]");
            AccZ = new LineSeriesContainer(this, "Z", "[m/s²]");

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

        public bool RecordRemote
        {
            get { return (bool)GetValue(RecordRemoteProperty); }
            set { SetValue(RecordRemoteProperty, value); }
        }

        public void Fired(IOProxy proxy, List<AbstractProxyEventData> eventData)
        {

            ImuEventData imuEventData = (ImuEventData)eventData.FirstOrDefault(c => c is ImuEventData);
            if (imuEventData != null)
            {
                GyroX.AddDataPoint(imuEventData.Time, imuEventData.GyroX);
                GyroY.AddDataPoint(imuEventData.Time, imuEventData.GyroY);
                GyroZ.AddDataPoint(imuEventData.Time, imuEventData.GyroZ);
                AccX.AddDataPoint(imuEventData.Time, imuEventData.AccelX);
                AccY.AddDataPoint(imuEventData.Time, imuEventData.AccelY);
                AccZ.AddDataPoint(imuEventData.Time, imuEventData.AccelZ);
            }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();
            Enabled = Parent.SettingContainer.Settings.StreamingSettings.ImuRawStreamEnabled;
            RecordRemote = Parent.SettingContainer.Settings.ImuSettings.RecordRemote;
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataPlotViewModel dpvm = (d as DataPlotViewModel);
            bool changed = false;
            switch (e.Property.Name)
            {
                case "Enabled":
                    changed = dpvm.Parent.SettingContainer.Settings.StreamingSettings.ImuRawStreamEnabled != dpvm.Enabled;
                    dpvm.Parent.SettingContainer.Settings.StreamingSettings.ImuRawStreamEnabled = dpvm.Enabled;
                    try
                    {
                        if (dpvm.Enabled)
                            dpvm.Parent.IOProxy.Subscribe(dpvm, ProxyEventType.ImuEvent);
                        else
                            dpvm.Parent.IOProxy.Unsubscribe(dpvm, ProxyEventType.ImuEvent);
                    }
                    catch (Exception) { }
                    break;

                case "RecordRemote":
                    changed = dpvm.Parent.SettingContainer.Settings.ImuSettings.RecordRemote != dpvm.RecordRemote;
                    dpvm.Parent.SettingContainer.Settings.ImuSettings.RecordRemote = dpvm.RecordRemote;
                    break;

                default:
                    break;
            }
            if (changed)
            {
                dpvm.Parent.UpdateSettings(false);
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
    }
}