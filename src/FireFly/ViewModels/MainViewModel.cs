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
    public class MainViewModel : DependencyObject
    {
        public static readonly DependencyProperty CalibrationViewModelProperty =
            DependencyProperty.Register("CalibrationViewModel", typeof(CalibrationViewModel), typeof(MainViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty CameraViewModelProperty =
                    DependencyProperty.Register("CameraViewModel", typeof(CameraViewModel), typeof(MainViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty ConnectivityStateProperty =
                    DependencyProperty.Register("ConnectivityState", typeof(LinkUpConnectivityState), typeof(MainViewModel), new PropertyMetadata(LinkUpConnectivityState.Disconnected));

        public static readonly DependencyProperty DataPlotViewModelProperty =
            DependencyProperty.Register("DataPlotViewModel", typeof(DataPlotViewModel), typeof(MainViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty NodeNameProperty =
            DependencyProperty.Register("NodeName", typeof(string), typeof(MainViewModel), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty NodeProperty =
            DependencyProperty.Register("Node", typeof(LinkUpNode), typeof(MainViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty SettingViewModelProperty =
            DependencyProperty.Register("SettingViewModel", typeof(SettingViewModel), typeof(MainViewModel), new PropertyMetadata(null));

        private readonly SynchronizationContext _SyncContext;

        private LinkUpConnector _Connector;
        private IDialogCoordinator _DialogCoordinator;
        private SettingContainer _SettingContainer = new SettingContainer();

        public MainViewModel()
        {
            SettingContainer.SettingFileName = "config.json";
            SettingContainer.Load();

            _SyncContext = SynchronizationContext.Current;

            CameraViewModel = new CameraViewModel(this);
            SettingViewModel = new SettingViewModel(this);
            DataPlotViewModel = new DataPlotViewModel(this);
            CalibrationViewModel = new CalibrationViewModel(this);

            _DialogCoordinator = MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance;

            SettingsUpdated(false);
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

        internal void SettingsUpdated(bool connectionSettingsChanged)
        {
            if (SettingViewModel != null && CameraViewModel != null)
            {
                CameraViewModel.SettingsUpdated();
                SettingViewModel.SettingsUpdated();
                DataPlotViewModel.SettingsUpdated();
                CalibrationViewModel.SettingsUpdated();

                if (connectionSettingsChanged)
                {
                    if (_Connector != null)
                    {
                        _Connector.Dispose();
                    }

                    _Connector = new LinkUpTcpClientConnector(IPAddress.Parse(SettingViewModel.IpAddress), SettingViewModel.Port);
                    _Connector.ConnectivityChanged += Connector_ConnectivityChanged;

                    Node = new LinkUpNode();
                    Node.Name = NodeName;
                    Node.AddSubNode(Connector);
                    UpdateLinkUpBindings();
                }

                _SettingContainer.Save();
            }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainViewModel mwvm = (d as MainViewModel);
            if (mwvm.Connector == null)
            {
                try
                {
                    mwvm._Connector = new LinkUpTcpClientConnector(IPAddress.Parse(mwvm.SettingViewModel.IpAddress), mwvm.SettingViewModel.Port);
                }
                catch (Exception)
                {
                    mwvm._Connector = new LinkUpTcpClientConnector(IPAddress.Parse("127.0.0.1"), 3000);
                }
                mwvm.Connector.ConnectivityChanged += mwvm.Connector_ConnectivityChanged;
            }

            mwvm.Node = new LinkUpNode();
            mwvm.Node.Name = mwvm.NodeName;
            mwvm.Node.AddSubNode(mwvm.Connector);
            mwvm.UpdateLinkUpBindings();
        }

        private void Connector_ConnectivityChanged(LinkUpConnector connector, LinkUpConnectivityState connectivity)
        {
            _SyncContext.Post(o =>
            {
                ConnectivityState = connectivity;
            }
            , null);
        }

        private void UpdateLinkUpBindings()
        {
            CameraViewModel.UpdateLinkUpBindings();
            SettingViewModel.UpdateLinkUpBindings();
            DataPlotViewModel.UpdateLinkUpBindings();
            CalibrationViewModel.UpdateLinkUpBindings();
        }
    }
}