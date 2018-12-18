namespace FireFly.Settings
{
    public class ExtrinsicCalibrationSettings : AbstractSettings
    {
        private double[,] _M_Inv_Acc;
        private double[,] _M_Inv_Gyro;
        private double[,] _R_Acc_Gyro;
        private double _ReprojectionSigma;

        private double[,] _T_Cam_Imu;
        private bool _TimeCalibration;

        public double[,] M_Inv_Acc
        {
            get
            {
                return _M_Inv_Acc;
            }

            set
            {
                _M_Inv_Acc = value;
            }
        }

        public double[,] M_Inv_Gyro
        {
            get
            {
                return _M_Inv_Gyro;
            }

            set
            {
                _M_Inv_Gyro = value;
            }
        }

        public double[,] R_Acc_Gyro
        {
            get
            {
                return _R_Acc_Gyro;
            }

            set
            {
                _R_Acc_Gyro = value;
            }
        }

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

        public double[,] T_Cam_Imu
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