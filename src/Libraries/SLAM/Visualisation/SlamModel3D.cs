using HelixToolkit.Wpf;
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

        private Map _Map = new Map();
        private SynchronizationContext _SyncContext;

        public SlamModel3D(SynchronizationContext synchronizationContext)
        {
            _SyncContext = synchronizationContext;
        }

        public Model3D Model
        {
            get { return (Model3D)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
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


            MeshBuilder meshBuilder = new MeshBuilder();
            meshBuilder.AddTube(points.Select(c => new Point3D(c[0], c[1], c[2])).ToList(), 0.1, 10, false);
            GeometryModel3D geometryModel3D = new GeometryModel3D(meshBuilder.ToMesh(), Materials.Gold);
            geometryModel3D.Freeze();

            _SyncContext.Post(d =>
            {
                Model3DGroup modelGroup = new Model3DGroup();
                modelGroup.Children.Add(geometryModel3D);
                Model = modelGroup;
            }, null);
        }
    }
}