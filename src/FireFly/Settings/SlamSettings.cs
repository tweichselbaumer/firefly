namespace FireFly.Settings
{
    public class SlamSettings : AbstractSettings
    {
        private bool _EnableVisualInertial;
        private bool _ReproducibleExecution;
        private bool _ShowKeyFrameOrientations;

        public bool EnableVisualInertial
        {
            get
            {
                return _EnableVisualInertial;
            }

            set
            {
                _EnableVisualInertial = value;
            }
        }

        public bool ReproducibleExecution
        {
            get
            {
                return _ReproducibleExecution;
            }

            set
            {
                _ReproducibleExecution = value;
            }
        }

        public bool ShowKeyFrameOrientations
        {
            get
            {
                return _ShowKeyFrameOrientations;
            }

            set
            {
                _ShowKeyFrameOrientations = value;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            ReproducibleExecution = false;
        }
    }
}