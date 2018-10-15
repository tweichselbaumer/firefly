using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace FireFly.Data.Storage.Model
{
    public enum CalibrationTargetType
    {
        Aprilgrid,
        Checkerboard,
        Circlegrid
    }

    public class CalibrationTarget
    {
        private int _TagCols;
        private int _TagRows;
        private double _TagSize;
        private double _TagSpacing;
        private CalibrationTargetType _TargetType;

        public int TagCols
        {
            get
            {
                return _TagCols;
            }

            set
            {
                _TagCols = value;
            }
        }

        public int TagRows
        {
            get
            {
                return _TagRows;
            }

            set
            {
                _TagRows = value;
            }
        }

        public double TagSize
        {
            get
            {
                return _TagSize;
            }

            set
            {
                _TagSize = value;
            }
        }

        public double TagSpacing
        {
            get
            {
                return _TagSpacing;
            }

            set
            {
                _TagSpacing = value;
            }
        }

        [YamlIgnore]
        public CalibrationTargetType TargetType
        {
            get
            {
                return _TargetType;
            }

            set
            {
                _TargetType = value;
            }
        }

        [YamlMember(Alias = "target_type", ApplyNamingConventions = false, ScalarStyle = ScalarStyle.SingleQuoted)]
        public string TargetTypeString
        {
            get
            {
                return TargetType.ToString().ToLower();
            }
            set
            {
                switch (value)
                {
                    case "aprilgrid":
                        _TargetType = CalibrationTargetType.Aprilgrid;
                        break;

                    case "checkerboard":
                        _TargetType = CalibrationTargetType.Checkerboard;
                        break;

                    case "circlegrid":
                        _TargetType = CalibrationTargetType.Circlegrid;
                        break;

                    default:
                        break;
                }
            }
        }
    }
}