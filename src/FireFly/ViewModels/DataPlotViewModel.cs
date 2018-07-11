using FireFly.Models;
using LinkUp.Node;
using System;
using System.Timers;
using System.Windows;

namespace FireFly.ViewModels
{
    public class DataPlotViewModel : AbstractViewModel
    {
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
            GyroX = new LineSeriesContainer(this, "Gyro X");
            GyroY = new LineSeriesContainer(this, "Gyro Y");
            GyroZ = new LineSeriesContainer(this, "Gyro Z");
            AccX = new LineSeriesContainer(this, "Acc X");
            AccY = new LineSeriesContainer(this, "Acc Y");
            AccZ = new LineSeriesContainer(this, "Acc Z");

            _Timer = new Timer(200);
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
        }

        internal override void UpdateLinkUpBindings()
        {
            if (Parent.Node != null)
            {
                _EventLabel = Parent.Node.GetLabelByName<LinkUpEventLabel>("firefly/test/imu_event");
                _EventLabel.Subscribe();
                _EventLabel.Fired += _EventLabel_Fired;
            }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataPlotViewModel dpvm = (d as DataPlotViewModel);
            bool changed = false;
            switch (e.Property.Name)
            {
                default:
                    break;
            }
            if (changed)
            {
                dpvm.Parent.SettingsUpdated(false);
            }
        }

        private void _EventLabel_Fired(LinkUpEventLabel label, byte[] data)
        {
            double gyro_scale = 16.4;
            double acc_scale = 2048;
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