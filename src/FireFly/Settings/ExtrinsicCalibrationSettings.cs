namespace FireFly.Settings
{
    public class ExtrinsicCalibrationSettings : AbstractSettings
    {
        private double[][] _T_Cam_Imu;

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
    }
}