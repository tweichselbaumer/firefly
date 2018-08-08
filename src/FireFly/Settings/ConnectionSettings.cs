using System.Reflection;

namespace FireFly.Settings
{
    public class ConnectionSettings : AbstractSettings
    {
        private string _IpAddress;

        private int _Port;

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

        public override void SetDefaults()
        {
            base.SetDefaults();

            Port = 3000;
            IpAddress = "0.0.0.0";
        }
    }
}