namespace FireFly.Settings
{
    public class ConnectionSettings
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
    }
}