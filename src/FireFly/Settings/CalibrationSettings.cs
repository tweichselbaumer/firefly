namespace FireFly.Settings
{
    public class CalibrationSettings : AbstractSettings
    {
        private AprilGridCalibrationSettings _AprilGridCalibration = new AprilGridCalibrationSettings();
        private ChArucoCalibrationSettings _ChArucoCalibrationSettings = new ChArucoCalibrationSettings();
        private ExtrinsicCalibrationSettings _ExtrinsicCalibrationSettings = new ExtrinsicCalibrationSettings();
        private ImuCalibrationSettings _ImuCalibration = new ImuCalibrationSettings();
        private IntrinsicCalibrationSettings _IntrinsicCalibrationSettings = new IntrinsicCalibrationSettings();
        private PhotometricCalibrationSettings _PhotometricCalibrationSettings = new PhotometricCalibrationSettings();

        public AprilGridCalibrationSettings AprilGridCalibration
        {
            get
            {
                return _AprilGridCalibration;
            }

            set
            {
                _AprilGridCalibration = value;
            }
        }

        public ChArucoCalibrationSettings ChArucoCalibrationSettings
        {
            get
            {
                return _ChArucoCalibrationSettings;
            }

            set
            {
                _ChArucoCalibrationSettings = value;
            }
        }

        public ExtrinsicCalibrationSettings ExtrinsicCalibrationSettings
        {
            get
            {
                return _ExtrinsicCalibrationSettings;
            }

            set
            {
                _ExtrinsicCalibrationSettings = value;
            }
        }

        public ImuCalibrationSettings ImuCalibration
        {
            get
            {
                return _ImuCalibration;
            }

            set
            {
                _ImuCalibration = value;
            }
        }

        public IntrinsicCalibrationSettings IntrinsicCalibrationSettings
        {
            get
            {
                return _IntrinsicCalibrationSettings;
            }

            set
            {
                _IntrinsicCalibrationSettings = value;
            }
        }

        public PhotometricCalibrationSettings PhotometricCalibrationSettings
        {
            get
            {
                return _PhotometricCalibrationSettings;
            }

            set
            {
                _PhotometricCalibrationSettings = value;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
        }
    }
}