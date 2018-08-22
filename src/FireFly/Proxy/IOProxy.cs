using FireFly.Data.Storage;
using LinkUp.Node;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FireFly.Proxy
{
    public enum IOProxyMode
    {
        Live,
        Offline
    }

    public class IOProxy : IDisposable
    {
        private LinkUpEventLabel _CameraEventLabel;
        private LinkUpEventLabel _CameraImuEventLabel;
        private LinkUpEventLabel _ImuEventLabel;
        private LinkUpPropertyLabel<Int16> _ExposureLabel;
        private LinkUpFunctionLabel _ReplayDataSend;
        private LinkUpFunctionLabel _GetRemoteChessboardCorner;
        private LinkUpNode _Node;
        private IOProxyMode _ProxyMode = IOProxyMode.Live;
        private List<Tuple<IProxyEventSubscriber, ProxyEventType>> _Subscriptions = new List<Tuple<IProxyEventSubscriber, ProxyEventType>>();
        private List<Task> _Tasks = new List<Task>();

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

        public IOProxyMode ProxyMode
        {
            get
            {
                return _ProxyMode;
            }

            private set
            {
                _ProxyMode = value;
                lock (_Subscriptions)
                {
                    UpdateSubscription();
                }
            }
        }

        public void Dispose()
        {
            //TODO: wait for tasks
        }

        public void ReplayOffline(DataReader reader, Action<TimeSpan> updateTime, Action onClose)
        {
            _Tasks.Add(Task.Factory.StartNew(() =>
            {
                ProxyMode = IOProxyMode.Offline;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                int nextTimeUpdate = 1000;
                long startTime = -1;
                int currentTime = 0;
                while (reader.HasNext())
                {
                    Tuple<long, List<Tuple<ReaderMode, object>>> res = reader.Next();
                    if (startTime == -1)
                        startTime = res.Item1;

                    lock (_Subscriptions)
                    {
                        ImuEventData imuEventData = null;
                        CameraEventData cameraEventData = null;

                        int rawSize = 0;
                        byte[] rawImage = null;
                        byte[] rawImu = null;
                        double exposureTime = 0.0;

                        foreach (Tuple<ReaderMode, object> val in res.Item2)
                        {
                            if (val.Item1 == ReaderMode.Imu0)
                            {
                                imuEventData = ImuEventData.Parse(res.Item1, (Tuple<double, double, double, double, double, double>)val.Item2, res.Item2.Any(c => c.Item1 == ReaderMode.Camera0));
                                rawSize += imuEventData.RawSize;
                                rawImu = imuEventData.Raw;
                            }
                            if (val.Item1 == ReaderMode.Camera0)
                            {
                                cameraEventData = CameraEventData.Parse(((Tuple<double, byte[]>)val.Item2).Item2, 0, false, ((Tuple<double, byte[]>)val.Item2).Item1);
                                rawSize += cameraEventData.RawSize;
                                rawImage = ((Tuple<double, byte[]>)val.Item2).Item2;
                                exposureTime = ((Tuple<double, byte[]>)val.Item2).Item1;
                            }
                        }

                        if (rawSize > 0)
                        {
                            byte[] data = new byte[rawSize];
                            Array.Copy(rawImu, data, rawImu.Length);
                            if (rawImage != null)
                            {
                                Array.Copy(BitConverter.GetBytes(exposureTime), 0, data, imuEventData.RawSize, sizeof(double));
                                Array.Copy(rawImage, 0, data, imuEventData.RawSize + sizeof(double), rawImage.Length);
                            }
                            _ReplayDataSend.AsyncCall(data);
                        }

                        foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in _Subscriptions.Where(c => c.Item2 == ProxyEventType.CameraImuEvent))
                        {
                            if (cameraEventData != null)
                                t.Item1.Fired(this, new List<AbstractProxyEventData>() { cameraEventData, imuEventData });
                            else
                            {
                                if (imuEventData != null)
                                    t.Item1.Fired(this, new List<AbstractProxyEventData>() { imuEventData });
                            }
                        }

                        foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in _Subscriptions.Where(c => c.Item2 == ProxyEventType.CameraEvent))
                        {
                            if (cameraEventData != null)
                                t.Item1.Fired(this, new List<AbstractProxyEventData>() { cameraEventData });
                        }

                        foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in _Subscriptions.Where(c => c.Item2 == ProxyEventType.ImuEvent))
                        {
                            if (imuEventData != null)
                                t.Item1.Fired(this, new List<AbstractProxyEventData>() { imuEventData });
                        }
                    }
                    currentTime += reader.DeltaTimeMs;
                    int sleep = (int)(currentTime - watch.ElapsedMilliseconds);
                    if (sleep > reader.DeltaTimeMs)
                        Thread.Sleep(sleep);

                    if (res.Item1 / 1000 > nextTimeUpdate)
                    {
                        nextTimeUpdate += 1000;
                        updateTime(reader.Length - TimeSpan.FromMilliseconds((res.Item1 - startTime) / (1000 * 1000)));
                    }
                }
                reader.Close();
                ProxyMode = IOProxyMode.Live;
                onClose();
            }, TaskCreationOptions.LongRunning));
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

        public void SetExposure(Int16 exposure)
        {
            if (_ExposureLabel != null)
            {
                _ExposureLabel.Value = exposure;
            }
        }

        public void UpdateLinkUpBindings()
        {
            if (Node != null)
            {
                _CameraEventLabel = Node.GetLabelByName<LinkUpEventLabel>("firefly/computer_vision/camera_event");
                _ImuEventLabel = Node.GetLabelByName<LinkUpEventLabel>("firefly/computer_vision/imu_event");
                _CameraImuEventLabel = Node.GetLabelByName<LinkUpEventLabel>("firefly/computer_vision/camera_imu_event");

                _ExposureLabel = Node.GetLabelByName<LinkUpPropertyLabel<Int16>>("firefly/computer_vision/camera_exposure");

                _ReplayDataSend = Node.GetLabelByName<LinkUpFunctionLabel>("firefly/computer_vision/replay_data");

                _GetRemoteChessboardCorner = Node.GetLabelByName<LinkUpFunctionLabel>("firefly/computer_vision/get_chessboard_corner");

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
                    CameraEventData eventData = CameraEventData.Parse(data, 0, true);
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
                        cameraEventData = CameraEventData.Parse(data, 23, true);

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
            if (ProxyMode == IOProxyMode.Live)
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
            else if (ProxyMode == IOProxyMode.Offline)
            {
                _CameraEventLabel.Unsubscribe();
                _ImuEventLabel.Unsubscribe();
                _CameraImuEventLabel.Unsubscribe();
            }
        }

        public byte[] GetRemoteChessboardCorner(byte[] input)
        {
            if (_GetRemoteChessboardCorner != null)
            {
                return _GetRemoteChessboardCorner.Call(input);
            }
            return null;
        }
    }
}