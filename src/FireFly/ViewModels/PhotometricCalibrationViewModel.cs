using FireFly.Command;
using FireFly.Data.Storage;
using FireFly.Utilities;
using System.Threading.Tasks;

namespace FireFly.ViewModels
{
    public class PhotometricCalibrationViewModel : AbstractViewModel
    {
        public PhotometricCalibrationViewModel(MainViewModel parent) : base(parent)
        {
        }

        public RelayCommand<object> RunCalibrationCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoRunCalibration(o);
                    });
            }
        }

        private Task DoRunCalibration(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    //TODO:
                    string file = @"C:\Users\thoma\OneDrive\Mechatronik\Masterarbeit\Daten\sweep1.ffc";
                    string outputPath = @"E:\Data\sweep1";

                    double fxO = Parent.CameraViewModel.OrginalCameraMatrix.GetValue(0, 0);
                    double fyO = Parent.CameraViewModel.OrginalCameraMatrix.GetValue(1, 1);
                    double cxO = Parent.CameraViewModel.OrginalCameraMatrix.GetValue(0, 2);
                    double cyO = Parent.CameraViewModel.OrginalCameraMatrix.GetValue(1, 2);

                    double fxN = Parent.CameraViewModel.NewCameraMatrix.GetValue(0, 0);
                    double fyN = Parent.CameraViewModel.NewCameraMatrix.GetValue(1, 1);
                    double cxN = Parent.CameraViewModel.NewCameraMatrix.GetValue(0, 2);
                    double cyN = Parent.CameraViewModel.NewCameraMatrix.GetValue(1, 2);

                    double k1 = Parent.CameraViewModel.DistortionCoefficients.GetValue(0, 0);
                    double k2 = Parent.CameraViewModel.DistortionCoefficients.GetValue(0, 1);
                    double k3 = Parent.CameraViewModel.DistortionCoefficients.GetValue(0, 2);
                    double k4 = Parent.CameraViewModel.DistortionCoefficients.GetValue(0, 3);

                    int width = Parent.CameraViewModel.ImageWidth;
                    int height = Parent.CameraViewModel.ImageHeight;

                    DataReader reader = new DataReader(file, ReaderMode.Camera0);
                    reader.Open();
                    PhotometricCalibratrionExporter exporter = new PhotometricCalibratrionExporter(fxO, fyO, cxO, cyO, fxO, fyO, cxO, cyO, width, height, k1, k2, k3, k4, outputPath);
                    exporter.Open();
                    exporter.AddFromReader(reader);
                    exporter.Close();
                    reader.Close();
                }, null);
            });
        }
    }
}