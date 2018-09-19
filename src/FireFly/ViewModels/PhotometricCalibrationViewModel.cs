using FireFly.Command;
using FireFly.CustomDialogs;
using FireFly.Data.Storage;
using FireFly.Utilities;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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
                    CustomDialog customDialog = new CustomDialog() { Title = "Select calibration file" };

                    var dataContext = new ReplaySelectDialogModel(obj =>
                    {
                        Parent.SyncContext.Post(d =>
                        {
                            Task.Factory.StartNew(async () =>
                            {
                                MetroDialogSettings settings = new MetroDialogSettings()
                                {
                                    AnimateShow = false,
                                    AnimateHide = false
                                };
                                var controller = await Parent.DialogCoordinator.ShowProgressAsync(Parent, "Please wait...", "Photometric calibration!", settings: settings);
                                controller.SetCancelable(false);
                                controller.SetIndeterminate();


                                ReplaySelectDialogModel replaySelectDialogModel = obj as ReplaySelectDialogModel;
                                if (replaySelectDialogModel.SelectedFile != null)
                                {
                                    string file = null;
                                    string outputPath = null;

                                    double fxO = 0.0;
                                    double fyO = 0.0;
                                    double cxO = 0.0;
                                    double cyO = 0.0;

                                    double fxN = 0.0;
                                    double fyN = 0.0;
                                    double cxN = 0.0;
                                    double cyN = 0.0;

                                    double k1 = 0.0;
                                    double k2 = 0.0;
                                    double k3 = 0.0;
                                    double k4 = 0.0;

                                    int width = 0;
                                    int height = 0;
                                    Parent.SyncContext.Send(async d2 =>
                                    {
                                        file = replaySelectDialogModel.SelectedFile.FullPath;
                                        outputPath = Path.Combine(Path.GetTempPath(), "firefly", Guid.NewGuid().ToString());

                                        fxO = Parent.CameraViewModel.OrginalCameraMatrix.GetValue(0, 0);
                                        fyO = Parent.CameraViewModel.OrginalCameraMatrix.GetValue(1, 1);
                                        cxO = Parent.CameraViewModel.OrginalCameraMatrix.GetValue(0, 2);
                                        cyO = Parent.CameraViewModel.OrginalCameraMatrix.GetValue(1, 2);

                                        fxN = Parent.CameraViewModel.NewCameraMatrix.GetValue(0, 0);
                                        fyN = Parent.CameraViewModel.NewCameraMatrix.GetValue(1, 1);
                                        cxN = Parent.CameraViewModel.NewCameraMatrix.GetValue(0, 2);
                                        cyN = Parent.CameraViewModel.NewCameraMatrix.GetValue(1, 2);

                                        k1 = Parent.CameraViewModel.DistortionCoefficients.GetValue(0, 0);
                                        k2 = Parent.CameraViewModel.DistortionCoefficients.GetValue(0, 1);
                                        k3 = Parent.CameraViewModel.DistortionCoefficients.GetValue(0, 2);
                                        k4 = Parent.CameraViewModel.DistortionCoefficients.GetValue(0, 3);

                                        width = Parent.CameraViewModel.ImageWidth;
                                        height = Parent.CameraViewModel.ImageHeight;

                                    }, null);

                                    DataReader reader = new DataReader(file, ReaderMode.Camera0);

                                    reader.Open();

                                    PhotometricCalibratrionExporter exporter = new PhotometricCalibratrionExporter(fxO, fyO, cxO, cyO, fxN, fyN, cxN, cyN, width, height, k1, k2, k3, k4, outputPath, false);
                                    exporter.Open();
                                    exporter.AddFromReader(reader, delegate (double percent)
                                    {
                                        double value = percent * 0.33 + 0.33;
                                        value = value > 1 ? 1 : value;
                                        controller.SetProgress(value);
                                    });
                                    exporter.Close();
                                    reader.Close();

                                    Process p = new Process();
                                    //p.StartInfo.RedirectStandardError = true;
                                    p.StartInfo.RedirectStandardOutput = true;
                                    //p.StartInfo.RedirectStandardInput = true;
                                    p.StartInfo.UseShellExecute = false;
                                    p.StartInfo.WorkingDirectory = outputPath;
                                    p.StartInfo.CreateNoWindow = true;
                                    p.EnableRaisingEvents = true;
                                    p.StartInfo.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Tools", "responseCalib.exe");
                                    p.StartInfo.Arguments = string.Format("{0}\\ -noGUI", outputPath);

                                    p.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
                                        {
                                            Debug.WriteLine(e.Data);
                                        }
                                    );

                                    p.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
                                    {
                                        Debug.WriteLine(e.Data);
                                    });

                                    p.Exited += new EventHandler((s, e) =>
                                    {
                                        Parent.SyncContext.Post(x =>
                                        {
                                            Directory.Delete(outputPath, true);
                                            controller.CloseAsync();
                                            Parent.DialogCoordinator.HideMetroDialogAsync(Parent, customDialog);
                                        }, null);
                                    });

                                    p.Start();
                                    p.BeginOutputReadLine();
                                    //p.BeginErrorReadLine();
                                }

                            }, TaskCreationOptions.LongRunning);
                            Parent.DialogCoordinator.HideMetroDialogAsync(Parent, customDialog);
                        }, null);
                    });
                    Parent.ReplayViewModel.Refresh();
                    dataContext.FilesForReplay.AddRange(Parent.ReplayViewModel.FilesForReplay.Where(x => !x.Item1.IsRemote));
                    customDialog.Content = new ReplaySelectDialog { DataContext = dataContext };

                    Parent.DialogCoordinator.ShowMetroDialogAsync(Parent, customDialog);
                }, null);
            });
        }
    }
}