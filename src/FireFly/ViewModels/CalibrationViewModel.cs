using FireFly.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FireFly.ViewModels
{
    public class CalibrationViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty ImagesProperty =
            DependencyProperty.Register("Images", typeof(ObservableCollection<BitmapSource>), typeof(CalibrationViewModel), new PropertyMetadata(new ObservableCollection<BitmapSource>()));

        public CalibrationViewModel(MainViewModel parent) : base(parent)
        {
        }

        public ObservableCollection<BitmapSource> Images
        {
            get { return (ObservableCollection<BitmapSource>)GetValue(ImagesProperty); }
            set { SetValue(ImagesProperty, value); }
        }

        public RelayCommand<object> TakeSnapshotCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoTakeSnapshot(o);
                    });
            }
        }

        private Task DoTakeSnapshot(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    Images.Add(Parent.CameraViewModel.ImageSource);
                }, null);
            });
        }

        internal override void SettingsUpdated()
        {
        }

        internal override void UpdateLinkUpBindings()
        {
        }
    }
}