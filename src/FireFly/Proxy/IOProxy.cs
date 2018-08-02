using LinkUp.Node;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FireFly.Proxy
{
    public class IOProxy
    {
        private LinkUpEventLabel _CameraEventLabel;
        private LinkUpEventLabel _CameraImuEventLabel;
        private LinkUpEventLabel _ImuEventLabel;
        private LinkUpNode _Node;
        private List<Tuple<IProxyEventSubscriber, ProxyEventType>> _Subscriptions = new List<Tuple<IProxyEventSubscriber, ProxyEventType>>();

        public IOProxy()
        {
        }

        public LinkUpNode Node
        {
            get
            {
                return _Node;
            }

            set
            {
                _Node = value;
            }
        }

        public void Subscribe(IProxyEventSubscriber subscriber, ProxyEventType eventType)
        {
            lock (_Subscriptions)
            {
                if (!_Subscriptions.Any(c => c.Item1 == subscriber && c.Item2 == eventType))
                {
                    _Subscriptions.Add(new Tuple<IProxyEventSubscriber, ProxyEventType>(subscriber, eventType));
                }
                UpdateSubscription();
            }
        }

        public void Unsubscribe(IProxyEventSubscriber subscriber, ProxyEventType eventType)
        {
            lock (_Subscriptions)
            {
                if (_Subscriptions.Any(c => c.Item1 == subscriber && c.Item2 == eventType))
                {
                    _Subscriptions.Remove(_Subscriptions.FirstOrDefault(c => c.Item1 == subscriber && c.Item2 == eventType));
                }
                UpdateSubscription();
            }
        }

        public void UpdateLinkUpBindings()
        {
            if (Node != null)
            {
                _CameraEventLabel = Node.GetLabelByName<LinkUpEventLabel>("firefly/computer_vision/camera_event");
                _ImuEventLabel = Node.GetLabelByName<LinkUpEventLabel>("firefly/computer_vision/imu_event");
                _CameraImuEventLabel = Node.GetLabelByName<LinkUpEventLabel>("firefly/computer_vision/camera_imu_event");

                _CameraEventLabel.Fired += _CameraEventLabel_Fired;
                _ImuEventLabel.Fired += _CameraEventLabel_Fired;
                _CameraImuEventLabel.Fired += _CameraEventLabel_Fired;

                lock (_Subscriptions)
                {
                    UpdateSubscription();
                }
            }
        }

        private void _CameraEventLabel_Fired(LinkUpEventLabel label, byte[] data)
        {
            lock (_Subscriptions)
            {
                if (label == _CameraEventLabel)
                {
                    CameraEventData eventData = CameraEventData.Parse(data, 0);
                    foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in _Subscriptions.Where(c => c.Item2 == ProxyEventType.CameraEvent))
                    {
                        t.Item1.Fired(this, new List<AbstractProxyEventData>() { eventData });
                    }
                }
                else if (label == _ImuEventLabel)
                {
                    ImuEventData eventData = ImuEventData.Parse(data, 0);
                    foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in _Subscriptions.Where(c => c.Item2 == ProxyEventType.ImuEvent))
                    {
                        t.Item1.Fired(this, new List<AbstractProxyEventData>() { eventData });
                    }
                }
                else if (label == _CameraImuEventLabel)
                {
                    ImuEventData imuEventData = ImuEventData.Parse(data, 0);
                    CameraEventData cameraEventData = null;

                    if (imuEventData.HasCameraImage)
                        cameraEventData = CameraEventData.Parse(data, 23);

                    foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in _Subscriptions.Where(c => c.Item2 == ProxyEventType.CameraImuEvent))
                    {
                        if (cameraEventData != null)
                            t.Item1.Fired(this, new List<AbstractProxyEventData>() { cameraEventData, imuEventData });
                        else
                            t.Item1.Fired(this, new List<AbstractProxyEventData>() { imuEventData });
                    }

                    foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in _Subscriptions.Where(c => c.Item2 == ProxyEventType.CameraEvent))
                    {
                        if (cameraEventData != null)
                            t.Item1.Fired(this, new List<AbstractProxyEventData>() { cameraEventData });
                    }

                    foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in _Subscriptions.Where(c => c.Item2 == ProxyEventType.ImuEvent))
                    {
                        t.Item1.Fired(this, new List<AbstractProxyEventData>() { imuEventData });
                    }
                }
            }
        }

        private void UpdateSubscription()
        {
            if (_CameraEventLabel != null && _ImuEventLabel != null && _CameraImuEventLabel != null)
            {
                if (_Subscriptions.Any(c => c.Item2 == ProxyEventType.CameraEvent) && _Subscriptions.Any(c => c.Item2 == ProxyEventType.ImuEvent) || _Subscriptions.Any(c => c.Item2 == ProxyEventType.CameraImuEvent))
                {
                    _CameraImuEventLabel.Subscribe();
                    _ImuEventLabel.Unsubscribe();
                    _CameraEventLabel.Unsubscribe();
                }
                else
                {
                    _CameraImuEventLabel.Unsubscribe();
                    if (_Subscriptions.Any(c => c.Item2 == ProxyEventType.CameraEvent))
                    {
                        _CameraEventLabel.Subscribe();
                    }
                    else
                    {
                        _CameraEventLabel.Unsubscribe();
                    }
                    if (_Subscriptions.Any(c => c.Item2 == ProxyEventType.ImuEvent))
                    {
                        _ImuEventLabel.Subscribe();
                    }
                    else
                    {
                        _ImuEventLabel.Unsubscribe();
                    }
                }
            }
        }
    }
}