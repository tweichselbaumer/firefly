using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireFly.Proxy
{
    public enum SlamStatusOverall : byte
    {
        Start = 1,
        Stop = 2,
        Restart = 3,
    }

    public enum SlamOperationStatus : byte
    {
        Unknown = 0,
        Running = 1,
        Stopped = 2,
        Initializing = 3,
        WaitingForMotion = 4,
    }



    public class SlamStatusEventData : AbstractProxyEventData
    {

        private SlamOperationStatus _Status;

        public SlamOperationStatus Status
        {
            get
            {
                return _Status;
            }

            set
            {
                _Status = value;
            }
        }

        public static SlamStatusEventData Parse(byte[] data)
        {
            SlamStatusEventData obj = new SlamStatusEventData();

            obj.Status = (SlamOperationStatus)data[0];

            return obj;
        }
    }
}
