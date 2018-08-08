using FireFly.Models;
using FireFly.Proxy;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;

namespace FireFly.ViewModels
{
    public class DataPlotViewModel : AbstractViewModel, IProxyEventSubscriber
    {
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(DataPlotViewModel), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

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

        public void Fired(IOProxy proxy, List<AbstractProxyEventData> eventData)
        {
            if (eventData.Count == 1 && eventData[0] is ImuEventData)
            {
                ImuEventData data = eventData[0] as ImuEventData;
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
            Enabled = Parent.SettingContainer.Settings.StreamingSettings.ImuRawStreamEnabled;
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