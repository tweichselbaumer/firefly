using MathNet.Numerics.LinearAlgebra;
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

        public static readonly DependencyProperty TrajectoryFrameProperty =
            DependencyProperty.Register("TrajectoryFrame", typeof(Point3DCollection), typeof(SlamModel3D), new PropertyMetadata(null));

        public static readonly DependencyProperty TrajectoryKeyFrameProperty =
         DependencyProperty.Register("TrajectoryKeyFrame", typeof(Point3DCollection), typeof(SlamModel3D), new PropertyMetadata(null));

        public static readonly DependencyProperty Transform3DProperty =
                DependencyProperty.Register("Transform3D", typeof(MatrixTransform3D), typeof(SlamModel3D), new PropertyMetadata(null));

        private Map _Map = new Map();
        private System.Timers.Timer _Timer;
        private SynchronizationContext _SyncContext;

        public SlamModel3D(SynchronizationContext synchronizationContext)
        {
            _SyncContext = synchronizationContext;

            TrajectoryFrame = new Point3DCollection();
            TrajectoryKeyFrame = new Point3DCollection();

            _Timer = new System.Timers.Timer(200);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
        }

        private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_Map.HasFrames() || _Map.HasKeyFrames())
            {
                bool onlyNew = true;

                List<Vector<double>> pointsKeyFrame = _Map.GetTrajectory(TrajectoryType.Optimazation);
                List<Vector<double>> pointsFrame = _Map.GetTrajectory(TrajectoryType.PreOptimazation);

                List<GeometryModel3D> pointClouds = _Map.GetPointCloud(onlyNew);

                _SyncContext.Post(d =>
                {
                    TrajectoryKeyFrame = new Point3DCollection(pointsKeyFrame.SelectMany(c => new List<Point3D>() { new Point3D(c[0], c[1], c[2]), new Point3D(c[0], c[1], c[2]) }));
                    if (TrajectoryKeyFrame.Count > 0)
                        TrajectoryKeyFrame.RemoveAt(0);

                    TrajectoryFrame = new Point3DCollection(pointsFrame.SelectMany(c => new List<Point3D>() { new Point3D(c[0], c[1], c[2]), new Point3D(c[0], c[1], c[2]) }));
                    if (TrajectoryFrame.Count > 0)
                        TrajectoryFrame.RemoveAt(0);

                    Transform3D = new MatrixTransform3D(_Map.LastTransformation().Matrix3D);
                    if (!(onlyNew && Model != null))
                    {
                        Model = new Model3DGroup();
                    }
                    foreach (GeometryModel3D pointCloud in pointClouds)
                    {
                        (Model as Model3DGroup).Children.Add(pointCloud);
                    }
                }, null);
            }
        }

        public Model3D Model
        {
            get { return (Model3D)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
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
            //List<Vector<double>> points = _Map.GetTrajectory(TrajectoryType.PreOptimazation);

            //MeshBuilder meshBuilder = new MeshBuilder();
            //meshBuilder.AddTube(points.Select(c => new Point3D(c[0], c[1], c[2])).ToList(), 0.1, 10, false);
            //GeometryModel3D geometryModel3D = new GeometryModel3D(meshBuilder.ToMesh(), Materials.Gold);
            //geometryModel3D.Freeze();

            //_SyncContext.Post(d =>
            //{
            //    Model3DGroup modelGroup = new Model3DGroup();
            //    modelGroup.Children.Add(geometryModel3D);
            //    Model = modelGroup;
            //}, null);
        }

        public void AddNewKeyFrame(KeyFrame keyFrame)
        {

            _Map.AddNewKeyFrame(keyFrame);
        }

        public void Reset()
        {
            _Map.Reset();
            _SyncContext.Send(d =>
            {
                Model = new Model3DGroup();
            }, null);
        }
    }
}