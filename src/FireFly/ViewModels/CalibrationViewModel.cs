using System.Windows;

namespace FireFly.ViewModels
{
    public class CalibrationViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty ExtrinsicCalibrationViewModelProperty =
            DependencyProperty.Register("ExtrinsicCalibrationViewModel", typeof(ExtrinsicCalibrationViewModel), typeof(CalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty ImuCalibrationViewModelProperty =
            DependencyProperty.Register("ImuCalibrationViewModel", typeof(ImuCalibrationViewModel), typeof(CalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty IntrinsicCalibrationViewModelProperty =
                    DependencyProperty.Register("IntrinsicCalibrationViewModel", typeof(IntrinsicCalibrationViewModel), typeof(CalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty PhotometricCalibrationViewModelProperty =
            DependencyProperty.Register("PhotometricCalibrationViewModel", typeof(PhotometricCalibrationViewModel), typeof(CalibrationViewModel), new PropertyMetadata(null));

        public CalibrationViewModel(MainViewModel parent) : base(parent)
        {
            IntrinsicCalibrationViewModel = new IntrinsicCalibrationViewModel(parent);
            PhotometricCalibrationViewModel = new PhotometricCalibrationViewModel(parent);
            ExtrinsicCalibrationViewModel = new ExtrinsicCalibrationViewModel(parent);
            ImuCalibrationViewModel = new ImuCalibrationViewModel(parent);
        }

        public ExtrinsicCalibrationViewModel ExtrinsicCalibrationViewModel
        {
            get { return (ExtrinsicCalibrationViewModel)GetValue(ExtrinsicCalibrationViewModelProperty); }
            set { SetValue(ExtrinsicCalibrationViewModelProperty, value); }
        }

        public ImuCalibrationViewModel ImuCalibrationViewModel
        {
            get { return (ImuCalibrationViewModel)GetValue(ImuCalibrationViewModelProperty); }
            set { SetValue(ImuCalibrationViewModelProperty, value); }
        }

        public IntrinsicCalibrationViewModel IntrinsicCalibrationViewModel
        {
            get { return (IntrinsicCalibrationViewModel)GetValue(IntrinsicCalibrationViewModelProperty); }
            set { SetValue(IntrinsicCalibrationViewModelProperty, value); }
        }

        public PhotometricCalibrationViewModel PhotometricCalibrationViewModel
        {
            get { return (PhotometricCalibrationViewModel)GetValue(PhotometricCalibrationViewModelProperty); }
            set { SetValue(PhotometricCalibrationViewModelProperty, value); }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();
            IntrinsicCalibrationViewModel.SettingsUpdated();
        }
    }
}