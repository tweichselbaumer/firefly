using FireFly.VI.SLAM.Sophus;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace FireFly.VI.SLAM
{
    public enum TrajectoryType
    {
        PreOptimazation,
        Optimazation
    }

    public class Map
    {
        private List<Frame> _Frames = new List<Frame>();
        private List<KeyFrame> _KeyFrames = new List<KeyFrame>();

        public List<Frame> Frames
        {
            get
            {
                lock (_Frames)
                {
                    return _Frames.Where(c => c != null).ToList();
                }
            }
        }

        public List<KeyFrame> KeyFrames
        {
            get
            {
                lock (_KeyFrames)
                {
                    return _KeyFrames.Where(c => c != null).ToList();
                }
            }
        }

        public void AddNewFrame(Frame frame)
        {
            lock (_Frames)
            {
                while (_Frames.Count <= frame.Id)
                {
                    _Frames.Add(null);
                }
                _Frames[(int)frame.Id] = frame;
            }
        }

        public KeyFrame AddNewKeyFrame(KeyFrame keyFrame)
        {
            lock (_KeyFrames)
            {
                while (_KeyFrames.Count <= keyFrame.Id)
                {
                    _KeyFrames.Add(null);
                }
                KeyFrame old = _KeyFrames[(int)keyFrame.Id];
                _KeyFrames[(int)keyFrame.Id] = keyFrame;
                return old;
            }
        }

        public List<Model3D> GetKeyFrameOrientations()
        {
            List<Model3D> kfos = new List<Model3D>();
            lock (_KeyFrames)
            {
                foreach (KeyFrame keyFrame in _KeyFrames)
                {
                    if (keyFrame != null)
                    {
                        Model3D kfo = keyFrame.GetCoordinateSystem().Content;
                        kfo.Freeze();
                        kfos.Add(kfo);
                    }
                }
            }
            return kfos;
        }

        public List<GeometryModel3D> GetPointCloud(bool onlyNew = false)
        {
            List<GeometryModel3D> pointClouds = new List<GeometryModel3D>();
            lock (_KeyFrames)
            {
                foreach (KeyFrame keyFrame in _KeyFrames)
                {
                    if (keyFrame != null)
                    {
                        GeometryModel3D cloud = keyFrame.GetPointCloud(onlyNew);
                        if (cloud != null)
                            pointClouds.Add(cloud);
                    }
                }
            }
            return pointClouds;
        }

        public List<Vector3> GetTrajectory(TrajectoryType trajectoryType = TrajectoryType.PreOptimazation)
        {
            switch (trajectoryType)
            {
                case TrajectoryType.PreOptimazation:
                    lock (_Frames)
                    {
                        return _Frames.Where(d => d != null).Select(c => c.T_cam_world.Inverse().SE3.Translation).ToList();
                    }
                case TrajectoryType.Optimazation:
                    lock (_KeyFrames)
                    {
                        return _KeyFrames.Where(d => d != null).Select(c => c.Frame.T_cam_world.Inverse().SE3.Translation).ToList();
                    }
                default:
                    return new List<Vector3>();
            }
        }

        public bool HasFrames()
        {
            lock (_Frames)
            {
                return _Frames.Count > 0;
            }
        }

        public bool HasKeyFrames()
        {
            lock (_KeyFrames)
            {
                return _KeyFrames.Count > 0;
            }
        }

        public Sim3 LastTransformation()
        {
            lock (_Frames)
            {
                if (_Frames.Count > 0)
                    return _Frames.Where(c => c != null).Last().T_cam_world.Inverse();
                else
                    return new Sim3();
            }
        }

        public void Reset()
        {
            lock (_Frames)
            {
                _Frames.Clear();
            }
            lock (_KeyFrames)
            {
                _KeyFrames.Clear();
            }
        }
    }
}