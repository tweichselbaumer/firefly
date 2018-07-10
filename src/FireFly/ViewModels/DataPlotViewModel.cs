using LinkUp.Node;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace FireFly.ViewModels
{
    public class DataPlotViewModel : AbstractViewModel
    {


        public ObservableCollection<DataPoint> GyroXPoints
        {
            get { return (ObservableCollection<DataPoint>)GetValue(GyroXPointsProperty); }
            set { SetValue(GyroXPointsProperty, value); }
        }

        public static readonly DependencyProperty GyroXPointsProperty =
            DependencyProperty.Register("GyroXPoints", typeof(ObservableCollection<DataPoint>), typeof(DataPlotViewModel), new PropertyMetadata(new ObservableCollection<DataPoint>()));


        private LinkUpEventLabel _EventLabel;

        public DataPlotViewModel(MainViewModel parent) : base(parent)
        {
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

        private void _EventLabel_Fired(LinkUpEventLabel label, byte[] data)
        {
            //uint32_t timestamp;
            //uint32_t sample;
            //int16_t gx;
            //int16_t gy;
            //int16_t gz;
            //int16_t ax;
            //int16_t ay;
            //int16_t az;
            //int16_t temperature;
            //bool cam;

            double x = ((double)BitConverter.ToUInt32(data, 0)) / 1000;
            double y = ((double)BitConverter.ToInt16(data, 8)) / 131;

            Parent.SyncContext.Post(o =>
            {
                GyroXPoints.Add(new DataPoint(x, y));
                if (GyroXPoints.Count > 2000)
                {
                    GyroXPoints.RemoveAt(0);
                }
            }, null);
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataPlotViewModel dpvm = (d as DataPlotViewModel);
            bool changed = false;
            switch (e.Property.Name)
            {
                //case "Quality":
                //    changed = cvm.Parent.SettingContainer.Settings.StreamingSettings.Quality != cvm.Quality;
                //    cvm.Parent.SettingContainer.Settings.StreamingSettings.Quality = cvm.Quality;
                //    try
                //    {
                //        if (changed)
                //            cvm._QualityLabel.Value = (byte)cvm.Quality;
                //    }
                //    catch (Exception) { }
                //    break;
                //case "Enabled":
                //    changed = cvm.Parent.SettingContainer.Settings.StreamingSettings.Enabled != cvm.Enabled;
                //    cvm.Parent.SettingContainer.Settings.StreamingSettings.Enabled = cvm.Enabled;
                //    try
                //    {
                //        if (changed)
                //        {
                //            if (cvm.Enabled)
                //                cvm._EventLabel.Subscribe();
                //            else
                //                cvm._EventLabel.Unsubscribe();
                //        }

                //    }
                //    catch (Exception) { }
                //    break;

                default:
                    break;
            }
            if (changed)
            {
                dpvm.Parent.SettingsUpdated(false);
            }
        }
    }
}