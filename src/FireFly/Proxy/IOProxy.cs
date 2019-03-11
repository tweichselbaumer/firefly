using FireFly.Data.Storage;
using FireFly.Settings;
using LinkUp.Node;
using LinkUp.Raw;
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
        private LinkUpPropertyLabel<Double> _AccelerometerNoiseLabel;
        private LinkUpPropertyLabel<Double> _AccelerometerScaleLabel;
        private LinkUpPropertyLabel<Double> _AccelerometerWalkLabel;
        private BlockingCollection<Tuple<ImuEventData, CameraEventData>> _BackgroundQueue = new BlockingCollection<Tuple<ImuEventData, CameraEventData>>();
        private Task _BackgroundTask;
        private LinkUpEventLabel _CameraEventLabel;
        private LinkUpEventLabel _CameraImuEventLabel;
        private LinkUpPropertyLabel<Int16> _ExposureLabel;
        private LinkUpFunctionLabel _GetRemoteChessboardCornerLabel;
        private LinkUpPropertyLabel<Double> _GyroscopeNoiseLabel;
        private LinkUpPropertyLabel<Double> _GyroscopeScaleLabel;
        private LinkUpPropertyLabel<Double> _GyroscopeWalkLabel;
        private LinkUpEventLabel _ImuDerivedEventLabel;
        private LinkUpEventLabel _ImuEventLabel;
        private LinkUpPropertyLabel_Binary _ImuFilterALabel;
        private LinkUpPropertyLabel_Binary _ImuFilterBLabel;
        private LinkUpPropertyLabel_Binary _MInvAccLabel;
        private LinkUpPropertyLabel_Binary _MInvGyroLabel;
        private LinkUpNode _Node;
        private IOProxyMode _ProxyMode = IOProxyMode.Live;
        private LinkUpPropertyLabel_Binary _RAccGyroLabel;
        private LinkUpPropertyLabel<Boolean> _RecordRemoteLabel;
        private LinkUpFunctionLabel _ReplayDataSend;
        private bool _Running;
        private SettingContainer _SettingContainer;
        private LinkUpFunctionLabel _SlamChangeStatusLabel;
        private LinkUpEventLabel _SlamMapEventLabel;
        private LinkUpPropertyLabel<Boolean> _SlamReproducibleExecutionLabel;
        private LinkUpEventLabel _SlamStatusEventLabel;
        private List<Tuple<IProxyEventSubscriber, ProxyEventType>> _Subscriptions = new List<Tuple<IProxyEventSubscriber, ProxyEventType>>();
        private List<Task> _Tasks = new List<Task>();
        private LinkUpPropertyLabel_Binary _TCamImuLabel;
        private LinkUpPropertyLabel<Double> _TemperatureOffsetLabel;
        private LinkUpPropertyLabel<Double> _TemperatureScaleLabel;
        private LinkUpFunctionLabel _UpdateSettingsLabel;
        private long lastTimestamp = 0;

        public IOProxy(SettingContainer settingContainer)
        {
            _SettingContainer = settingContainer;
            _Running = true;
            _BackgroundTask = Task.Factory.StartNew(() =>
            {
                DoWork();
            }, TaskCreationOptions.LongRunning);
        }

        public LinkUpConnectivityState ConnectivityState
        {
            get
            {
                LinkUpConnectivityState linkUpConnectivityState = LinkUpConnectivityState.Disconnected;
                if (Node != null)
                {
                    if (Node.SubNodes != null && Node.SubNodes.Count > 0)
                    {
                        linkUpConnectivityState = Node.SubNodes[0].Connector.ConnectivityState;
                    }
                }
                return linkUpConnectivityState;
            }
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

                Task.Run(() =>
                {
                    lock (_Subscriptions)
                    {
                        UpdateSubscription();
                    }
                });
            }
        }

        public void ChangeSlamStatus(SlamStatusOverall slamStatus, bool wait = false)
        {
            try
            {
                if (_SlamChangeStatusLabel != null && ConnectivityState == LinkUpConnectivityState.Connected)
                {
                    if (wait)
                        _SlamChangeStatusLabel.Call(new byte[] { (byte)slamStatus });
                    else
                        _SlamChangeStatusLabel.AsyncCall(new byte[] { (byte)slamStatus });
                }
            }
            catch (Exception) { }
        }

        public void Connector_ConnectivityChanged(LinkUpConnector connector, LinkUpConnectivityState connectivity)
        {
            if (connectivity == LinkUpConnectivityState.Connected)
            {
                Update();
            }
        }

        public void Dispose()
        {
            _Running = false;
            _BackgroundTask.Wait();
            _Tasks.ForEach(c => c.Wait());
            //TODO: wait for tasks
        }

        public byte[] GetRemoteChessboardCorner(byte[] input)
        {
            if (_GetRemoteChessboardCornerLabel != null)
            {
                return _GetRemoteChessboardCornerLabel.Call(input);
            }
            return null;
        }

        public void ReplayOffline(RawDataReader reader, Action<TimeSpan> updateTime, Action onClose, Func<bool> isPaused, Func<bool> isStopped)
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
                    while (isPaused())
                    {
                        watch.Stop();
                        Thread.Sleep(500);
                        watch.Start();
                    }
                    if (isStopped())
                    {
                        break;
                    }

                    Tuple<long, List<Tuple<RawReaderMode, object>>> res = reader.Next();
                    if (startTime == -1)
                        startTime = res.Item1;

                    ImuEventData imuEventData = null;
                    CameraEventData cameraEventData = null;

                    int rawSize = 0;
                    byte[] rawImage = null;
                    byte[] rawImu = null;
                    double exposureTime = 0.0;

                    foreach (Tuple<RawReaderMode, object> val in res.Item2)
                    {
                        if (val.Item1 == RawReaderMode.Imu0)
                        {
                            imuEventData = ImuEventData.Parse(res.Item1, (Tuple<double, double, double, double, double, double>)val.Item2, res.Item2.Any(c => c.Item1 == RawReaderMode.Camera0));
                            rawSize += imuEventData.RawSize;
                            rawImu = imuEventData.GetRaw(_SettingContainer.Settings.ImuSettings.GyroscopeScale, _SettingContainer.Settings.ImuSettings.AccelerometerScale, _SettingContainer.Settings.ImuSettings.TemperatureScale, _SettingContainer.Settings.ImuSettings.TemperatureOffset);
                        }
                        if (val.Item1 == RawReaderMode.Camera0)
                        {
                            cameraEventData = CameraEventData.Parse(((Tuple<double, byte[]>)val.Item2).Item2, 0, false, ((Tuple<double, byte[]>)val.Item2).Item1);
                            rawSize += cameraEventData.RawSize;
                            rawImage = ((Tuple<double, byte[]>)val.Item2).Item2;
                            exposureTime = ((Tuple<double, byte[]>)val.Item2).Item1;
                        }
                    }
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                    {
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
                    }
                    else
                        _BackgroundQueue.Add(new Tuple<ImuEventData, CameraEventData>(imuEventData, cameraEventData));

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

        public void SetExposure(Int16 exposure)
        {
            if (_ExposureLabel != null)
            {
                _ExposureLabel.Value = exposure;
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

                _SlamMapEventLabel = Node.GetLabelByName<LinkUpEventLabel>("firefly/computer_vision/slam_map_event");

                _ExposureLabel = Node.GetLabelByName<LinkUpPropertyLabel<Int16>>("firefly/computer_vision/camera_exposure");

                _ReplayDataSend = Node.GetLabelByName<LinkUpFunctionLabel>("firefly/computer_vision/replay_data");

                _GetRemoteChessboardCornerLabel = Node.GetLabelByName<LinkUpFunctionLabel>("firefly/computer_vision/get_chessboard_corner");

                _UpdateSettingsLabel = Node.GetLabelByName<LinkUpFunctionLabel>("firefly/computer_vision/update_settings");

                _AccelerometerScaleLabel = Node.GetLabelByName<LinkUpPropertyLabel<Double>>("firefly/computer_vision/acc_scale");
                _GyroscopeScaleLabel = Node.GetLabelByName<LinkUpPropertyLabel<Double>>("firefly/computer_vision/gyro_scale");
                _TemperatureScaleLabel = Node.GetLabelByName<LinkUpPropertyLabel<Double>>("firefly/computer_vision/temp_scale");
                _TemperatureOffsetLabel = Node.GetLabelByName<LinkUpPropertyLabel<Double>>("firefly/computer_vision/temp_offset");

                _RecordRemoteLabel = Node.GetLabelByName<LinkUpPropertyLabel<Boolean>>("firefly/computer_vision/record_remote");

                _ImuFilterALabel = Node.GetLabelByName<LinkUpPropertyLabel_Binary>("firefly/computer_vision/imu_filter_a");
                _ImuFilterBLabel = Node.GetLabelByName<LinkUpPropertyLabel_Binary>("firefly/computer_vision/imu_filter_b");
                _ImuDerivedEventLabel = Node.GetLabelByName<LinkUpEventLabel>("firefly/computer_vision/imu_derived_event");

                _SlamStatusEventLabel = Node.GetLabelByName<LinkUpEventLabel>("firefly/computer_vision/slam_status_event");
                _SlamChangeStatusLabel = Node.GetLabelByName<LinkUpFunctionLabel>("firefly/computer_vision/slam_change_status");

                _SlamReproducibleExecutionLabel = Node.GetLabelByName<LinkUpPropertyLabel<Boolean>>("firefly/computer_vision/slam_reproducible_execution");

                _TCamImuLabel = Node.GetLabelByName<LinkUpPropertyLabel_Binary>("firefly/computer_vision/calibration_T_cam_imu");
                _RAccGyroLabel = Node.GetLabelByName<LinkUpPropertyLabel_Binary>("firefly/computer_vision/calibration_R_acc_gyro");
                _MInvGyroLabel = Node.GetLabelByName<LinkUpPropertyLabel_Binary>("firefly/computer_vision/calibration_M_inv_gyro");
                _MInvAccLabel = Node.GetLabelByName<LinkUpPropertyLabel_Binary>("firefly/computer_vision/calibration_M_inv_acc");
                _GyroscopeNoiseLabel = Node.GetLabelByName<LinkUpPropertyLabel<Double>>("firefly/computer_vision/calibration_gyro_noise");
                _GyroscopeWalkLabel = Node.GetLabelByName<LinkUpPropertyLabel<Double>>("firefly/computer_vision/calibration_gyro_walk");
                _AccelerometerNoiseLabel = Node.GetLabelByName<LinkUpPropertyLabel<Double>>("firefly/computer_vision/calibration_acc_noise");
                _AccelerometerWalkLabel = Node.GetLabelByName<LinkUpPropertyLabel<Double>>("firefly/computer_vision/calibration_acc_walk");

                _ImuDerivedEventLabel.Fired += _ImuFilterEvent_Fired;

                _SlamStatusEventLabel.Fired += _SlamStatusEventLabel_Fired;

                _CameraEventLabel.Fired += _CameraEventLabel_Fired;
                _ImuEventLabel.Fired += _CameraEventLabel_Fired;
                _CameraImuEventLabel.Fired += _CameraEventLabel_Fired;

                _SlamMapEventLabel.Fired += _SlamMapEventLabel_Fired;

                Update();
            }
        }

        public void UpdateSettings()
        {
            if (_AccelerometerScaleLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _AccelerometerScaleLabel.Value = _SettingContainer.Settings.ImuSettings.AccelerometerScale;
                }
                catch (Exception) { }
            }
            if (_GyroscopeScaleLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _GyroscopeScaleLabel.Value = _SettingContainer.Settings.ImuSettings.GyroscopeScale;
                }
                catch (Exception) { }
            }
            if (_TemperatureScaleLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _TemperatureScaleLabel.Value = _SettingContainer.Settings.ImuSettings.TemperatureScale;
                }
                catch (Exception) { }
            }
            if (_TemperatureOffsetLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _TemperatureOffsetLabel.Value = _SettingContainer.Settings.ImuSettings.TemperatureOffset;
                }
                catch (Exception) { }
            }

            if (_RecordRemoteLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _RecordRemoteLabel.Value = _SettingContainer.Settings.ImuSettings.RecordRemote;
                }
                catch (Exception) { }
            }

            if (_SlamReproducibleExecutionLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _SlamReproducibleExecutionLabel.Value = _SettingContainer.Settings.SlamSettings.ReproducibleExecution;
                }
                catch (Exception) { }
            }

            if (_AccelerometerNoiseLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _AccelerometerNoiseLabel.Value = _SettingContainer.Settings.CalibrationSettings.ImuCalibration.AccelerometerNoiseDensity;
                }
                catch (Exception) { }
            }

            if (_AccelerometerWalkLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _AccelerometerWalkLabel.Value = _SettingContainer.Settings.CalibrationSettings.ImuCalibration.AccelerometerRandomWalk;
                }
                catch (Exception) { }
            }

            if (_GyroscopeNoiseLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _GyroscopeNoiseLabel.Value = _SettingContainer.Settings.CalibrationSettings.ImuCalibration.GyroscopeNoiseDensity;
                }
                catch (Exception) { }
            }

            if (_GyroscopeWalkLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _GyroscopeWalkLabel.Value = _SettingContainer.Settings.CalibrationSettings.ImuCalibration.GyroscopeRandomWalk;
                }
                catch (Exception) { }
            }

            if (_TCamImuLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _TCamImuLabel.Value = DoubleArrayArrayToByteArray(_SettingContainer.Settings.CalibrationSettings.ExtrinsicCalibrationSettings.T_Cam_Imu);
                }
                catch (Exception) { }
            }

            if (_MInvGyroLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _MInvGyroLabel.Value = DoubleArrayArrayToByteArray(_SettingContainer.Settings.CalibrationSettings.ExtrinsicCalibrationSettings.M_Inv_Gyro);
                }
                catch (Exception) { }
            }

            if (_RAccGyroLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _RAccGyroLabel.Value = DoubleArrayArrayToByteArray(_SettingContainer.Settings.CalibrationSettings.ExtrinsicCalibrationSettings.R_Acc_Gyro);
                }
                catch (Exception) { }
            }

            if (_MInvAccLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _MInvAccLabel.Value = DoubleArrayArrayToByteArray(_SettingContainer.Settings.CalibrationSettings.ExtrinsicCalibrationSettings.M_Inv_Acc);
                }
                catch (Exception) { }
            }

            if (_UpdateSettingsLabel != null)
            {
                try
                {
                    if (ConnectivityState == LinkUpConnectivityState.Connected)
                        _UpdateSettingsLabel.AsyncCall(new byte[] { });
                }
                catch (Exception) { }
            }
        }

        private void _CameraEventLabel_Fired(LinkUpEventLabel label, byte[] data)
        {
            List<Tuple<IProxyEventSubscriber, ProxyEventType>> subscriberImu = null;
            List<Tuple<IProxyEventSubscriber, ProxyEventType>> subscriberCamImu = null;
            List<Tuple<IProxyEventSubscriber, ProxyEventType>> subscriberCam = null;

            lock (_Subscriptions)
            {
                subscriberCam = _Subscriptions.Where(c => c.Item2 == ProxyEventType.CameraEvent).ToList();
                subscriberImu = _Subscriptions.Where(c => c.Item2 == ProxyEventType.ImuEvent).ToList();
                subscriberCamImu = _Subscriptions.Where(c => c.Item2 == ProxyEventType.CameraImuEvent).ToList();
            }

            if (label == _CameraEventLabel && subscriberCam != null && subscriberCam.Count() > 0)
            {
                CameraEventData eventData = CameraEventData.Parse(data, 0, true);
                foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in subscriberCam)
                {
                    t.Item1.Fired(this, new List<AbstractProxyEventData>() { eventData });
                }
            }
            else if (label == _ImuEventLabel && subscriberImu != null && subscriberImu.Count() > 0)
            {
                ImuEventData eventData = ImuEventData.Parse(data, 0, _SettingContainer.Settings.ImuSettings.GyroscopeScale, _SettingContainer.Settings.ImuSettings.AccelerometerScale, _SettingContainer.Settings.ImuSettings.TemperatureScale, _SettingContainer.Settings.ImuSettings.TemperatureOffset);
                foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in subscriberImu)
                {
                    t.Item1.Fired(this, new List<AbstractProxyEventData>() { eventData });
                }
            }
            else if (label == _CameraImuEventLabel)
            {
                ImuEventData imuEventData = ImuEventData.Parse(data, 0, _SettingContainer.Settings.ImuSettings.GyroscopeScale, _SettingContainer.Settings.ImuSettings.AccelerometerScale, _SettingContainer.Settings.ImuSettings.TemperatureScale, _SettingContainer.Settings.ImuSettings.TemperatureOffset);
                CameraEventData cameraEventData = null;

                if (imuEventData.HasCameraImage)
                    cameraEventData = CameraEventData.Parse(data, 23, true);

                if (subscriberCamImu != null && subscriberCamImu.Count() > 0)
                {
                    foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in subscriberCamImu)
                    {
                        if (cameraEventData != null)
                            t.Item1.Fired(this, new List<AbstractProxyEventData>() { cameraEventData, imuEventData });
                        else
                            t.Item1.Fired(this, new List<AbstractProxyEventData>() { imuEventData });
                    }
                }
                if (subscriberCam != null && subscriberCam.Count() > 0)
                {
                    foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in subscriberCam)
                    {
                        if (cameraEventData != null)
                            t.Item1.Fired(this, new List<AbstractProxyEventData>() { cameraEventData });
                    }
                }
                if (subscriberImu != null && subscriberImu.Count() > 0)
                {
                    foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in subscriberImu)
                    {
                        t.Item1.Fired(this, new List<AbstractProxyEventData>() { imuEventData });
                    }
                }
                lastTimestamp = imuEventData.TimeNanoSeconds;
            }
        }

        private void _ImuFilterEvent_Fired(LinkUpEventLabel label, byte[] data)
        {
            List<Tuple<IProxyEventSubscriber, ProxyEventType>> subscriber = null;

            lock (_Subscriptions)
            {
                subscriber = _Subscriptions.Where(c => c.Item2 == ProxyEventType.ImuDerivedEvent).ToList();
            }

            if (label == _ImuDerivedEventLabel && subscriber != null && subscriber.Count() > 0)
            {
                ImuDerivedEventData eventData = ImuDerivedEventData.Parse(data, 0);
                foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in subscriber)
                {
                    t.Item1.Fired(this, new List<AbstractProxyEventData>() { eventData });
                }
            }
        }

        private void _SlamMapEventLabel_Fired(LinkUpEventLabel label, byte[] data)
        {
            List<Tuple<IProxyEventSubscriber, ProxyEventType>> subscriber = null;

            lock (_Subscriptions)
            {
                subscriber = _Subscriptions.Where(c => c.Item2 == ProxyEventType.SlamMapEvent).ToList();
            }

            if (label == _SlamMapEventLabel && subscriber != null && subscriber.Count() > 0)
            {
                SlamMapEventData eventData = SlamMapEventData.Parse(data);
                foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in subscriber)
                {
                    t.Item1.Fired(this, new List<AbstractProxyEventData>() { eventData });
                }
            }
        }

        private void _SlamStatusEventLabel_Fired(LinkUpEventLabel label, byte[] data)
        {
            List<Tuple<IProxyEventSubscriber, ProxyEventType>> subscriber = null;

            lock (_Subscriptions)
            {
                subscriber = _Subscriptions.Where(c => c.Item2 == ProxyEventType.SlamStatusEvent).ToList();
            }

            if (label == _SlamStatusEventLabel && subscriber != null && subscriber.Count() > 0)
            {
                SlamStatusEventData eventData = SlamStatusEventData.Parse(data);
                foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in subscriber)
                {
                    t.Item1.Fired(this, new List<AbstractProxyEventData>() { eventData });
                }
            }
        }

        private byte[] DoubleArrayArrayToByteArray(double[,] array)
        {
            byte[] data = new byte[array.GetLength(0) * array.GetLength(1) * sizeof(double)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    byte[] temp = BitConverter.GetBytes(array[i, j]);
                    Array.Copy(temp, 0, data, (i * array.GetLength(1) + j) * sizeof(double), sizeof(double));
                }
            }
            return data;
        }

        private void DoWork()
        {
            while (_Running)
            {
                Tuple<ImuEventData, CameraEventData> next;

                if (_BackgroundQueue.TryTake(out next, 100))
                {
                    ImuEventData imuEventData = next.Item1;
                    CameraEventData cameraEventData = next.Item2;

                    List<Tuple<IProxyEventSubscriber, ProxyEventType>> subscriberCamImu = null;
                    List<Tuple<IProxyEventSubscriber, ProxyEventType>> subscriberCam = null;
                    List<Tuple<IProxyEventSubscriber, ProxyEventType>> subscriberImu = null;

                    lock (_Subscriptions)
                    {
                        subscriberCamImu = _Subscriptions.Where(c => c.Item2 == ProxyEventType.CameraImuEvent).ToList();
                        subscriberCam = _Subscriptions.Where(c => c.Item2 == ProxyEventType.CameraEvent).ToList();
                        subscriberImu = _Subscriptions.Where(c => c.Item2 == ProxyEventType.ImuEvent).ToList();
                    }

                    foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in subscriberCamImu)
                    {
                        if (cameraEventData != null)
                            t.Item1.Fired(this, new List<AbstractProxyEventData>() { cameraEventData, imuEventData });
                        else
                        {
                            if (imuEventData != null)
                                t.Item1.Fired(this, new List<AbstractProxyEventData>() { imuEventData });
                        }
                    }

                    foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in subscriberCam)
                    {
                        if (cameraEventData != null)
                            t.Item1.Fired(this, new List<AbstractProxyEventData>() { cameraEventData });
                    }

                    foreach (Tuple<IProxyEventSubscriber, ProxyEventType> t in subscriberImu)
                    {
                        if (imuEventData != null)
                            t.Item1.Fired(this, new List<AbstractProxyEventData>() { imuEventData });
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void Update()
        {
            try
            {
                lock (_Subscriptions)
                {
                    UpdateSubscription();
                }
                UpdateSettings();
            }
            catch (Exception ex)
            {
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
                        try
                        {
                            if (ConnectivityState == LinkUpConnectivityState.Connected)
                                _CameraImuEventLabel.Subscribe();
                        }
                        catch (Exception) { }
                        try
                        {
                            if (ConnectivityState == LinkUpConnectivityState.Connected)
                                _ImuEventLabel.Unsubscribe();
                        }
                        catch (Exception) { }
                        try
                        {
                            if (ConnectivityState == LinkUpConnectivityState.Connected)
                                _CameraEventLabel.Unsubscribe();
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        try
                        {
                            if (ConnectivityState == LinkUpConnectivityState.Connected)
                                _CameraImuEventLabel.Unsubscribe();
                        }
                        catch (Exception) { }
                        if (_Subscriptions.Any(c => c.Item2 == ProxyEventType.CameraEvent))
                        {
                            try
                            {
                                if (ConnectivityState == LinkUpConnectivityState.Connected)
                                    _CameraEventLabel.Subscribe();
                            }
                            catch (Exception) { }
                        }
                        else
                        {
                            try
                            {
                                if (ConnectivityState == LinkUpConnectivityState.Connected)
                                    _CameraEventLabel.Unsubscribe();
                            }
                            catch (Exception) { }
                        }
                        if (_Subscriptions.Any(c => c.Item2 == ProxyEventType.ImuEvent))
                        {
                            try
                            {
                                if (ConnectivityState == LinkUpConnectivityState.Connected)
                                    _ImuEventLabel.Subscribe();
                            }
                            catch (Exception) { }
                        }
                        else
                        {
                            try
                            {
                                if (ConnectivityState == LinkUpConnectivityState.Connected)
                                    _ImuEventLabel.Unsubscribe();
                            }
                            catch (Exception) { }
                        }
                    }
                }
            }
            //else if (ProxyMode == IOProxyMode.Offline)
            //{
            //    try
            //    {
            //        if (ConnectivityState == LinkUpConnectivityState.Connected)
            //            _CameraEventLabel.Unsubscribe();
            //    }
            //    catch (Exception) { }
            //    try
            //    {
            //        if (ConnectivityState == LinkUpConnectivityState.Connected)
            //            _ImuEventLabel.Unsubscribe();
            //    }
            //    catch (Exception) { }
            //    try
            //    {
            //        if (ConnectivityState == LinkUpConnectivityState.Connected)
            //            _CameraImuEventLabel.Unsubscribe();
            //    }
            //    catch (Exception) { }
            //}

            if (_SlamMapEventLabel != null)
            {
                if (_Subscriptions.Any(c => c.Item2 == ProxyEventType.SlamMapEvent))
                {
                    try
                    {
                        if (ConnectivityState == LinkUpConnectivityState.Connected)
                            _SlamMapEventLabel.Subscribe();
                    }
                    catch (Exception) { }
                }
                else
                {
                    try
                    {
                        if (ConnectivityState == LinkUpConnectivityState.Connected)
                            _SlamMapEventLabel.Unsubscribe();
                    }
                    catch (Exception) { }
                }
            }

            if (_SlamStatusEventLabel != null)
            {
                if (_Subscriptions.Any(c => c.Item2 == ProxyEventType.SlamStatusEvent))
                {
                    try
                    {
                        if (ConnectivityState == LinkUpConnectivityState.Connected)
                            _SlamStatusEventLabel.Subscribe();
                    }
                    catch (Exception) { }
                }
                else
                {
                    try
                    {
                        if (ConnectivityState == LinkUpConnectivityState.Connected)
                            _SlamStatusEventLabel.Unsubscribe();
                    }
                    catch (Exception) { }
                }
            }

            if (_ImuDerivedEventLabel != null)
            {
                if (_Subscriptions.Any(c => c.Item2 == ProxyEventType.ImuDerivedEvent))
                {
                    try
                    {
                        if (ConnectivityState == LinkUpConnectivityState.Connected)
                            _ImuDerivedEventLabel.Subscribe();
                    }
                    catch (Exception) { }
                }
                else
                {
                    try
                    {
                        if (ConnectivityState == LinkUpConnectivityState.Connected)
                            _ImuDerivedEventLabel.Unsubscribe();
                    }
                    catch (Exception) { }
                }
            }
        }
    }
}