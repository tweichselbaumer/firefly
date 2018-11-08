namespace FireFly.Settings
{
    public class ConnectionSettings : AbstractSettings
    {
        private string _Hostname;
        private string _IpAddress;
        private string _Password;
        private int _Port;
        private string _Username;

        public string Hostname
        {
            get
            {
                return _Hostname;
            }

            set
            {
                _Hostname = value;
            }
        }

        public string IpAddress
        {
            get
            {
                return _IpAddress;
            }

            set
            {
                _IpAddress = value;
            }
        }

        public string Password
        {
            get
            {
                return _Password;
            }

            set
            {
                _Password = value;
            }
        }

        public int Port
        {
            get
            {
                return _Port;
            }

            set
            {
                _Port = value;
            }
        }

        public string Username
        {
            get
            {
                return _Username;
            }

            set
            {
                _Username = value;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            Port = 3000;
            IpAddress = "0.0.0.0";
        }
    }
}