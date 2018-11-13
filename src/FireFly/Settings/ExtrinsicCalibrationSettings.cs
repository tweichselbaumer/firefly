namespace FireFly.Settings
{
    public class ExtrinsicCalibrationSettings : AbstractSettings
    {
        private double _ReprojectionSigma;

        private double[][] _T_Cam_Imu;

        private bool _TimeCalibration;

        public double ReprojectionSigma
        {
            get
            {
                return _ReprojectionSigma;
            }

            set
            {
                _ReprojectionSigma = value;
            }
        }

        public double[][] T_Cam_Imu
        {
            get
            {
                return _T_Cam_Imu;
            }

            set
            {
                _T_Cam_Imu = value;
            }
        }

        public bool TimeCalibration
        {
            get
            {
                return _TimeCalibration;
            }

            set
            {
                _TimeCalibration = value;
            }
        }
    }
}