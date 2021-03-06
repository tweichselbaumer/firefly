﻿using FireFly.Command;
using FireFly.Proxy;
using FireFly.Utilities;
using FireFly.VI.SLAM.Visualisation;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace FireFly.ViewModels
{
    public class VisualisationViewModel : AbstractViewModel, IProxyEventSubscriber
    {
        public static readonly DependencyProperty EnableVisualInertialProperty =
            DependencyProperty.Register("EnableVisualInertial", typeof(bool), typeof(VisualisationViewModel), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty FPSProperty =
                    DependencyProperty.Register("FPS", typeof(int), typeof(VisualisationViewModel), new PropertyMetadata(0));

        public static readonly DependencyProperty ReproducibleExecutionProperty =
            DependencyProperty.Register("ReproducibleExecution", typeof(bool), typeof(VisualisationViewModel), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty ShowKeyFrameOrientationsProperty =
            DependencyProperty.Register("ShowKeyFrameOrientations", typeof(bool), typeof(VisualisationViewModel), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty StatusProperty =
                    DependencyProperty.Register("Status", typeof(SlamOperationStatus), typeof(VisualisationViewModel), new PropertyMetadata(SlamOperationStatus.Stopped));

        private FPSCounter _FPSCounter = new FPSCounter();

        private SlamModel3D _SlamModel3D;

        private Timer _Timer;

        public VisualisationViewModel(MainViewModel parent) : base(parent)
        {
            _SlamModel3D = new SlamModel3D(Parent.SyncContext);
            Parent.IOProxy.Subscribe(this, ProxyEventType.SlamMapEvent);
            _Timer = new Timer(300);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
            Parent.IOProxy.Subscribe(this, ProxyEventType.SlamStatusEvent);
        }

        public bool EnableVisualInertial
        {
            get { return (bool)GetValue(EnableVisualInertialProperty); }
            set { SetValue(EnableVisualInertialProperty, value); }
        }

        public RelayCommand<object> ExportCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoExport(o);
                    });
            }
        }

        public int FPS
        {
            get { return (int)GetValue(FPSProperty); }
            set { SetValue(FPSProperty, value); }
        }

        public bool ReproducibleExecution
        {
            get { return (bool)GetValue(ReproducibleExecutionProperty); }
            set { SetValue(ReproducibleExecutionProperty, value); }
        }

        public bool ShowKeyFrameOrientations
        {
            get { return (bool)GetValue(ShowKeyFrameOrientationsProperty); }
            set { SetValue(ShowKeyFrameOrientationsProperty, value); }
        }

        public SlamModel3D SlamModel3D
        {
            get
            {
                return _SlamModel3D;
            }
        }

        public RelayCommand<object> StartCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoStart(o);
                    });
            }
        }

        public SlamOperationStatus Status
        {
            get { return (SlamOperationStatus)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public RelayCommand<object> StopCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoStop(o);
                    });
            }
        }

        public void Fired(IOProxy proxy, List<AbstractProxyEventData> eventData)
        {
            SlamMapEventData slamMapEvent = (SlamMapEventData)eventData.FirstOrDefault(c => c is SlamMapEventData);
            SlamStatusEventData slamStatusEvent = (SlamStatusEventData)eventData.FirstOrDefault(c => c is SlamStatusEventData);
            if (slamMapEvent != null)
            {
                if (slamMapEvent.PublishType == SlamPublishType.Frame)
                {
                    SlamModel3D.AddNewFrame(slamMapEvent.Frame);
                    _FPSCounter.CountFrame();
                }
                else if (slamMapEvent.PublishType == SlamPublishType.KeyframeWithPoints)
                {
                    SlamModel3D.AddNewKeyFrame(slamMapEvent.KeyFrame);
                }
                else if (slamMapEvent.PublishType == SlamPublishType.Reset)
                {
                    SlamModel3D.Reset();
                }
            }
            if (slamStatusEvent != null)
            {
                Parent.SyncContext.Post(o =>
                {
                    Status = slamStatusEvent.Status;
                }, null);
            }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();

            ReproducibleExecution = Parent.SettingContainer.Settings.SlamSettings.ReproducibleExecution;
            ShowKeyFrameOrientations = Parent.SettingContainer.Settings.SlamSettings.ShowKeyFrameOrientations;
            EnableVisualInertial = Parent.SettingContainer.Settings.SlamSettings.EnableVisualInertial;
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VisualisationViewModel vvm = (d as VisualisationViewModel);
            bool changed = false;
            bool connectionSettingsChanged = false;
            switch (e.Property.Name)
            {
                case "ReproducibleExecution":
                    changed = vvm.Parent.SettingContainer.Settings.SlamSettings.ReproducibleExecution != vvm.ReproducibleExecution;
                    vvm.Parent.SettingContainer.Settings.SlamSettings.ReproducibleExecution = vvm.ReproducibleExecution;
                    connectionSettingsChanged = false;
                    break;

                case "EnableVisualInertial":
                    changed = vvm.Parent.SettingContainer.Settings.SlamSettings.EnableVisualInertial != vvm.EnableVisualInertial;
                    vvm.Parent.SettingContainer.Settings.SlamSettings.EnableVisualInertial = vvm.EnableVisualInertial;
                    connectionSettingsChanged = false;
                    break;

                case "ShowKeyFrameOrientations":
                    changed = vvm.Parent.SettingContainer.Settings.SlamSettings.ShowKeyFrameOrientations != vvm.ShowKeyFrameOrientations;
                    vvm.Parent.SettingContainer.Settings.SlamSettings.ShowKeyFrameOrientations = vvm.ShowKeyFrameOrientations;
                    connectionSettingsChanged = false;
                    vvm.SlamModel3D.ShowKeyFrameOrientations = vvm.ShowKeyFrameOrientations;
                    break;

                default:
                    break;
            }
            if (changed)
            {
                vvm.Parent.UpdateSettings(connectionSettingsChanged);
            }
        }

        private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Parent.SyncContext.Post(o =>
            {
                FPS = (int)_FPSCounter.FramesPerSecond;
            }, null);
        }

        private Task DoExport(object o)
        {
            return Task.Run(async () =>
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = null;
                bool save = false;

                Parent.SyncContext.Send(c =>
                {
                    saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                    saveFileDialog.Filter = "Matlab (*.mat) | *.mat";
                    save = saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;
                }, null);

                if (save)
                {
                    MetroDialogSettings settings = new MetroDialogSettings()
                    {
                        AnimateShow = false,
                        AnimateHide = false
                    };

                    var controller = await Parent.DialogCoordinator.ShowProgressAsync(Parent, "Please wait...", "Export data to Matlab!", settings: Parent.MetroDialogSettings);

                    SlamModel3D.ExportToMatlab(saveFileDialog.FileName);

                    controller.SetCancelable(false);

                    await controller.CloseAsync();
                }
            });
        }

        private Task DoStart(object o)
        {
            return Task.Run(() =>
            {
                Parent.IOProxy.ChangeSlamStatus(SlamStatusOverall.Start);
            });
        }

        private Task DoStop(object o)
        {
            return Task.Run(() =>
            {
                Parent.IOProxy.ChangeSlamStatus(SlamStatusOverall.Stop);
            });
        }
    }
}