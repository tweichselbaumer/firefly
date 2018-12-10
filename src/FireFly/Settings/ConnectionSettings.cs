using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FireFly.Settings
{
    public class Connection
    {
        private string _Hostname;
        private Guid _Id;
        private string _IpAddress;
        private string _Password;
        private int _Port;
        private string _Username;
        private string _ExecutablePath;
        private bool _IsLocal;

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

        public Guid Id
        {
            get
            {
                return _Id;
            }

            set
            {
                _Id = value;
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

        public string ExecutablePath
        {
            get
            {
                return _ExecutablePath;
            }

            set
            {
                _ExecutablePath = value;
            }
        }

        public bool IsLocal
        {
            get
            {
                return _IsLocal;
            }

            set
            {
                _IsLocal = value;
            }
        }
    }

    public class ConnectionSettings : AbstractSettings
    {
        private List<Connection> _Connections = new List<Connection>();

        private Guid _SelectedConnectionGuid;

        public List<Connection> Connections
        {
            get
            {
                return _Connections;
            }

            set
            {
                _Connections = value;
            }
        }

        [JsonIgnore]
        public Connection SelectedConnection
        {
            get
            {
                Connection connection = _Connections.FirstOrDefault(c => c.Id == SelectedConnectionGuid);
                if (connection == null)
                {
                    return new Connection() { Id = Guid.Empty };
                }
                else
                {
                    return connection;
                }
            }
        }

        public Guid SelectedConnectionGuid
        {
            get
            {
                return _SelectedConnectionGuid;
            }

            set
            {
                _SelectedConnectionGuid = value;
            }
        }
    }
}