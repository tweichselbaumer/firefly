using FireFly.Command;
using FireFly.Data.Storage;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FireFly.ViewModels
{
    public class ReplayViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty IsReplayingProperty =
            DependencyProperty.Register("IsReplaying", typeof(bool), typeof(ReplayViewModel), new PropertyMetadata(false));

        public static readonly DependencyProperty ReplayTimeProperty =
            DependencyProperty.Register("ReplayTime", typeof(TimeSpan), typeof(ReplayViewModel), new PropertyMetadata(null));

        public ReplayViewModel(MainViewModel parent) : base(parent)
        {
        }

        public bool IsReplaying
        {
            get { return (bool)GetValue(IsReplayingProperty); }
            set { SetValue(IsReplayingProperty, value); }
        }

        public TimeSpan ReplayTime
        {
            get { return (TimeSpan)GetValue(ReplayTimeProperty); }
            set { SetValue(ReplayTimeProperty, value); }
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

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();
        }

        private Task DoStart(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    IsReplaying = true;
                }, null);
                DataReader reader = new DataReader("131777679083647432.zip", ReaderMode.Imu0 | ReaderMode.Camera0);
                reader.Open();

                //MatlabExporter matlabExporter = new MatlabExporter("output.mat", MatlabFormat.Imu0);
                //matlabExporter.Open();
                //matlabExporter.AddFromReader(reader);
                //matlabExporter.Close();

                Parent.IOProxy.ReplayOffline(reader, new Action<TimeSpan>((t) =>
                {
                    Parent.SyncContext.Post(c =>
                    {
                        ReplayTime = t;
                    }, null);
                }), new Action(() =>
                {
                    Parent.SyncContext.Post(c =>
                    {
                        IsReplaying = false;
                    }, null);
                }));
            });
        }
    }
}