using FireFly.Command;
using FireFly.Data.Storage;
using FireFly.Models;
using FireFly.Settings;
using FireFly.Utilities;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private List<FileLocation> _FileLocations = new List<FileLocation>();
        private string _IpAddress;
        private bool _IsStopping = false;
        private string _Password;
        private string _Username;

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

        public bool IsStopping
        {
            get
            {
                return _IsStopping;
            }

            set
            {
                _IsStopping = value;
            }
        }

        public RelayCommand<object> PauseCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoPause(o);
                    });
            }
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

        public void Refresh()
        {
            if (!IsReplaying)
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
                                ReplayFile replayFile = new ReplayFile() { Name = Path.GetFileNameWithoutExtension(file), FullPath = file };
                                tuple.Item2.Add(replayFile);
                                Task.Run(() =>
                                {
                                    string notes = RawDataReader.ReadNotes(file);
                                    Parent.SyncContext.Post(c =>
                                    {
                                        replayFile.Notes = notes;
                                    }, null);
                                });
                            }
                        }
                    }
                }
                if (Parent.ConnectivityState == LinkUp.Raw.LinkUpConnectivityState.Connected)
                {
                    RemoteDataStore remoteDataStore = new RemoteDataStore(Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.IpAddress, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Username, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Password);

                    List<string> files = remoteDataStore.GetAllFileNames("/home/up/data");
                    if (files.Count > 0)
                    {
                        Tuple<FileLocation, List<ReplayFile>> tuple = new Tuple<FileLocation, List<ReplayFile>>(new FileLocation() { Name = "Remote", Path = "/home/up/data", IsRemote = true }, new List<ReplayFile>());
                        FilesForReplay.Add(tuple);

                        foreach (string filename in files)
                        {
                            if (Path.GetExtension(filename) == ".csv")
                                tuple.Item2.Add(new ReplayFile() { Name = filename, IsRemote = true, FullPath = string.Format("{0}/{1}", tuple.Item1.Path, filename) });
                        }
                    }
                }
            }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();

            bool changed = false;
            changed |= Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.IpAddress != _IpAddress;
            changed |= Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Username != _Username;
            changed |= Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Password != _Password;

            var firstNotSecond = _FileLocations.Select(c => c.Name + c.Path).Except(Parent.SettingContainer.Settings.GeneralSettings.FileLocations.Select(d => d.Name + d.Path)).ToList();
            var secondNotFirst = Parent.SettingContainer.Settings.GeneralSettings.FileLocations.Select(d => d.Name + d.Path).Except(_FileLocations.Select(c => c.Name + c.Path)).ToList();

            changed |= firstNotSecond.Any() || secondNotFirst.Any();

            if (changed)
            {
                _IpAddress = Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.IpAddress;
                _Username = Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Username;
                _Password = Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Password;

                _FileLocations.Clear();
                _FileLocations.AddRange(Parent.SettingContainer.Settings.GeneralSettings.FileLocations);

                Refresh();
            }
        }

        private Task DoExport(object o)
        {
            return Task.Factory.StartNew(async () =>
            {
                ReplayFile file = o as ReplayFile;
                RawDataReader reader = null;

                string fullPath = null;
                bool isRemote = false;
                System.Windows.Forms.SaveFileDialog saveFileDialog = null;
                bool save = false;

                Parent.SyncContext.Send(c =>
                {
                    fullPath = file.FullPath;
                    isRemote = file.IsRemote;
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

                    controller.SetCancelable(false);

                    if (isRemote)
                    {
                        reader = new RawDataReader(fullPath, RawReaderMode.Imu0, new RemoteDataStore(Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.IpAddress, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Username, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Password));
                        reader.Open(delegate (double percent)
                        {
                            double value = percent * 0.66;
                            value = value > 1 ? 1 : value;
                            controller.SetProgress(value);
                        });
                    }
                    else
                    {
                        reader = new RawDataReader(fullPath, RawReaderMode.Imu0 | RawReaderMode.Camera0);
                        controller.SetIndeterminate();
                        reader.Open();
                    }

                    RawMatlabExporter matlabExporter = new RawMatlabExporter(saveFileDialog.FileName, MatlabFormat.Imu0);

                    matlabExporter.Open();
                    if (isRemote)
                    {
                        matlabExporter.AddFromReader(reader, delegate (double percent)
                        {
                            double value = percent * 0.33 + 0.66;
                            value = value > 1 ? 1 : value;
                            controller.SetProgress(value);
                        });
                    }
                    else
                    {
                        matlabExporter.AddFromReader(reader, delegate (double percent)
                        {
                            controller.SetProgress(percent);
                        });
                    }
                    matlabExporter.Close();

                    reader.Close();
                    await controller.CloseAsync();
                }
            });
        }

        private Task DoPause(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                ReplayFile file = o as ReplayFile;
                Parent.SyncContext.Post(c =>
                {
                    file.IsPaused = true;
                }, null);
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
                RawDataReader reader = null;
                string fullPath = null;
                Parent.SyncContext.Send(c =>
                {
                    if (!IsReplaying)
                    {
                        IsReplaying = true;
                        fullPath = file.FullPath;
                        file.IsPlaying = true;
                    }
                    else
                    {
                        file.IsPlaying = true;
                        file.IsPaused = false;
                    }
                }, null);

                if (!string.IsNullOrEmpty(fullPath))
                {
                    reader = new RawDataReader(fullPath, RawReaderMode.Imu0 | RawReaderMode.Camera0);
                    reader.Open();

                    Parent.IOProxy.ChangeSlamStatus(Proxy.SlamStatusOverall.Restart, true);

                    Parent.IOProxy.ReplayOffline(reader, new Action<TimeSpan>((t) =>
                    {
                        Parent.SyncContext.Post(c =>
                       {
                           ReplayTime = t;
                       }, null);
                    }), new Action(() =>
                    {
                        Parent.IOProxy.ChangeSlamStatus(Proxy.SlamStatusOverall.Stop);
                        Parent.SyncContext.Post(c =>
                        {
                            IsReplaying = false;
                            file.IsPlaying = false;
                            file.IsPaused = false;
                            IsStopping = false;
                        }, null);
                    }), delegate ()
                    {
                        bool isPaused = false;

                        Parent.SyncContext.Send(c =>
                        {
                            isPaused = file.IsPaused;
                        }, null);

                        return isPaused;
                    },
                    delegate ()
                    {
                        return IsStopping;
                    });
                }
            });
        }

        private Task DoStop(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                ReplayFile file = o as ReplayFile;
                Parent.SyncContext.Post(c =>
                {
                    file.IsPaused = false;
                    IsStopping = true;
                }, null);
            });
        }
    }
}