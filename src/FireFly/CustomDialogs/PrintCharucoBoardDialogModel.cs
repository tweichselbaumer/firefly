using FireFly.Command;
using FireFly.Models;
using FireFly.VI.Calibration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static Emgu.CV.Aruco.Dictionary;

namespace FireFly.CustomDialogs
{
    public class PrintCharucoBoardDialogModel : DependencyObject
    {
        public static readonly DependencyProperty DictionaryProperty =
            DependencyProperty.Register("Dictionary", typeof(PredefinedDictionaryName), typeof(PrintCharucoBoardDialogModel), new FrameworkPropertyMetadata(PredefinedDictionaryName.Dict4X4_50, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(CvImageContainer), typeof(PrintCharucoBoardDialogModel), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty MarkerLengthProperty =
            DependencyProperty.Register("MarkerLength", typeof(int), typeof(PrintCharucoBoardDialogModel), new FrameworkPropertyMetadata(450, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty SquareLengthProperty =
            DependencyProperty.Register("SquareLength", typeof(int), typeof(PrintCharucoBoardDialogModel), new FrameworkPropertyMetadata(600, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty SquaresXProperty =
            DependencyProperty.Register("SquaresX", typeof(int), typeof(PrintCharucoBoardDialogModel), new FrameworkPropertyMetadata(5, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty SquaresYProperty =
            DependencyProperty.Register("SquaresY", typeof(int), typeof(PrintCharucoBoardDialogModel), new FrameworkPropertyMetadata(7, new PropertyChangedCallback(OnPropertyChanged)));

        private readonly RelayCommand<object> _CloseCommand;
        private readonly RelayCommand<object> _CreateCommand;

        public PrintCharucoBoardDialogModel(Action<PrintCharucoBoardDialogModel> createHandel, Action<PrintCharucoBoardDialogModel> closeHandel)
        {
            Image = new CvImageContainer();
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

        public PredefinedDictionaryName Dictionary
        {
            get { return (PredefinedDictionaryName)GetValue(DictionaryProperty); }
            set { SetValue(DictionaryProperty, value); }
        }

        public CvImageContainer Image
        {
            get { return (CvImageContainer)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public int MarkerLength
        {
            get { return (int)GetValue(MarkerLengthProperty); }
            set { SetValue(MarkerLengthProperty, value); }
        }

        public IEnumerable<PredefinedDictionaryName> PredefinedDictionaryNames
        {
            get
            {
                return Enum.GetValues(typeof(PredefinedDictionaryName)).Cast<PredefinedDictionaryName>();
            }
        }

        public int SquareLength
        {
            get { return (int)GetValue(SquareLengthProperty); }
            set { SetValue(SquareLengthProperty, value); }
        }

        public int SquaresX
        {
            get { return (int)GetValue(SquaresXProperty); }
            set { SetValue(SquaresXProperty, value); }
        }

        public int SquaresY
        {
            get { return (int)GetValue(SquaresYProperty); }
            set { SetValue(SquaresYProperty, value); }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                PrintCharucoBoardDialogModel obj = d as PrintCharucoBoardDialogModel;
                obj.Image.CvImage = ChArUcoCalibration.DrawBoard(obj.SquaresX, obj.SquaresY, obj.SquareLength, obj.MarkerLength, new System.Drawing.Size(obj.SquaresX * obj.SquareLength, obj.SquaresY * obj.SquareLength), 0, obj.Dictionary);
            }
            catch (Exception)
            { }
        }
    }
}