using FireFly.VI.SLAM.Sophus;

namespace FireFly.VI.SLAM
{
    public class Frame
    {
        private ulong _Id;
        private Sim3 _T_cam_world = new Sim3();

        public Sim3 T_cam_world
        {
            get
            {
                return _T_cam_world;
            }

            set
            {
                _T_cam_world = value;
            }
        }

        public ulong Id
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

        public static Frame CreateFrame(ulong id, double tx, double ty, double tz, double q1, double q2, double q3, double q4, double s)
        {
            Frame frame = new Frame();

            frame.Id = id;
            frame.T_cam_world = new Sim3(s, tx, ty, tz, q1, q2, q3, q4);

            return frame;
        }
    }
}