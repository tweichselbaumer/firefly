namespace FireFly.Settings
{
    public class SlamSettings : AbstractSettings
    {
        private bool _ReproducibleExecution;

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

        public override void SetDefaults()
        {
            base.SetDefaults();
            ReproducibleExecution = false;
        }
    }
}