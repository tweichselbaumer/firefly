using HelixToolkit.Wpf;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FireFly.VI.SLAM
{
    public class KeyFrame
    {
        private CoordinateSystemVisual3D _CoordinateSystemVisual3D;
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
            Frame = frame;
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

        public CoordinateSystemVisual3D GetCoordinateSystem(bool onlyNew = false)
        {
            bool newCoordinateSystem = false;
            if (_CoordinateSystemVisual3D == null)
            {
                newCoordinateSystem = true;
                _CoordinateSystemVisual3D = new CoordinateSystemVisual3D();
                _CoordinateSystemVisual3D.ArrowLengths = 0.05;
                _CoordinateSystemVisual3D.Transform = new MatrixTransform3D(Frame.T_base_world.Inverse().Matrix3D);
            }

            if (onlyNew && newCoordinateSystem || !onlyNew)
                return _CoordinateSystemVisual3D;
            else
                return null;
        }

        public GeometryModel3D GetPointCloud(bool onlyNew = false)
        {
            bool newPointCloud = false;
            if (_PointCloud == null)
            {
                newPointCloud = true;
                MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
                Matrix3D matrix3D = Frame.T_cam_world.Inverse().Matrix3D;

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

                        AddCubeToMesh(meshGeometry3D, point3D, 0.002, p.Colors[0]);
                    }
                }
                //Material material = new DiffuseMaterial(new SolidColorBrush(Colors.Black) { Opacity = 1 });
                Material material = new DiffuseMaterial(new LinearGradientBrush(Colors.Black, Colors.White, 0.0));

                _PointCloud = new GeometryModel3D(meshGeometry3D, material);
                _PointCloud.Transform = new MatrixTransform3D(matrix3D);
                _PointCloud.Freeze();
            }
            if (onlyNew && newPointCloud || !onlyNew)
                return _PointCloud;
            else
                return null;
        }

        private void AddCubeToMesh(MeshGeometry3D mesh, Point3D center, double size, byte color = 0)
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

                for (int i = 0; i < 12; i++)
                {
                    mesh.TextureCoordinates.Add(new System.Windows.Point(((double)color) / 255.0, 0));
                }

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