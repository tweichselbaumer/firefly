using Emgu.CV;
using FireFly.Command;
using FireFly.Models;
using FireFly.Utilities;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace FireFly.CustomDialogs
{
    public class ImageResultViewerDialogModel : DependencyObject
    {
        private readonly RelayCommand<object> _CloseCommand;
        private readonly RelayCommand<object> _SaveCommand;
        private readonly RelayCommand<object> _ExportCommand;


        public RangeObservableCollection<CvImageContainer> Images
        {
            get { return (RangeObservableCollection<CvImageContainer>)GetValue(ImagesProperty); }
            set { SetValue(ImagesProperty, value); }
        }

        public static readonly DependencyProperty ImagesProperty =
            DependencyProperty.Register("Images", typeof(RangeObservableCollection<CvImageContainer>), typeof(ImageResultViewerDialogModel), new PropertyMetadata(null));


        public ImageResultViewerDialogModel(Action<ImageResultViewerDialogModel> closeHandel, Action<ImageResultViewerDialogModel> saveHandel, Action<ImageResultViewerDialogModel> exportHandel, string directory, string pattern)
        {
            Images = new RangeObservableCollection<CvImageContainer>();
            foreach (string file in Directory.GetFiles(directory, pattern))
            {
                Images.Add(new CvImageContainer() { CvImage = CvInvoke.Imread(file) });
            }

            _CloseCommand = new RelayCommand<object>(
                async (object o) =>
                {
                    await Task.Factory.StartNew(() =>
                    {
                        closeHandel(this);
                    });
                });

            _SaveCommand = new RelayCommand<object>(
                async (object o) =>
                {
                    await Task.Factory.StartNew(() =>
                    {
                        saveHandel(this);
                    });
                });

            _ExportCommand = new RelayCommand<object>(
                async (object o) =>
                {
                    await Task.Factory.StartNew(() =>
                    {
                        exportHandel(this);
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

        public RelayCommand<object> ExportCommand
        {
            get
            {
                return _ExportCommand;
            }
        }
    }
}