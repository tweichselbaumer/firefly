using System.Windows;

namespace FireFly.ViewModels
{
    public class CalibrationViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty IntrinsicCalibrationViewModelProperty =
            DependencyProperty.Register("IntrinsicCalibrationViewModel", typeof(IntrinsicCalibrationViewModel), typeof(CalibrationViewModel), new PropertyMetadata(null));

        public CalibrationViewModel(MainViewModel parent) : base(parent)
        {
            IntrinsicCalibrationViewModel = new IntrinsicCalibrationViewModel(parent);
        }

        public IntrinsicCalibrationViewModel IntrinsicCalibrationViewModel
        {
            get { return (IntrinsicCalibrationViewModel)GetValue(IntrinsicCalibrationViewModelProperty); }
            set { SetValue(IntrinsicCalibrationViewModelProperty, value); }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();
            IntrinsicCalibrationViewModel.SettingsUpdated();
        }
    }
}