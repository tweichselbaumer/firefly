using FireFly.VI.SLAM;
using FireFly.VI.SLAM.Visualisation;
using System;
using System.Timers;
using System.Windows;

namespace FireFly.ViewModels
{
    public class VisualisationViewModel : AbstractViewModel
    {
        int i = 0;
        public static readonly DependencyProperty SlamModel3DProperty =
            DependencyProperty.Register("SlamModel3D", typeof(SlamModel3D), typeof(VisualisationViewModel), new PropertyMetadata(null));

        public VisualisationViewModel(MainViewModel parent) : base(parent)
        {
            SlamModel3D = new SlamModel3D(Parent.SyncContext);
            SlamModel3D.AddNewFrame(Frame.CreateFrame(0, 0, 0, 0, 1, 0, 0, 0, 1));
            Timer t = new Timer(100);
            t.Elapsed += T_Elapsed;
            t.Start();
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            SlamModel3D slamModel3D = null;
            Parent.SyncContext.Send(d =>
            {
                slamModel3D = SlamModel3D;
            }, null);

            slamModel3D.AddNewFrame(Frame.CreateFrame(0, i++ * 0.1, 1, 0.2, 1, 0, 0, 0, 1));
        }

        public SlamModel3D SlamModel3D
        {
            get { return (SlamModel3D)GetValue(SlamModel3DProperty); }
            set { SetValue(SlamModel3DProperty, value); }
        }
    }
}