using FireFly.Proxy;
using FireFly.Utilities;
using FireFly.VI.SLAM;
using FireFly.VI.SLAM.Visualisation;
using System.Collections.Generic;
using System.Timers;
using System.Windows;

namespace FireFly.ViewModels
{
    public class VisualisationViewModel : AbstractViewModel, IProxyEventSubscriber
    {
        public static readonly DependencyProperty FPSProperty =
            DependencyProperty.Register("FPS", typeof(int), typeof(VisualisationViewModel), new PropertyMetadata(0));

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

        public void Fired(IOProxy proxy, List<AbstractProxyEventData> eventData)
        {
            if (eventData.Count == 1 && eventData[0] is SlamEventData)
            {
                SlamEventData data = (eventData[0] as SlamEventData);

                if (data.PublishType == SlamPublishType.Frame)
                {
                    SlamModel3D.AddNewFrame(data.Frame);
                    _FPSCounter.CountFrame();
                }
                else if (data.PublishType == SlamPublishType.KeyframeWithPoints)
                {
                    SlamModel3D.AddNewKeyFrame(data.KeyFrame);
                }
                else if (data.PublishType == SlamPublishType.Reset)
                {
                    SlamModel3D.Reset();
                }
            }
        }

        private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Parent.SyncContext.Post(o =>
            {
                FPS = (int)_FPSCounter.FramesPerSecond;
            }, null);
        }
    }
}