using FireFly.Command;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FireFly.CustomDialogs
{
    public class NewFileLocationDialogModel : DependencyObject
    {
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(NewFileLocationDialog), new PropertyMetadata(""));

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(NewFileLocationDialog), new PropertyMetadata(""));

        private readonly RelayCommand<object> _CloseCommand;
        private readonly RelayCommand<object> _SaveCommand;

        public NewFileLocationDialogModel(Action<NewFileLocationDialogModel> closeHandle, Action<NewFileLocationDialogModel> saveHandle)
        {
            _CloseCommand = new RelayCommand<object>(
                async (object o) =>
                {
                    await Task.Factory.StartNew(() =>
                    {
                        closeHandle(this);
                    });
                });

            _SaveCommand = new RelayCommand<object>(
                async (object o) =>
                {
                    await Task.Factory.StartNew(() =>
                    {
                        saveHandle(this);
                    });
                });
        }

        public RelayCommand<object> CloseCommand
        {
            get
            {
                return _CloseCommand;
            }
        }

        public RelayCommand<object> SaveCommand
        {
            get
            {
                return _SaveCommand;
            }
        }

        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }
    }
}