﻿using FireFly.VI.SLAM.Data;
using FireFly.VI.SLAM.Sophus;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Media3D;

namespace FireFly.VI.SLAM.Visualisation
{
    public class SlamModel3D : DependencyObject
    {
        public static readonly DependencyProperty CameraPositionProperty =
            DependencyProperty.Register("CameraPosition", typeof(MatrixTransform3D), typeof(SlamModel3D), new PropertyMetadata(null));

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(Model3D), typeof(SlamModel3D), new PropertyMetadata(null));

        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register("CoordinateSystems", typeof(ObservableCollection<Visual3D>), typeof(SlamModel3D), new PropertyMetadata(null));

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

        private Map _Map = new Map();

        private bool _ShowKeyFrameOrientations;

        private SynchronizationContext _SyncContext;

        private System.Timers.Timer _Timer;

        public SlamModel3D(SynchronizationContext synchronizationContext, bool enableRenderTimer = true)
        {
            _SyncContext = synchronizationContext;

            TrajectoryFrame = new Point3DCollection();
            TrajectoryKeyFrame = new Point3DCollection();

            if (enableRenderTimer)
            {
                _Timer = new System.Timers.Timer(50);
                _Timer.Elapsed += _Timer_Elapsed;
                _Timer.Start();
            }

            CoordinateSystems = new ObservableCollection<Visual3D>();
        }

        public MatrixTransform3D CameraPosition
        {
            get { return (MatrixTransform3D)GetValue(CameraPositionProperty); }
            set { SetValue(CameraPositionProperty, value); }
        }

        public ObservableCollection<Visual3D> CoordinateSystems
        {
            get { return (ObservableCollection<Visual3D>)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
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

        public SE3 LastTransformation()
        {
            return _Map.LastTransformation();
        }

        public void Render()
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

                    CameraPosition = new MatrixTransform3D(_Map.LastTransformation().Matrix3D);

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
                        foreach (Visual3D visual3D in _Map.GetKeyFrameOrientations(CoordinateSystems.Count != 0))
                        {
                            CoordinateSystems.Add(visual3D);
                        }
                    }
                    else
                    {
                        CoordinateSystems.Clear();
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

        public void Reset()
        {
            _Map.Reset();
            _SyncContext.Send(d =>
            {
                Model = new Model3DGroup();
                CoordinateSystems.Clear();
            }, null);
        }

        private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Render();
        }
    }
}