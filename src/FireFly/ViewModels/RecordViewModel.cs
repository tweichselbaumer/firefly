using FireFly.Command;
using FireFly.Data.Storage;
using FireFly.Proxy;
using FireFly.Settings;
using FireFly.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace FireFly.ViewModels
{
    public class RecordViewModel : AbstractViewModel, IProxyEventSubscriber
    {
        public static readonly DependencyProperty FileLocationProperty =
            DependencyProperty.Register("FileLocation", typeof(FileLocation), typeof(RecordViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(RecordViewModel), new PropertyMetadata(""));

        public static readonly DependencyProperty IsRecordingProperty =
            DependencyProperty.Register("IsRecording", typeof(bool), typeof(RecordViewModel), new PropertyMetadata(false));

        public static readonly DependencyProperty NotesProperty =
            DependencyProperty.Register("Notes", typeof(string), typeof(RecordViewModel), new PropertyMetadata(""));

        public static readonly DependencyProperty RecordingTimeProperty =
            DependencyProperty.Register("RecordingTime", typeof(TimeSpan), typeof(RecordViewModel), new PropertyMetadata(null));

        private DataWritter _DataWritter;

        private Stopwatch _StopWatch;

        private Timer _Timer;

        public RecordViewModel(MainViewModel parent) : base(parent)
        {
            _StopWatch = new Stopwatch();
            _Timer = new Timer(1000);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
        }

        public FileLocation FileLocation
        {
            get { return (FileLocation)GetValue(FileLocationProperty); }
            set { SetValue(FileLocationProperty, value); }
        }

        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        public bool IsRecording
        {
            get { return (bool)GetValue(IsRecordingProperty); }
            set { SetValue(IsRecordingProperty, value); }
        }

        public string Notes
        {
            get { return (string)GetValue(NotesProperty); }
            set { SetValue(NotesProperty, value); }
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
                    _DataWritter.AddImage(0, imuEventData.TimeNanoSeconds, cameraEventData.Image.ToPNGBinary(3), cameraEventData.ExposureTime);
                }
            }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();
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
                Parent.SyncContext.Post(async c =>
                {
                    if (string.IsNullOrEmpty(FileName))
                    {
                        var controller = await Parent.DialogCoordinator.ShowMessageAsync(Parent, "Name is missing!", "Please set name!", MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative, null);
                        return;
                    }

                    if (FileLocation == null)
                    {
                        var controller = await Parent.DialogCoordinator.ShowMessageAsync(Parent, "Location is missing!", "Please select a location!", MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative, null);
                        return;
                    }

                    string fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(Path.Combine(FileLocation.Path, string.Format("{0}.{1}", FileName, "ffc"))));
                    if (File.Exists(fullPath))
                    {
                        var controller = await Parent.DialogCoordinator.ShowMessageAsync(Parent, "File already exists!", "Do you want to replace the existing file?", MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative, null);
                        if (controller == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
                        {
                            File.Delete(fullPath);
                        }
                        else
                        {
                            return;
                        }
                    }
                    _DataWritter = new DataWritter(fullPath);
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
    }
}