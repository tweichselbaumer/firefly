using FireFly.Models;
using LinkUp.Node;
using System;
using System.Timers;
using System.Windows;

namespace FireFly.ViewModels
{
    public class DataPlotViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(DataPlotViewModel), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        private LineSeriesContainer _AccX;
        private LineSeriesContainer _AccY;
        private LineSeriesContainer _AccZ;
        private LinkUpEventLabel _EventLabel;
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

            _Timer = new Timer(50);
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

        internal override void SettingsUpdated()
        {
            Enabled = Parent.SettingContainer.Settings.StreamingSettings.ImuRawStreamEnabled;
        }

        internal override void UpdateLinkUpBindings()
        {
            if (Parent.Node != null)
            {
                _EventLabel = Parent.Node.GetLabelByName<LinkUpEventLabel>("firefly/test/imu_event");
                if (Enabled)
                    _EventLabel.Subscribe();
                else
                    _EventLabel.Unsubscribe();
                _EventLabel.Fired += EventLabel_Fired;
            }
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
                        if (changed)
                        {
                            if (dpvm.Enabled)
                                dpvm._EventLabel.Subscribe();
                            else
                                dpvm._EventLabel.Unsubscribe();
                        }
                    }
                    catch (Exception) { }
                    break;

                default:
                    break;
            }
            if (changed)
            {
                dpvm.Parent.SettingsUpdated(false);
            }
        }

        private void EventLabel_Fired(LinkUpEventLabel label, byte[] data)
        {
            double gyro_scale = 16.4;
            double acc_scale = 2048 / 9.80665;
            double temp_offset = 21;
            double temp_scale = 333.8;

            double time = ((double)BitConverter.ToUInt32(data, 0)) / 1000;

            double gyroX = ((double)BitConverter.ToInt16(data, 8)) / gyro_scale;
            double gyroY = ((double)BitConverter.ToInt16(data, 10)) / gyro_scale;
            double gyroZ = ((double)BitConverter.ToInt16(data, 12)) / gyro_scale;

            double accX = ((double)BitConverter.ToInt16(data, 14)) / acc_scale;
            double accY = ((double)BitConverter.ToInt16(data, 16)) / acc_scale;
            double accZ = ((double)BitConverter.ToInt16(data, 18)) / acc_scale;

            double temperatur = ((double)BitConverter.ToInt16(data, 20)) / temp_scale + temp_offset;

            bool camera = BitConverter.ToBoolean(data, 22);


            GyroX.AddDataPoint(time, gyroX);
            GyroY.AddDataPoint(time, gyroY);
            GyroZ.AddDataPoint(time, gyroZ);
            AccX.AddDataPoint(time, accX);
            AccY.AddDataPoint(time, accY);
            AccZ.AddDataPoint(time, accZ);

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