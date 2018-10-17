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



        public RangeObservableCollection<CvImageContainer> Images
        {
            get { return (RangeObservableCollection<CvImageContainer>)GetValue(ImagesProperty); }
            set { SetValue(ImagesProperty, value); }
        }

        public static readonly DependencyProperty ImagesProperty =
            DependencyProperty.Register("Images", typeof(RangeObservableCollection<CvImageContainer>), typeof(ImageResultViewerDialogModel), new PropertyMetadata(null));


        public ImageResultViewerDialogModel(Action<ImageResultViewerDialogModel> closeHandel, string directory, string pattern)
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
        }

        public RelayCommand<object> CloseCommand
        {
            get
            {
                return _CloseCommand;
            }
        }
    }
}