using FireFly.Command;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FireFly.CustomDialogs
{
    public class PrintAprilGridDialogModel : DependencyObject
    {
        public static readonly DependencyProperty TagSizeProperty =
            DependencyProperty.Register("TagSize", typeof(double), typeof(PrintAprilGridDialogModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty TagSpacingFactorProperty =
            DependencyProperty.Register("TagSpacingFactor", typeof(double), typeof(PrintAprilGridDialogModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty TagsXProperty =
            DependencyProperty.Register("TagsX", typeof(int), typeof(PrintAprilGridDialogModel), new PropertyMetadata(0));

        public static readonly DependencyProperty TagsYProperty =
            DependencyProperty.Register("TagsY", typeof(int), typeof(PrintAprilGridDialogModel), new PropertyMetadata(0));

        private readonly RelayCommand<object> _CloseCommand;
        private readonly RelayCommand<object> _CreateCommand;

        public PrintAprilGridDialogModel(Action<PrintAprilGridDialogModel> createHandel, Action<PrintAprilGridDialogModel> closeHandel)
        {
            _CloseCommand = new RelayCommand<object>(
                async (object o) =>
                {
                    await Task.Factory.StartNew(() =>
                    {
                        closeHandel(this);
                    });
                });
            _CreateCommand = new RelayCommand<object>(
                async (object o) =>
                {
                    await Task.Factory.StartNew(() =>
                    {
                        createHandel(this);
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

        public RelayCommand<object> CreateCommand
        {
            get
            {
                return _CreateCommand;
            }
        }

        public double TagSize
        {
            get { return (double)GetValue(TagSizeProperty); }
            set { SetValue(TagSizeProperty, value); }
        }

        public double TagSpacingFactor
        {
            get { return (double)GetValue(TagSpacingFactorProperty); }
            set { SetValue(TagSpacingFactorProperty, value); }
        }

        public int TagsX
        {
            get { return (int)GetValue(TagsXProperty); }
            set { SetValue(TagsXProperty, value); }
        }

        public int TagsY
        {
            get { return (int)GetValue(TagsYProperty); }
            set { SetValue(TagsYProperty, value); }
        }
    }
}