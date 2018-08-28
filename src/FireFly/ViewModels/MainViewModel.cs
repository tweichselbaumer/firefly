using FireFly.Proxy;
using FireFly.Settings;
using LinkUp.Node;
using LinkUp.Raw;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Net;
using System.Threading;
using System.Windows;

namespace FireFly.ViewModels
{
    public class MainViewModel : AbstractBaseViewModel
    {
        public static readonly DependencyProperty BytesReceivedPerSecProperty =
            DependencyProperty.Register("BytesReceivedPerSec", typeof(double), typeof(MainViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty BytesSentPerSecProperty =
            DependencyProperty.Register("BytesSentPerSec", typeof(double), typeof(MainViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty CalibrationViewModelProperty =
            DependencyProperty.Register("CalibrationViewModel", typeof(CalibrationViewModel), typeof(MainViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty CameraViewModelProperty =
            DependencyProperty.Register("CameraViewModel", typeof(CameraViewModel), typeof(MainViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty ConfigFileNameProperty =
            DependencyProperty.Register("ConfigFileName", typeof(string), typeof(MainViewModel), new FrameworkPropertyMetadata("config.json", new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty ConnectivityStateProperty =
            DependencyProperty.Register("ConnectivityState", typeof(LinkUpConnectivityState), typeof(MainViewModel), new PropertyMetadata(LinkUpConnectivityState.Disconnected));

        public static readonly DependencyProperty DataPlotViewModelProperty =
            DependencyProperty.Register("DataPlotViewModel", typeof(DataPlotViewModel), typeof(MainViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty NodeNameProperty =
            DependencyProperty.Register("NodeName", typeof(string), typeof(MainViewModel), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty NodeProperty =
            DependencyProperty.Register("Node", typeof(LinkUpNode), typeof(MainViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty RecordViewModelProperty =
            DependencyProperty.Register("RecordViewModel", typeof(RecordViewModel), typeof(MainViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty ReplayViewModelProperty =
            DependencyProperty.Register("ReplayViewModel", typeof(ReplayViewModel), typeof(MainViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty SettingViewModelProperty =
            DependencyProperty.Register("SettingViewModel", typeof(SettingViewModel), typeof(MainViewModel), new PropertyMetadata(null));

        private readonly SynchronizationContext _SyncContext;

        private LinkUpConnector _Connector;

        private IDialogCoordinator _DialogCoordinator;

        private IOProxy _IOProxy;

        private SettingContainer _SettingContainer = new SettingContainer();

        public MainViewModel()
        {
            _IOProxy = new IOProxy(_SettingContainer);

            _SyncContext = SynchronizationContext.Current;

            CameraViewModel = new CameraViewModel(this);
            SettingViewModel = new SettingViewModel(this);
            DataPlotViewModel = new DataPlotViewModel(this);
            CalibrationViewModel = new CalibrationViewModel(this);
            RecordViewModel = new RecordViewModel(this);
            ReplayViewModel = new ReplayViewModel(this);

            _DialogCoordinator = MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance;
        }

        public double BytesReceivedPerSec
        {
            get { return (double)GetValue(BytesReceivedPerSecProperty); }
            set { SetValue(BytesReceivedPerSecProperty, value); }
        }

        public double BytesSentPerSec
        {
            get { return (double)GetValue(BytesSentPerSecProperty); }
            set { SetValue(BytesSentPerSecProperty, value); }
        }

        public CalibrationViewModel CalibrationViewModel
        {
            get { return (CalibrationViewModel)GetValue(CalibrationViewModelProperty); }
            set { SetValue(CalibrationViewModelProperty, value); }
        }

        public CameraViewModel CameraViewModel
        {
            get { return (CameraViewModel)GetValue(CameraViewModelProperty); }
            set { SetValue(CameraViewModelProperty, value); }
        }

        public string ConfigFileName
        {
            get { return (string)GetValue(ConfigFileNameProperty); }
            set { SetValue(ConfigFileNameProperty, value); }
        }

        public LinkUpConnectivityState ConnectivityState
        {
            get { return (LinkUpConnectivityState)GetValue(ConnectivityStateProperty); }
            set { SetValue(ConnectivityStateProperty, value); }
        }

        public LinkUpConnector Connector
        {
            get
            {
                return _Connector;
            }
        }

        public DataPlotViewModel DataPlotViewModel
        {
            get { return (DataPlotViewModel)GetValue(DataPlotViewModelProperty); }
            set { SetValue(DataPlotViewModelProperty, value); }
        }

        public IDialogCoordinator DialogCoordinator
        {
            get
            {
                return _DialogCoordinator;
            }
        }

        public IOProxy IOProxy
        {
            get
            {
                return _IOProxy;
            }
        }

        public LinkUpNode Node
        {
            get { return (LinkUpNode)GetValue(NodeProperty); }
            set { SetValue(NodeProperty, value); }
        }

        public string NodeName
        {
            get { return (string)GetValue(NodeNameProperty); }
            set { SetValue(NodeNameProperty, value); }
        }

        public RecordViewModel RecordViewModel
        {
            get { return (RecordViewModel)GetValue(RecordViewModelProperty); }
            set { SetValue(RecordViewModelProperty, value); }
        }

        public ReplayViewModel ReplayViewModel
        {
            get { return (ReplayViewModel)GetValue(ReplayViewModelProperty); }
            set { SetValue(ReplayViewModelProperty, value); }
        }

        public SettingContainer SettingContainer
        {
            get
            {
                return _SettingContainer;
            }
        }

        public SettingViewModel SettingViewModel
        {
            get { return (SettingViewModel)GetValue(SettingViewModelProperty); }
            set { SetValue(SettingViewModelProperty, value); }
        }

        public SynchronizationContext SyncContext
        {
            get
            {
                return _SyncContext;
            }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();
        }

        internal void UpdateSettings(bool connectionSettingsChanged)
        {
            SettingsUpdated();

            if (connectionSettingsChanged)
            {
                if (Node != null)
                {
                    Node.Dispose();
                    Node = null;
                }

                _Connector = new LinkUpTcpClientConnector(IPAddress.Parse(SettingViewModel.IpAddress), SettingViewModel.Port);
                _Connector.ConnectivityChanged += Connector_ConnectivityChanged;
                _Connector.ConnectivityChanged += IOProxy.Connector_ConnectivityChanged;
                _Connector.MetricUpdate += Connector_MetricUpdate;

                Node = new LinkUpNode();
                Node.Name = NodeName;
                Node.AddSubNode(Connector);
                IOProxy.Node = Node;

                IOProxy.UpdateLinkUpBindings();
            }

            _SettingContainer.Save();

            _IOProxy.UpdateSettings();
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainViewModel mwvm = (d as MainViewModel);
            switch (e.Property.Name)
            {
                case "ConfigFileName":
                    mwvm.SettingContainer.SettingFileName = mwvm.ConfigFileName;
                    mwvm.SettingContainer.Load();
                    mwvm.UpdateSettings(true);
                    break;

                default:

                    if (mwvm.Node != null)
                    {
                        mwvm.Node.Dispose();
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(mwvm.SettingViewModel.IpAddress))
                        {
                            mwvm._Connector = new LinkUpTcpClientConnector(IPAddress.Parse(mwvm.SettingViewModel.IpAddress), mwvm.SettingViewModel.Port);

                            mwvm.Connector.ConnectivityChanged += mwvm.Connector_ConnectivityChanged;
                            mwvm.Connector.ConnectivityChanged += mwvm.IOProxy.Connector_ConnectivityChanged;
                            mwvm.Connector.MetricUpdate += mwvm.Connector_MetricUpdate;

                            mwvm.Node = new LinkUpNode();
                            mwvm.Node.Name = mwvm.NodeName;
                            mwvm.Node.AddSubNode(mwvm.Connector);
                            mwvm.IOProxy.Node = mwvm.Node;
                            mwvm.IOProxy.UpdateLinkUpBindings();
                        }
                    }
                    catch (Exception)
                    {
                    }
                    break;
            }
        }

        private void Connector_ConnectivityChanged(LinkUpConnector connector, LinkUpConnectivityState connectivity)
        {
            _SyncContext.Post(o =>
            {
                ConnectivityState = connectivity;
            }
            , null);
        }

        private void Connector_MetricUpdate(LinkUpConnector connector, double bytesSentPerSecond, double bytesReceivedPerSecond)
        {
            _SyncContext.Post(o =>
            {
                BytesSentPerSec = bytesSentPerSecond;
                BytesReceivedPerSec = bytesReceivedPerSecond;
            }
            , null);
        }
    }
}