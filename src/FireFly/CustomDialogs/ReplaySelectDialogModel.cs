using FireFly.Command;
using FireFly.Models;
using FireFly.Settings;
using FireFly.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace FireFly.CustomDialogs
{
    public class ReplaySelectDialogModel : DependencyObject
    {
        public static readonly DependencyProperty FilesForReplayProperty =
            DependencyProperty.Register("FilesForReplay", typeof(RangeObservableCollection<Tuple<FileLocation, List<ReplayFile>>>), typeof(ReplaySelectDialogModel), new PropertyMetadata(null));

        private readonly RelayCommand<object> _CloseCommand;
        private readonly RelayCommand<object> _SelectCommand;

        private ReplayFile _SelectedFile;


        public ReplaySelectDialogModel(Action<ReplaySelectDialogModel> closeHandel)
        {
            FilesForReplay = new RangeObservableCollection<Tuple<FileLocation, List<ReplayFile>>>();
            _CloseCommand = new RelayCommand<object>(
                async (object o) =>
                {
                    await Task.Factory.StartNew(() =>
                    {
                        _SelectedFile = null;
                        closeHandel(this);
                    });
                });
            _SelectCommand = new RelayCommand<object>(
               async (object o) =>
               {
                   await Task.Factory.StartNew(() =>
                   {
                       _SelectedFile = o as ReplayFile;
                       closeHandel(this);
                   });
               });
        }

        public RangeObservableCollection<Tuple<FileLocation, List<ReplayFile>>> FilesForReplay
        {
            get { return (RangeObservableCollection<Tuple<FileLocation, List<ReplayFile>>>)GetValue(FilesForReplayProperty); }
            set { SetValue(FilesForReplayProperty, value); }
        }

        public RelayCommand<object> CloseCommand
        {
            get
            {
                return _CloseCommand;
            }
        }

        public RelayCommand<object> SelectCommand
        {
            get
            {
                return _SelectCommand;
            }
        }

        public ReplayFile SelectedFile
        {
            get
            {
                return _SelectedFile;
            }
        }
    }
}