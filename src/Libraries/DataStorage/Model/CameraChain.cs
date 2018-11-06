using YamlDotNet.Serialization;

namespace FireFly.Data.Storage.Model
{
    public enum CameraModel
    {
        Pinhole,
        Omnidirectional,
        DoubleSphere,
        ExtendedUnified
    }

    public enum DistortionModel
    {
        RadialTangential,
        Equidistant,
        FieldOfView,
        None
    }

    public class Camera
    {
        private CameraModel _CameraModel;
        private double _Cx;
        private double _Cy;
        private double[] _DistortionCoefficients;
        private DistortionModel _DistortionModel;
        private double _Fx;
        private double _Fy;
        private int _Height;
        private string _RosTopic;
        private double[][] _TCamImu;
        private double _TimeshiftCamImu;
        private int _Width;

        [YamlIgnore]
        public CameraModel CameraModel
        {
            get
            {
                return _CameraModel;
            }

            set
            {
                _CameraModel = value;
            }
        }

        [YamlMember(Alias = "camera_model", ApplyNamingConventions = false)]
        public string CameraModelString
        {
            get
            {
                switch (_CameraModel)
                {
                    case CameraModel.Pinhole:
                        return "pinhole";

                    case CameraModel.Omnidirectional:
                        return "omni";

                    case CameraModel.DoubleSphere:
                        return "ds";

                    case CameraModel.ExtendedUnified:
                        return "eucm";

                    default:
                        return "pinhole";
                }
            }
            set
            {
                switch (value)
                {
                    case "pinhole":
                        _CameraModel = CameraModel.Pinhole;
                        break;

                    case "omni":
                        _CameraModel = CameraModel.Omnidirectional;
                        break;

                    case "ds":
                        _CameraModel = CameraModel.DoubleSphere;
                        break;

                    case "eucm":
                        _CameraModel = CameraModel.ExtendedUnified;
                        break;

                    default:
                        break;
                }
            }
        }

        [YamlMember(Alias = "cam_overlaps", ApplyNamingConventions = false)]
        public object[] CamOverlaps
        {
            get
            {
                return new object[] { };
            }
            set
            {
            }
        }

        [YamlIgnore]
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

        [YamlIgnore]
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

        [YamlMember(Alias = "distortion_coeffs", ApplyNamingConventions = false)]
        public double[] DistortionCoefficients
        {
            get
            {
                return _DistortionCoefficients;
            }

            set
            {
                _DistortionCoefficients = value;
            }
        }

        [YamlIgnore]
        public DistortionModel DistortionModel
        {
            get
            {
                return _DistortionModel;
            }

            set
            {
                _DistortionModel = value;
            }
        }

        [YamlMember(Alias = "distortion_model", ApplyNamingConventions = false)]
        public string DistortionModelString
        {
            get
            {
                switch (_DistortionModel)
                {
                    case DistortionModel.RadialTangential:
                        return "radtan";

                    case DistortionModel.Equidistant:
                        return "equidistant";

                    case DistortionModel.None:
                        return "none";

                    case DistortionModel.FieldOfView:
                        return "fov";

                    default:
                        return "none";
                }
            }

            set
            {
                switch (value)
                {
                    case "radtan":
                        _DistortionModel = DistortionModel.RadialTangential;
                        break;

                    case "equidistant":
                        _DistortionModel = DistortionModel.Equidistant;
                        break;

                    case "none":
                        _DistortionModel = DistortionModel.None;
                        break;

                    case "fov":
                        _DistortionModel = DistortionModel.FieldOfView;
                        break;

                    default:
                        break;
                }
            }
        }

        [YamlIgnore]
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

        [YamlIgnore]
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

        [YamlIgnore]
        public int Height
        {
            get
            {
                return _Height;
            }

            set
            {
                _Height = value;
            }
        }

        [YamlMember(Alias = "intrinsics", ApplyNamingConventions = false)]
        public double[] Intrinsics
        {
            get
            {
                return new double[] { Fx, Fy, Cx, Cy };
            }
            set
            {
                if (value.Length == 4)
                {
                    Fx = value[0];
                    Fy = value[1];
                    Cx = value[2];
                    Cy = value[3];
                }
            }
        }

        [YamlMember(Alias = "resolution", ApplyNamingConventions = false)]
        public int[] Resolution
        {
            get
            {
                return new int[] { Width, Height };
            }
            set
            {
                if (value.Length == 2)
                {
                    Width = value[0];
                    Height = value[1];
                }
            }
        }

        [YamlMember(Alias = "rostopic", ApplyNamingConventions = false)]
        public string RosTopic
        {
            get
            {
                return _RosTopic;
            }

            set
            {
                _RosTopic = value;
            }
        }

        [YamlMember(Alias = "T_cam_imu", ApplyNamingConventions = false)]
        public double[][] T_Cam_Imu
        {
            get
            {
                return _TCamImu;
            }

            set
            {
                _TCamImu = value;
            }
        }

        [YamlMember(Alias = "timeshift_cam_imu", ApplyNamingConventions = false)]
        public double TimeshiftCamImu
        {
            get
            {
                return _TimeshiftCamImu;
            }

            set
            {
                _TimeshiftCamImu = value;
            }
        }

        [YamlIgnore]
        public int Width
        {
            get
            {
                return _Width;
            }

            set
            {
                _Width = value;
            }
        }
    }

    public class CameraChain
    {
        private Camera _Cam0;

        public Camera Cam0
        {
            get
            {
                return _Cam0;
            }

            set
            {
                _Cam0 = value;
            }
        }
    }
}