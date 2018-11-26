using FireFly.Proxy;
using FireFly.VI.SLAM;
using FireFly.VI.SLAM.Visualisation;
using System.Collections.Generic;

namespace FireFly.ViewModels
{
    public class VisualisationViewModel : AbstractViewModel, IProxyEventSubscriber
    {
        private SlamModel3D _SlamModel3D;

        public VisualisationViewModel(MainViewModel parent) : base(parent)
        {
            _SlamModel3D = new SlamModel3D(Parent.SyncContext);
            _SlamModel3D.AddNewFrame(new Frame(0, 0, 0, 0, 1, 0, 0, 0, 1));
            Parent.IOProxy.Subscribe(this, ProxyEventType.SlamMapEvent);
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
    }
}