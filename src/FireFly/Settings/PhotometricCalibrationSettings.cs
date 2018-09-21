using System.Collections.Generic;

namespace FireFly.Settings
{
    public class PhotometricCalibrationSettings : AbstractSettings
    {
        private List<double> _ResponseValues = new List<double>();
        private string _VignetteFileBase64;

        public List<double> ResponseValues
        {
            get
            {
                return _ResponseValues;
            }

            set
            {
                _ResponseValues = value;
            }
        }

        public string VignetteFileBase64
        {
            get
            {
                return _VignetteFileBase64;
            }

            set
            {
                _VignetteFileBase64 = value;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
        }
    }
}