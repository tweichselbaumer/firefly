using LinkUp.Node;
using LinkUp.Raw;
using System.Net;
using System.Threading;
using System.Windows;

namespace FireFly.ViewModels
{
    public class MainViewModel : DependencyObject
    {
        public static readonly DependencyProperty CameraViewModelProperty =
            DependencyProperty.Register("CameraViewModel", typeof(CameraViewModel), typeof(MainViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty ConnectivityStateProperty =
                    DependencyProperty.Register("ConnectivityState", typeof(LinkUpConnectivityState), typeof(MainViewModel), new PropertyMetadata(LinkUpConnectivityState.Disconnected));

        public static readonly DependencyProperty NodeNameProperty =
                    DependencyProperty.Register("NodeName", typeof(string), typeof(MainViewModel), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty NodeProperty =
            DependencyProperty.Register("Node", typeof(LinkUpNode), typeof(MainViewModel), new PropertyMetadata(null));

        private readonly SynchronizationContext _SyncContext;

        private LinkUpConnector _Connector;

        public MainViewModel()
        {
            _SyncContext = SynchronizationContext.Current;
            CameraViewModel = new CameraViewModel(this);
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

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainViewModel mwvm = (d as MainViewModel);
            if (mwvm.Connector != null)
            {
                mwvm.Connector.Dispose();
            }
            mwvm._Connector = new LinkUpTcpClientConnector(IPAddress.Parse("192.168.1.232"), 3000);
            mwvm.Connector.ConnectivityChanged += mwvm.Connector_ConnectivityChanged;

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
        }
    }
}