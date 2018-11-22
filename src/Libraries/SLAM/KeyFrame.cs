using HelixToolkit.Wpf;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace FireFly.VI.SLAM
{
    public class KeyFrame
    {
        private double _Cx;
        private double _Cy;
        private Frame _Frame;
        private double _Fx;
        private double _Fy;
        private uint _Id;
        private List<Point> _Points = new List<Point>();
        private GeometryModel3D _PointCloud;

        public KeyFrame(uint id, double fx, double fy, double cx, double cy, int points, Frame frame)
        {
            Id = id;
            Fx = fx;
            Fy = fy;
            Cx = cx;
            Cy = cy;
            Points.AddRange(Enumerable.Range(1, points).Select(c => (Point)null));
        }

        public double Cx
        {
            get
            {
                return _Cx;
            }

            set
            {
                _Cx = value;
            }
        }

        public double Cy
        {
            get
            {
                return _Cy;
            }

            set
            {
                _Cy = value;
            }
        }

        public Frame Frame
        {
            get
            {
                return _Frame;
            }

            set
            {
                _Frame = value;
            }
        }

        public double Fx
        {
            get
            {
                return _Fx;
            }

            set
            {
                _Fx = value;
            }
        }

        public double Fy
        {
            get
            {
                return _Fy;
            }

            set
            {
                _Fy = value;
            }
        }

        public uint Id
        {
            get
            {
                return _Id;
            }

            set
            {
                _Id = value;
            }
        }

        public List<Point> Points
        {
            get
            {
                return _Points;
            }

            set
            {
                _Points = value;
            }
        }

        public GeometryModel3D GetPointCloud()
        {
            if (_PointCloud == null)
            {
                MeshBuilder meshBuilder = new MeshBuilder();

                foreach (Point p in _Points)
                {
                    if (p.InverseDepth > 0)
                    {
                        Point3D center = new Point3D();

                        Vector<double> c_position = Vector<double>.Build.Dense(4);
                        c_position[0] = (p.U - Cx) / (p.InverseDepth * Fx);
                        c_position[1] = (p.V - Cy) / (p.InverseDepth * Fy);
                        c_position[2] = 1 / p.InverseDepth;
                        c_position[3] = 1;

                        Vector<double> w_position = Frame.T_cam_world.Inverse().Matrix * c_position;

                        center.X = w_position[0];
                        center.Y = w_position[1];
                        center.Z = w_position[2];

                        meshBuilder.AddSphere(center, 0.01);
                    }
                }

                _PointCloud = new GeometryModel3D(meshBuilder.ToMesh(), Materials.Black);
            }
            return _PointCloud;
        }
    }
}