using FireFly.Command;
using FireFly.Proxy;
using FireFly.Utilities;
using FireFly.VI.SLAM;
using FireFly.VI.SLAM.Visualisation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace FireFly.ViewModels
{
    public class VisualisationViewModel : AbstractViewModel, IProxyEventSubscriber
    {
        public static readonly DependencyProperty FPSProperty =
            DependencyProperty.Register("FPS", typeof(int), typeof(VisualisationViewModel), new PropertyMetadata(0));

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(SlamOperationStatus), typeof(VisualisationViewModel), new PropertyMetadata(SlamOperationStatus.Stopped));

        private FPSCounter _FPSCounter = new FPSCounter();

        private SlamModel3D _SlamModel3D;

        private Timer _Timer;

        public VisualisationViewModel(MainViewModel parent) : base(parent)
        {
            _SlamModel3D = new SlamModel3D(Parent.SyncContext);
            _SlamModel3D.AddNewFrame(new Frame(0, 0, 0, 0, 1, 0, 0, 0, 1));
            Parent.IOProxy.Subscribe(this, ProxyEventType.SlamMapEvent);
            _Timer = new Timer(300);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
            Parent.IOProxy.Subscribe(this, ProxyEventType.SlamStatusEvent);
        }

        public int FPS
        {
            get { return (int)GetValue(FPSProperty); }
            set { SetValue(FPSProperty, value); }
        }

        public SlamModel3D SlamModel3D
        {
            get
            {
                return _SlamModel3D;
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

        public SlamOperationStatus Status
        {
            get { return (SlamOperationStatus)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
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
            SlamMapEventData slamMapEvent = (SlamMapEventData)eventData.FirstOrDefault(c => c is SlamMapEventData);
            SlamStatusEventData slamStatusEvent = (SlamStatusEventData)eventData.FirstOrDefault(c => c is SlamStatusEventData);
            if (slamMapEvent != null)
            {
                if (slamMapEvent.PublishType == SlamPublishType.Frame)
                {
                    SlamModel3D.AddNewFrame(slamMapEvent.Frame);
                    _FPSCounter.CountFrame();
                }
                else if (slamMapEvent.PublishType == SlamPublishType.KeyframeWithPoints)
                {
                    SlamModel3D.AddNewKeyFrame(slamMapEvent.KeyFrame);
                }
                else if (slamMapEvent.PublishType == SlamPublishType.Reset)
                {
                    SlamModel3D.Reset();
                }
            }
            if (slamStatusEvent != null)
            {
                Parent.SyncContext.Post(o =>
                {
                    Status = slamStatusEvent.Status;
                }, null);
            }
        }

        private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Parent.SyncContext.Post(o =>
            {
                FPS = (int)_FPSCounter.FramesPerSecond;
            }, null);
        }

        private Task DoStart(object o)
        {
            return Task.Run(() =>
            {
                Parent.IOProxy.ChangeSlamStatus(SlamStatusOverall.Start);
            });
        }

        private Task DoStop(object o)
        {
            return Task.Run(() =>
            {
                Parent.IOProxy.ChangeSlamStatus(SlamStatusOverall.Stop);
            });
        }
    }
}