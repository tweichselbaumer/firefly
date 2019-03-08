using FireFly.VI.SLAM.Sophus;

namespace FireFly.VI.SLAM
{
    public class Frame
    {
        private Vector3 _BiasAccelerometer = new Vector3();
        private Vector3 _BiasGyroscope = new Vector3();
        private uint _Id;
        private SE3 _T_base_world = new SE3();
        private Sim3 _T_cam_world = new Sim3();
        private double _Time;
        private Vector3 _Velocity = new Vector3();

        public Frame(uint id, double time, Sim3 Tcw, SE3 Tbw, Vector3 v, Vector3 bg, Vector3 ba)
        {
            Id = id;
            Time = time;
            T_cam_world = Tcw;
            T_base_world = Tbw;
            Velocity = v;
            BiasGyroscope = bg;
            BiasAccelerometer = ba;
        }

        public Vector3 BiasAccelerometer
        {
            get
            {
                return _BiasAccelerometer;
            }

            set
            {
                _BiasAccelerometer = value;
            }
        }

        public Vector3 BiasGyroscope
        {
            get
            {
                return _BiasGyroscope;
            }

            set
            {
                _BiasGyroscope = value;
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

        public SE3 T_base_world
        {
            get
            {
                return _T_base_world;
            }

            set
            {
                _T_base_world = value;
            }
        }

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

        public double Time
        {
            get
            {
                return _Time;
            }

            set
            {
                _Time = value;
            }
        }

        public Vector3 Velocity
        {
            get
            {
                return _Velocity;
            }

            set
            {
                _Velocity = value;
            }
        }
    }
}