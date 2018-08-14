using FireFly.Command;
using FireFly.Data.Storage;
using FireFly.Models;
using FireFly.Settings;
using FireFly.Utilities;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace FireFly.ViewModels
{
    public class ReplayViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty FilesForReplayProperty =
            DependencyProperty.Register("FilesForReplay", typeof(RangeObservableCollection<Tuple<FileLocation, List<ReplayFile>>>), typeof(ReplayViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty IsReplayingProperty =
            DependencyProperty.Register("IsReplaying", typeof(bool), typeof(ReplayViewModel), new PropertyMetadata(false));

        public static readonly DependencyProperty ReplayTimeProperty =
            DependencyProperty.Register("ReplayTime", typeof(TimeSpan), typeof(ReplayViewModel), new PropertyMetadata(null));

        public ReplayViewModel(MainViewModel parent) : base(parent)
        {
            FilesForReplay = new RangeObservableCollection<Tuple<FileLocation, List<ReplayFile>>>();
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

        public RangeObservableCollection<Tuple<FileLocation, List<ReplayFile>>> FilesForReplay
        {
            get { return (RangeObservableCollection<Tuple<FileLocation, List<ReplayFile>>>)GetValue(FilesForReplayProperty); }
            set { SetValue(FilesForReplayProperty, value); }
        }

        public bool IsReplaying
        {
            get { return (bool)GetValue(IsReplayingProperty); }
            set { SetValue(IsReplayingProperty, value); }
        }

        public RelayCommand<object> RefreshCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoRefresh(o);
                    });
            }
        }

        public TimeSpan ReplayTime
        {
            get { return (TimeSpan)GetValue(ReplayTimeProperty); }
            set { SetValue(ReplayTimeProperty, value); }
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

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();
            Refresh();
        }

        private Task DoExport(object o)
        {
            return Task.Factory.StartNew(async () =>
            {
                ReplayFile file = o as ReplayFile;
                DataReader reader = null;

                string fullPath = null;
                Parent.SyncContext.Send(c =>
                {
                    fullPath = file.FullPath;
                }, null);

                reader = new DataReader(fullPath, ReaderMode.Imu0 | ReaderMode.Camera0);

                reader.Open();

                MetroDialogSettings settings = new MetroDialogSettings()
                {
                    AnimateShow = false,
                    AnimateHide = false
                };

                var controller = await Parent.DialogCoordinator.ShowProgressAsync(Parent, "Please wait...", "Export data to Matlab!", settings: settings);
                controller.SetIndeterminate();
                controller.SetCancelable(false);

                MatlabExporter matlabExporter = new MatlabExporter(string.Format("{0}.mat", fullPath), MatlabFormat.Imu0);
                matlabExporter.Open();
                matlabExporter.AddFromReader(reader);
                matlabExporter.Close();
                reader.Close();
                await controller.CloseAsync();
            });
        }

        private Task DoRefresh(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    Refresh();
                }, null);
            });
        }

        private Task DoStart(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                ReplayFile file = o as ReplayFile;
                DataReader reader = null;
                string fullPath = null;
                Parent.SyncContext.Send(c =>
                {
                    IsReplaying = true;
                    fullPath = file.FullPath;
                }, null);

                reader = new DataReader(fullPath, ReaderMode.Imu0 | ReaderMode.Camera0);
                reader.Open();

                Parent.IOProxy.ReplayOffline(reader, new Action<TimeSpan>((t) =>
                {
                    Parent.SyncContext.Post(c =>
                   {
                       ReplayTime = t;
                   }, null);
                }), new Action(() =>
                {
                    Parent.SyncContext.Post(c =>
                    {
                        IsReplaying = false;
                    }, null);
                }));
            });
        }

        private void Refresh()
        {
            FilesForReplay.Clear();
            if (Parent.SettingContainer.Settings.GeneralSettings.FileLocations != null && Parent.SettingContainer.Settings.GeneralSettings.FileLocations.Count > 0)
            {
                foreach (FileLocation loc in Parent.SettingContainer.Settings.GeneralSettings.FileLocations)
                {
                    Tuple<FileLocation, List<ReplayFile>> tuple = new Tuple<FileLocation, List<ReplayFile>>(loc, new List<ReplayFile>());
                    FilesForReplay.Add(tuple);
                    string fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(loc.Path));
                    if (Directory.Exists(fullPath))
                    {
                        foreach (string file in Directory.GetFiles(fullPath, "*.ffc"))
                        {
                            tuple.Item2.Add(new ReplayFile() { Name = Path.GetFileNameWithoutExtension(file), FullPath = file });
                        }
                    }
                }
            }
        }
    }
}