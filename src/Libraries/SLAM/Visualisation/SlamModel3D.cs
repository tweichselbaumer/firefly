using MathNet.Numerics.LinearAlgebra;
using System;
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

        public static readonly DependencyProperty TrajectoryProperty =
            DependencyProperty.Register("Trajectory", typeof(Point3DCollection), typeof(SlamModel3D), new PropertyMetadata(null));

        public static readonly DependencyProperty Transform3DProperty =
                DependencyProperty.Register("Transform3D", typeof(MatrixTransform3D), typeof(SlamModel3D), new PropertyMetadata(null));

        private Map _Map = new Map();

        private SynchronizationContext _SyncContext;

        public SlamModel3D(SynchronizationContext synchronizationContext)
        {
            _SyncContext = synchronizationContext;
            Trajectory = new Point3DCollection();
        }

        public Model3D Model
        {
            get { return (Model3D)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public Point3DCollection Trajectory
        {
            get { return (Point3DCollection)GetValue(TrajectoryProperty); }
            set { SetValue(TrajectoryProperty, value); }
        }

        public void Reset()
        {
            _Map.Reset();
        }

        public MatrixTransform3D Transform3D
        {
            get { return (MatrixTransform3D)GetValue(Transform3DProperty); }
            set { SetValue(Transform3DProperty, value); }
        }

        public void AddNewFrame(Frame frame)
        {
            //_Map.AddNewFrame(frame);
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
            List<Vector<double>> points = _Map.GetTrajectory(TrajectoryType.Optimazation);

            //List<GeometryModel3D> pointClouds = _Map.GetPointCloud();

            _SyncContext.Post(d =>
            {
                Model3DGroup modelGroup = new Model3DGroup();
                Trajectory = new Point3DCollection(points.SelectMany(c => new List<Point3D>() { new Point3D(c[0], c[1], c[2]), new Point3D(c[0], c[1], c[2]) }));
                Trajectory.RemoveAt(0);
                //foreach (GeometryModel3D pointCloud in pointClouds)
                //{
                //    modelGroup.Children.Add(pointCloud);
                //}

                Transform3D = new MatrixTransform3D(_Map.LastTransformation().Matrix3D);
                Model = modelGroup;
            }, null);
        }
    }
}