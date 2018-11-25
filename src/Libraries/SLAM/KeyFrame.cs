using FireFly.VI.SLAM.Sophus;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
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
        private GeometryModel3D _PointCloud;
        private List<Point> _Points = new List<Point>();

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
                MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
                Matrix3D matrix3D = Frame.T_cam_world.Inverse().Matrix3D;

                Debug.WriteLine("T_w_c");
                Debug.WriteLine(matrix3D.ToString());
                Debug.WriteLine("\n\n");
                foreach (Point p in _Points)
                {
                    int offset = meshGeometry3D.Positions.Count;
                    if (p.InverseDepth > 0)
                    {
                        Point3D point3D = new Point3D();

                        point3D.X = (p.U - Cx) / (p.InverseDepth * Fx);
                        point3D.Y = (p.V - Cy) / (p.InverseDepth * Fy);
                        point3D.Z = 1 / p.InverseDepth;

                        double x = point3D.X;
                        double y = point3D.Y;
                        double z = point3D.Z;

                        AddCubeToMesh(meshGeometry3D, point3D, 0.002);
                    }
                }
                Material material = new DiffuseMaterial(new SolidColorBrush(Colors.Black) { Opacity = 1 });

                _PointCloud = new GeometryModel3D(meshGeometry3D, material);
                _PointCloud.Transform = new MatrixTransform3D(matrix3D);
                _PointCloud.Freeze();
            }
            return _PointCloud;
        }
        private void AddCubeToMesh(MeshGeometry3D mesh, Point3D center, double size)
        {
            if (mesh != null)
            {
                int offset = mesh.Positions.Count;

                mesh.Positions.Add(new Point3D(center.X - size, center.Y + size, center.Z - size));
                mesh.Positions.Add(new Point3D(center.X + size, center.Y + size, center.Z - size));
                mesh.Positions.Add(new Point3D(center.X + size, center.Y + size, center.Z + size));
                mesh.Positions.Add(new Point3D(center.X - size, center.Y + size, center.Z + size));
                mesh.Positions.Add(new Point3D(center.X - size, center.Y - size, center.Z - size));
                mesh.Positions.Add(new Point3D(center.X + size, center.Y - size, center.Z - size));
                mesh.Positions.Add(new Point3D(center.X + size, center.Y - size, center.Z + size));
                mesh.Positions.Add(new Point3D(center.X - size, center.Y - size, center.Z + size));

                mesh.TriangleIndices.Add(offset + 3);
                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 6);

                mesh.TriangleIndices.Add(offset + 3);
                mesh.TriangleIndices.Add(offset + 6);
                mesh.TriangleIndices.Add(offset + 7);

                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 1);
                mesh.TriangleIndices.Add(offset + 5);

                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 5);
                mesh.TriangleIndices.Add(offset + 6);

                mesh.TriangleIndices.Add(offset + 1);
                mesh.TriangleIndices.Add(offset + 0);
                mesh.TriangleIndices.Add(offset + 4);

                mesh.TriangleIndices.Add(offset + 1);
                mesh.TriangleIndices.Add(offset + 4);
                mesh.TriangleIndices.Add(offset + 5);

                mesh.TriangleIndices.Add(offset + 0);
                mesh.TriangleIndices.Add(offset + 3);
                mesh.TriangleIndices.Add(offset + 7);

                mesh.TriangleIndices.Add(offset + 0);
                mesh.TriangleIndices.Add(offset + 7);
                mesh.TriangleIndices.Add(offset + 4);

                mesh.TriangleIndices.Add(offset + 7);
                mesh.TriangleIndices.Add(offset + 6);
                mesh.TriangleIndices.Add(offset + 5);

                mesh.TriangleIndices.Add(offset + 7);
                mesh.TriangleIndices.Add(offset + 5);
                mesh.TriangleIndices.Add(offset + 4);

                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 3);
                mesh.TriangleIndices.Add(offset + 0);

                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 0);
                mesh.TriangleIndices.Add(offset + 1);
            }
        }
    }
}