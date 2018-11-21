using Emgu.CV;
using FireFly.Command;
using FireFly.CustomDialogs;
using FireFly.Data.Storage;
using FireFly.Models;
using FireFly.Utilities;
using MahApps.Metro.Controls.Dialogs;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace FireFly.ViewModels
{
    public class PhotometricCalibrationViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty ResponseValuesProperty =
            DependencyProperty.Register("ResponseValues", typeof(RangeObservableCollection<DataPoint>), typeof(PhotometricCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty VignetteProperty =
            DependencyProperty.Register("Vignette", typeof(CvImageContainer), typeof(PhotometricCalibrationViewModel), new PropertyMetadata(null));

        public PhotometricCalibrationViewModel(MainViewModel parent) : base(parent)
        {
            ResponseValues = new RangeObservableCollection<DataPoint>();
            ResponseValues.CollectionChanged += ResponseValues_CollectionChanged;
        }

        public List<DataPoint> LinearResponseValues
        {
            get
            {
                return Enumerable.Range(0, 256).Select(c => new DataPoint(c, c)).ToList();
            }
        }

        public RangeObservableCollection<DataPoint> ResponseValues
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(ResponseValuesProperty); }
            set { SetValue(ResponseValuesProperty, value); }
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

        public RelayCommand<object> SaveVignetteCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoSaveVignette(o);
                    });
            }
        }

        public CvImageContainer Vignette
        {
            get { return (CvImageContainer)GetValue(VignetteProperty); }
            set { SetValue(VignetteProperty, value); }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();

            var firstNotSecond = ResponseValues.Select(c => c.Y).Except(Parent.SettingContainer.Settings.CalibrationSettings.PhotometricCalibrationSettings.ResponseValues).ToList();
            var secondNotFirst = Parent.SettingContainer.Settings.CalibrationSettings.PhotometricCalibrationSettings.ResponseValues.Except(ResponseValues.Select(c => c.Y)).ToList();

            bool changed = firstNotSecond.Any() || secondNotFirst.Any();

            if (changed)
            {
                List<double> l = Parent.SettingContainer.Settings.CalibrationSettings.PhotometricCalibrationSettings.ResponseValues.ToList();
                ResponseValues.Clear();
                ResponseValues.AddRange(l.Select((c, i) => new DataPoint(i, c)));
            }
            Vignette = new CvImageContainer();
            try
            {
                if (!string.IsNullOrEmpty(Parent.SettingContainer.Settings.CalibrationSettings.PhotometricCalibrationSettings.VignetteFileBase64))
                {
                    byte[] data = Convert.FromBase64String(Parent.SettingContainer.Settings.CalibrationSettings.PhotometricCalibrationSettings.VignetteFileBase64);
                    Mat temp = new Mat();
                    CvInvoke.Imdecode(data, Emgu.CV.CvEnum.ImreadModes.Grayscale, temp);
                    Vignette.CvImage = temp;
                }
            }
            catch (Exception)
            {
            }
        }

        private Task DoRunCalibration(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                bool response = o is string && !string.IsNullOrEmpty(o as string) && (o as string).Equals("response");

                Parent.SyncContext.Post(c =>
                {
                    CustomDialog customDialog;

                    if (response)
                    {
                        customDialog = new CustomDialog() { Title = "Select response calibration file" };
                    }
                    else
                    {
                        customDialog = new CustomDialog() { Title = "Select vignette calibration file" };
                    }

                    var dataContext = new ReplaySelectDialogModel(OnReplaySelectDialogClose(response, customDialog));
                    Parent.ReplayViewModel.Refresh();
                    dataContext.FilesForReplay.AddRange(Parent.ReplayViewModel.FilesForReplay.Where(x => !x.Item1.IsRemote));
                    customDialog.Content = new ReplaySelectDialog { DataContext = dataContext };

                    Parent.DialogCoordinator.ShowMetroDialogAsync(Parent, customDialog);
                }, null);
            });
        }

        private Task DoSaveVignette(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(c =>
                {
                    System.Windows.Forms.SaveFileDialog saveFileDialog = null;
                    bool save = false;
                    saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                    saveFileDialog.Filter = "Portable Network Graphics (*.png) | *.png";
                    save = saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;

                    if (save)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, Convert.FromBase64String(Parent.SettingContainer.Settings.CalibrationSettings.PhotometricCalibrationSettings.VignetteFileBase64));
                    }
                }, null);
            });
        }

        private Action<ReplaySelectDialogModel> OnReplaySelectDialogClose(bool response, CustomDialog customDialog)
        {
            return obj =>
            {
                Parent.SyncContext.Send(d =>
                {
                    Parent.DialogCoordinator.HideMetroDialogAsync(Parent, customDialog);
                }, null);

                Parent.SyncContext.Post(d =>
                {
                    Task.Factory.StartNew(async () =>
                    {
                        ReplaySelectDialogModel replaySelectDialogModel = obj as ReplaySelectDialogModel;
                        if (replaySelectDialogModel.SelectedFile != null)
                        {
                            MetroDialogSettings settings = new MetroDialogSettings()
                            {
                                AnimateShow = false,
                                AnimateHide = false
                            };
                            var controller = await Parent.DialogCoordinator.ShowProgressAsync(Parent, "Please wait...", "Photometric calibration!", settings: Parent.MetroDialogSettings);
                            controller.SetCancelable(false);
                            controller.SetIndeterminate();

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
                            List<double> responseValues = new List<double>();
                            Parent.SyncContext.Send(d2 =>
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

                                if (!response)
                                    responseValues.AddRange(ResponseValues.Select(f => f.Y));

                                width = Parent.CameraViewModel.ImageWidth;
                                height = Parent.CameraViewModel.ImageHeight;
                            }, null);

                            DataReader reader = new DataReader(file, ReaderMode.Camera0);

                            reader.Open();

                            PhotometricCalibratrionExporter exporter = new PhotometricCalibratrionExporter(fxO, fyO, cxO, cyO, fxN, fyN, cxN, cyN, width, height, k1, k2, k3, k4, outputPath, response, responseValues);
                            exporter.Open();
                            exporter.AddFromReader(reader, delegate (double percent)
                            {
                                double value = percent * 0.33;
                                value = value > 1 ? 1 : value;
                                controller.SetProgress(value);
                            });
                            exporter.Close();
                            reader.Close();

                            Process p = new Process();
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.WorkingDirectory = outputPath;
                            p.StartInfo.CreateNoWindow = true;
                            p.EnableRaisingEvents = true;

                            string options = "";

                            if (response)
                            {
                                p.StartInfo.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Tools", "responseCalib.exe");
                            }
                            else
                            {
                                p.StartInfo.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Tools", "vignetteCalib.exe");
                                options = "facW=7 facH=7";
                            }
                            p.StartInfo.Arguments = string.Format("{0}\\ -noGUI -showPercent {1}", outputPath, options);

                            p.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
                            {
                                Debug.WriteLine(e.Data);
                                if (!string.IsNullOrEmpty(e.Data))
                                {
                                    foreach (string line in e.Data.Split('\n'))
                                    {
                                        if (line.StartsWith("percent: "))
                                        {
                                            double percent = double.Parse(line.Replace("percent: ", ""), CultureInfo.InvariantCulture);
                                            double value = 0.33 + percent * 0.66;
                                            value = value > 1 ? 1 : value;
                                            controller.SetProgress(value);
                                        }
                                    }
                                }
                            }
                            );

                            p.Exited += new EventHandler((s, e) =>
                            {
                                Parent.SyncContext.Post(async x =>
                                {
                                    if (response)
                                    {
                                        ParseResponseResult(outputPath);
                                    }
                                    else
                                    {
                                        ParseVignetteResult(outputPath);
                                    }
                                    Directory.Delete(outputPath, true);
                                    await controller.CloseAsync();
                                }, null);
                            });

                            p.Start();
                            p.BeginOutputReadLine();
                        }
                    }, TaskCreationOptions.LongRunning);
                }, null);
            };
        }

        private void ParseResponseResult(string outputPath)
        {
            ResponseValues.Clear();
            int i = 0;
            string file = Path.Combine(outputPath, "photoCalibResult", "pcalib.txt");
            string content = File.ReadAllText(file);
            foreach (string item in content.Split(' '))
            {
                if (i >= 256)
                    break;
                double val = 0.0;
                double.TryParse(item, NumberStyles.Any, CultureInfo.InvariantCulture, out val);
                ResponseValues.Add(new DataPoint(i++, val));
            }
        }

        private void ParseVignetteResult(string outputPath)
        {
            string file = Path.Combine(outputPath, "vignetteCalibResult", "vignetteSmoothed.png");
            byte[] data = File.ReadAllBytes(file);
            Parent.SettingContainer.Settings.CalibrationSettings.PhotometricCalibrationSettings.VignetteFileBase64 = Convert.ToBase64String(data);
            Parent.UpdateSettings(false);
        }

        private void ResponseValues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
            {
                var firstNotSecond = ResponseValues.Select(c => c.Y).Except(Parent.SettingContainer.Settings.CalibrationSettings.PhotometricCalibrationSettings.ResponseValues).ToList();
                var secondNotFirst = Parent.SettingContainer.Settings.CalibrationSettings.PhotometricCalibrationSettings.ResponseValues.Except(ResponseValues.Select(c => c.Y)).ToList();

                bool changed = firstNotSecond.Any() || secondNotFirst.Any();

                if (changed)
                {
                    Parent.SettingContainer.Settings.CalibrationSettings.PhotometricCalibrationSettings.ResponseValues = ResponseValues.Select(c => c.Y).ToList();
                    Parent.UpdateSettings(false);
                }
            }
        }
    }
}