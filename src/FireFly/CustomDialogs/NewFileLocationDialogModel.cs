using FireFly.Command;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FireFly.CustomDialogs
{
    public class NewFileLocationDialogModel : DependencyObject
    {
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(NewFileLocationDialog), new PropertyMetadata(""));

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(NewFileLocationDialog), new PropertyMetadata(""));

        private readonly RelayCommand<object> _CloseCommand;

        public NewFileLocationDialogModel(Action<NewFileLocationDialogModel> closeHandel)
        {
            _CloseCommand = new RelayCommand<object>(
                async (object o) =>
                {
                    await Task.Factory.StartNew(() =>
                    {
                        closeHandel(this);
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

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }
    }
}