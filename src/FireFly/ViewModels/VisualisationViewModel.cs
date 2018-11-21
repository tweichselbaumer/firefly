using FireFly.Proxy;
using FireFly.VI.SLAM;
using FireFly.VI.SLAM.Sophus;
using FireFly.VI.SLAM.Visualisation;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Windows;

namespace FireFly.ViewModels
{
    public class VisualisationViewModel : AbstractViewModel, IProxyEventSubscriber
    {
        public static readonly DependencyProperty SlamModel3DProperty =
            DependencyProperty.Register("SlamModel3D", typeof(SlamModel3D), typeof(VisualisationViewModel), new PropertyMetadata(null));

        public VisualisationViewModel(MainViewModel parent) : base(parent)
        {
            SlamModel3D = new SlamModel3D(Parent.SyncContext);
            SlamModel3D.AddNewFrame(Frame.CreateFrame(0, 0, 0, 0, 1, 0, 0, 0, 1));
            Parent.IOProxy.Subscribe(this, ProxyEventType.SlamMapEvent);


            //SO3 so3 = new VI.SLAM.Sophus.SO3(new VI.SLAM.Sophus.Quaternion(0.93609, -0.16584, -0.18112, -0.25186));
            //Matrix<double> R_1I = so3.Matrix;
            //string a = R_1I.ToMatrixString();
            //so3 = so3.Inverse();
            //Matrix<double> R_I1 = so3.Matrix;
            //string b = R_I1.ToMatrixString();

            //Vector<double> t = Vector<double>.Build.Dense(3);
            //t[0] = 2;
            //t[1] = -5;
            //t[2] = -4;

            //Sim3 T = new Sim3(1, t, so3.Inverse());

            //Vector<double> t2 = so3.Inverse().TransformVector(t);
            //b = t2.ToVectorString() + "\n\n" + t.ToVectorString() + "\n\n" + T.Matrix.ToMatrixString() + "\n\n" + T.Inverse().Matrix.ToMatrixString();
            //int c = 2;
        }

        public SlamModel3D SlamModel3D
        {
            get { return (SlamModel3D)GetValue(SlamModel3DProperty); }
            set { SetValue(SlamModel3DProperty, value); }
        }

        public void Fired(IOProxy proxy, List<AbstractProxyEventData> eventData)
        {
            if (eventData.Count == 1 && eventData[0] is SlamEventData)
            {
                SlamModel3D slamModel3D = null;
                Parent.SyncContext.Send(d =>
                {
                    slamModel3D = SlamModel3D;
                }, null);

                slamModel3D.AddNewFrame((eventData[0] as SlamEventData).Frame);
            }
        }
    }
}