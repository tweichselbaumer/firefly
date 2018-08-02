using FireFly.Command;
using FireFly.Data.Storage;
using FireFly.Proxy;
using FireFly.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace FireFly.ViewModels
{
    public class RecordViewModel : AbstractViewModel, IProxyEventSubscriber
    {
        private DataWritter _DataWritter;

        public static readonly DependencyProperty IsRecordingProperty =
            DependencyProperty.Register("IsRecording", typeof(bool), typeof(RecordViewModel), new PropertyMetadata(false));

        public static readonly DependencyProperty RecordingTimeProperty =
            DependencyProperty.Register("RecordingTime", typeof(TimeSpan), typeof(RecordViewModel), new PropertyMetadata(null));

        private Timer _Timer;
        private Stopwatch _StopWatch;

        public RecordViewModel(MainViewModel parent) : base(parent)
        {
            _StopWatch = new Stopwatch();
            _Timer = new Timer(1000);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
        }

        public bool IsRecording
        {
            get { return (bool)GetValue(IsRecordingProperty); }
            set { SetValue(IsRecordingProperty, value); }
        }

        public TimeSpan RecordingTime
        {
            get { return (TimeSpan)GetValue(RecordingTimeProperty); }
            set { SetValue(RecordingTimeProperty, value); }
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

        internal override void SettingsUpdated()
        {
        }

        private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Parent.SyncContext.Post(c =>
            {
                RecordingTime = _StopWatch.Elapsed;
            }, null);
        }

        private Task DoStart(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    _DataWritter = new DataWritter(string.Format("{0}.zip", DateTime.Now.ToFileTime()));
                    _DataWritter.Open();
                    Parent.IOProxy.Subscribe(this, ProxyEventType.CameraImuEvent);
                    _StopWatch.Restart();
                    IsRecording = true;
                }, null);
            });
        }

        private Task DoStop(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    Parent.IOProxy.Unsubscribe(this, ProxyEventType.CameraImuEvent);
                    _DataWritter.Close();
                    _StopWatch.Restart();
                    IsRecording = false;
                }, null);
            });
        }

        public void Fired(IOProxy proxy, List<AbstractProxyEventData> eventData)
        {
            if (_DataWritter != null)
            {
                ImuEventData imuEventData = (ImuEventData)eventData.FirstOrDefault(c => c is ImuEventData);
                CameraEventData cameraEventData = (CameraEventData)eventData.FirstOrDefault(c => c is CameraEventData);

                if (imuEventData != null)
                {

                    _DataWritter.AddImu(0, imuEventData.TimeNanoSeconds, imuEventData.GyroX / 180 * Math.PI, imuEventData.GyroY / 180 * Math.PI, imuEventData.GyroZ / 180 * Math.PI, imuEventData.AccelX, imuEventData.AccelY, imuEventData.AccelZ);

                }
                if (cameraEventData != null)
                {
                    Task.Factory.StartNew(() =>
                    {
                        _DataWritter.AddImage(0, imuEventData.TimeNanoSeconds, cameraEventData.Image.ToPNGBinary(3));
                    });
                }
            }
        }
    }
}