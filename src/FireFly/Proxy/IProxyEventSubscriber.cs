using System.Collections.Generic;

namespace FireFly.Proxy
{
    public interface IProxyEventSubscriber
    {
        void Fired(IOProxy proxy, List<AbstractProxyEventData> eventData);
    }
}