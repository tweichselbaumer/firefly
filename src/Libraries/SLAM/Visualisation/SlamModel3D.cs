using FireFly.VI.SLAM.Data;
using HelixToolkit.Wpf;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Media3D;

namespace FireFly.VI.SLAM.Visualisation
{
    public class SlamModel3D : DependencyObject
    {
        

        public static readonly DependencyProperty ModelProperty =
                    DependencyProperty.Register("Model", typeof(Model3D), typeof(SlamModel3D), new PropertyMetadata(null));

        public static readonly DependencyProperty ShowFrameTrajectoryProperty =
            DependencyProperty.Register("ShowFrameTrajectory", typeof(bool), typeof(SlamModel3D), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowKeyFrameTrajectoryProperty =
            DependencyProperty.Register("ShowKeyFrameTrajectory", typeof(bool), typeof(SlamModel3D), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowPointCloudProperty =
            DependencyProperty.Register("ShowPointCloud", typeof(bool), typeof(SlamModel3D), new PropertyMetadata(true));

        public static readonly DependencyProperty TrajectoryFrameProperty =
            DependencyProperty.Register("TrajectoryFrame", typeof(Point3DCollection), typeof(SlamModel3D), new PropertyMetadata(null));

        public static readonly DependencyProperty TrajectoryKeyFrameProperty =
         DependencyProperty.Register("TrajectoryKeyFrame", typeof(Point3DCollection), typeof(SlamModel3D), new PropertyMetadata(null));

        public static readonly DependencyProperty Transform3DProperty =
                DependencyProperty.Register("Transform3D", typeof(MatrixTransform3D), typeof(SlamModel3D), new PropertyMetadata(null));

        private Map _Map = new Map();

        private bool _ShowKeyFrameOrientations;

        private SynchronizationContext _SyncContext;

        private System.Timers.Timer _Timer;

        public SlamModel3D(SynchronizationContext synchronizationContext)
        {
            _SyncContext = synchronizationContext;

            TrajectoryFrame = new Point3DCollection();
            TrajectoryKeyFrame = new Point3DCollection();

            _Timer = new System.Timers.Timer(200);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
        }

      

        public Model3D Model
        {
            get { return (Model3D)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public bool ShowFrameTrajectory
        {
            get { return (bool)GetValue(ShowFrameTrajectoryProperty); }
            set { SetValue(ShowFrameTrajectoryProperty, value); }
        }

        public bool ShowKeyFrameOrientations
        {
            get
            {
                return _ShowKeyFrameOrientations;
            }

            set
            {
                _ShowKeyFrameOrientations = value;
            }
        }

        public bool ShowKeyFrameTrajectory
        {
            get { return (bool)GetValue(ShowKeyFrameTrajectoryProperty); }
            set { SetValue(ShowKeyFrameTrajectoryProperty, value); }
        }

        public bool ShowPointCloud
        {
            get { return (bool)GetValue(ShowPointCloudProperty); }
            set { SetValue(ShowPointCloudProperty, value); }
        }

        public Point3DCollection TrajectoryFrame
        {
            get { return (Point3DCollection)GetValue(TrajectoryFrameProperty); }
            set { SetValue(TrajectoryFrameProperty, value); }
        }

        public Point3DCollection TrajectoryKeyFrame
        {
            get { return (Point3DCollection)GetValue(TrajectoryKeyFrameProperty); }
            set { SetValue(TrajectoryKeyFrameProperty, value); }
        }

        public MatrixTransform3D Transform3D
        {
            get { return (MatrixTransform3D)GetValue(Transform3DProperty); }
            set { SetValue(Transform3DProperty, value); }
        }

        public void AddNewFrame(Frame frame)
        {
            _Map.AddNewFrame(frame);
        }

        public void AddNewKeyFrame(KeyFrame keyFrame)
        {
            KeyFrame oldKeyFrame = _Map.AddNewKeyFrame(keyFrame);
            if (oldKeyFrame != null)
            {
                _SyncContext.Post(d =>
                {
                    (Model as Model3DGroup).Children.Remove(oldKeyFrame.GetPointCloud());
                }, null);
            }
        }

        public void ExportToMatlab(string fileName)
        {
            VIMatlabExporter exporter = new VIMatlabExporter(fileName);
            exporter.Open();

            exporter.ExportMap(_Map);

            exporter.Close();
        }

        public void Reset()
        {
            _Map.Reset();
            _SyncContext.Send(d =>
            {
                Model = new Model3DGroup();
            }, null);
        }

        private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool showPointCloud = false;
            bool showKeyFrameTrajectory = false;
            bool showFrameTrajectory = false;
            bool onlyNew = true;

            _SyncContext.Send(d =>
            {
                showPointCloud = ShowPointCloud;
                showKeyFrameTrajectory = ShowKeyFrameTrajectory;
                showFrameTrajectory = ShowFrameTrajectory;
                onlyNew = !(showPointCloud && Model != null && (Model as Model3DGroup).Children.Count == 0);

            }, null);

            if (_Map.HasFrames() || _Map.HasKeyFrames())
            {
                List<Vector3> pointsKeyFrame = new List<Vector3>();
                List<Vector3> pointsFrame = new List<Vector3>();

                if (showKeyFrameTrajectory)
                    pointsKeyFrame = _Map.GetTrajectory(TrajectoryType.Optimazation);
                if (showFrameTrajectory)
                    pointsFrame = _Map.GetTrajectory(TrajectoryType.PreOptimazation);

                List<GeometryModel3D> pointClouds = new List<GeometryModel3D>();
                if (showPointCloud)
                    pointClouds = _Map.GetPointCloud(onlyNew);
           
                _SyncContext.Post(d =>
                {
                    TrajectoryKeyFrame = new Point3DCollection(pointsKeyFrame.SelectMany(c => new List<Point3D>() { new Point3D(c.X, c.Y, c.Z), new Point3D(c.X, c.Y, c.Z) }));
                    if (TrajectoryKeyFrame.Count > 0)
                        TrajectoryKeyFrame.RemoveAt(0);

                    TrajectoryFrame = new Point3DCollection(pointsFrame.SelectMany(c => new List<Point3D>() { new Point3D(c.X, c.Y, c.Z), new Point3D(c.X, c.Y, c.Z) }));
                    if (TrajectoryFrame.Count > 0)
                        TrajectoryFrame.RemoveAt(0);

                    Transform3D = new MatrixTransform3D(_Map.LastTransformation().Matrix3D);
                    if (Model == null || !showPointCloud)
                    {
                        Model = new Model3DGroup();
                    }
                    foreach (GeometryModel3D pointCloud in pointClouds)
                    {
                        (Model as Model3DGroup).Children.Add(pointCloud);
                    }

                    if (ShowKeyFrameOrientations)
                    {
                    }
                }, null);
            }
            else
            {
                _SyncContext.Post(d =>
                {
                    if (TrajectoryFrame != null && TrajectoryFrame.Count > 0)
                        TrajectoryFrame = new Point3DCollection();
                    if (TrajectoryKeyFrame != null && TrajectoryKeyFrame.Count > 0)
                        TrajectoryKeyFrame = new Point3DCollection();
                }, null);
            }
        }
    }
}