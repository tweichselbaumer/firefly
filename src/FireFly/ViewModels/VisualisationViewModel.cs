using FireFly.Proxy;
using FireFly.VI.SLAM;
using FireFly.VI.SLAM.Visualisation;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;

namespace FireFly.ViewModels
{
    public class VisualisationViewModel : AbstractViewModel, IProxyEventSubscriber
    {
        public static readonly DependencyProperty SlamModel3DProperty =
            DependencyProperty.Register("SlamModel3D", typeof(SlamModel3D), typeof(VisualisationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty Transform3DProperty =
            DependencyProperty.Register("Transform3D", typeof(MatrixTransform3D), typeof(VisualisationViewModel), new PropertyMetadata(null));

        public VisualisationViewModel(MainViewModel parent) : base(parent)
        {
            SlamModel3D = new SlamModel3D(Parent.SyncContext);
            SlamModel3D.AddNewFrame(new Frame(0, 0, 0, 0, 1, 0, 0, 0, 1));
            Parent.IOProxy.Subscribe(this, ProxyEventType.SlamMapEvent);
        }

        public SlamModel3D SlamModel3D
        {
            get { return (SlamModel3D)GetValue(SlamModel3DProperty); }
            set { SetValue(SlamModel3DProperty, value); }
        }

        public MatrixTransform3D Transform3D
        {
            get { return (MatrixTransform3D)GetValue(Transform3DProperty); }
            set { SetValue(Transform3DProperty, value); }
        }

        public void Fired(IOProxy proxy, List<AbstractProxyEventData> eventData)
        {
            if (eventData.Count == 1 && eventData[0] is SlamEventData)
            {
                SlamEventData data = (eventData[0] as SlamEventData);

                SlamModel3D slamModel3D = null;
                Parent.SyncContext.Send(d =>
                {
                    slamModel3D = SlamModel3D;
                }, null);

                if (data.KeyFrame == null)
                {
                    slamModel3D.AddNewFrame(data.Frame);
                }
                else
                {
                    MatrixTransform3D t = slamModel3D.AddNewKeyFrame(data.KeyFrame);
                    Parent.SyncContext.Post(d =>
                    {
                        Transform3D = t;
                    }, null);
                }


            }
        }
    }
}