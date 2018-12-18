using FireFly.Command;
using FireFly.CustomDialogs;
using FireFly.Data.Storage;
using FireFly.Data.Storage.Model;
using FireFly.VI.SLAM.Sophus;
using MahApps.Metro.Controls.Dialogs;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace FireFly.ViewModels
{
    public class ExtrinsicCalibrationViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty T_Imu_CamProperty =
            DependencyProperty.Register("T_Imu_Cam", typeof(Matrix3D), typeof(ExtrinsicCalibrationViewModel), new PropertyMetadata(null));

        public ExtrinsicCalibrationViewModel(MainViewModel parent) : base(parent)
        {
        }

        public RelayCommand<object> StartCalibrationCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoStartCalibration(o);
                    });
            }
        }

        public Matrix3D T_Imu_Cam
        {
            get { return (Matrix3D)GetValue(T_Imu_CamProperty); }
            set { SetValue(T_Imu_CamProperty, value); }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();
            Parent.SyncContext.Post(c =>
            {
                if (Parent.SettingContainer.Settings.CalibrationSettings.ExtrinsicCalibrationSettings.T_Cam_Imu != null)
                {
                    Sim3 sim_cam_imu = new Sim3(Parent.SettingContainer.Settings.CalibrationSettings.ExtrinsicCalibrationSettings.T_Cam_Imu);
                    Sim3 sim_imu_cam = sim_cam_imu.Inverse();
                    string b = sim_cam_imu.Matrix.ToMatrixString() + "\n\n" + sim_imu_cam.Matrix.ToMatrixString();
                    T_Imu_Cam = sim_imu_cam.Matrix3D;
                }
            }, null);
        }

        private Task DoStartCalibration(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                Parent.SyncContext.Post(f =>
                {
                    CustomDialog customDialog;

                    customDialog = new CustomDialog() { Title = "Select extrinsic calibration file" };

                    var dataContext = new ReplaySelectDialogModel(OnReplaySelectDialogClose(customDialog));
                    Parent.ReplayViewModel.Refresh();
                    dataContext.FilesForReplay.AddRange(Parent.ReplayViewModel.FilesForReplay.Where(x => !x.Item1.IsRemote));
                    customDialog.Content = new ReplaySelectDialog
                    {
                        DataContext = dataContext
                    };

                    Parent.DialogCoordinator.ShowMetroDialogAsync(Parent, customDialog);
                }, null);
            });
        }

        private Action<ReplaySelectDialogModel> OnReplaySelectDialogClose(CustomDialog customDialog)
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
                            RemoteDataStore remoteDataStore = new RemoteDataStore(Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.IpAddress, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Username, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Password);

                            string guid = Guid.NewGuid().ToString();
                            string localFile = "";
                            string remoteFile = "";
                            string remoteFolder = string.Format(@"/var/tmp/firefly/{0}", guid);
                            string expactString = string.Format("{0}@{1}:.{{0,}}[$]", Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Username, Parent.SettingContainer.Settings.ConnectionSettings.SelectedConnection.Hostname);

                            string imuModel = Imu.ConvertImuModelToString(Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.ImuModel);

                            Parent.SyncContext.Send(c =>
                        {
                            localFile = replaySelectDialogModel.SelectedFile.FullPath;
                            remoteFile = string.Format(@"/var/tmp/firefly/{0}/{1}", guid, Path.GetFileName(localFile));
                        }, null);

                            var controller = await Parent.DialogCoordinator.ShowProgressAsync(Parent, "Please wait...", "Calculating calibration parameter now!", settings: Parent.MetroDialogSettings);
                            controller.SetIndeterminate();
                            controller.SetCancelable(false);

                            remoteDataStore.ExecuteCommands(new List<string>() { string.Format("mkdir -p {0}", remoteFolder) }, expactString);

                            remoteDataStore.UploadFile(remoteFile, localFile);

                            remoteDataStore.UploadContentToFile(string.Format(@"{0}/target.yaml", remoteFolder), YamlTranslator.ConvertToYaml(new CalibrationTarget()
                            {
                                TargetType = CalibrationTargetType.Aprilgrid,
                                TagSize = Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagSize,
                                TagSpacing = Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagSpacingFactor,
                                TagCols = Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagsX,
                                TagRows = Parent.SettingContainer.Settings.CalibrationSettings.AprilGridCalibration.TagsY
                            }));

                            remoteDataStore.UploadContentToFile(string.Format(@"{0}/imu.yaml", remoteFolder), YamlTranslator.ConvertToYaml(new Imu()
                            {
                                RosTopic = "/imu0",
                                UpdateRate = Parent.SettingContainer.Settings.ImuSettings.UpdateRate,
                                AccelerometerNoiseDensity = Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AccelerometerNoiseDensity * Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AccelerometerNoiseDensitySafetyScale,
                                AccelerometerRandomWalk = Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AccelerometerRandomWalk * Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AccelerometerRandomWalkSafetyScale,
                                GyroscopeNoiseDensity = Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.GyroscopeNoiseDensity * Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.GyroscopeNoiseDensitySafetyScale,
                                GyroscopeRandomWalk = Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.GyroscopeRandomWalk * Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.GyroscopeRandomWalkSafetyScale
                            }));

                            remoteDataStore.UploadContentToFile(string.Format(@"{0}/cam.yaml", remoteFolder), YamlTranslator.ConvertToYaml(new CameraChain()
                            {
                                Cam0 = new Data.Storage.Model.Camera()
                                {
                                    CameraModel = CameraModel.Pinhole,
                                    DistortionModel = DistortionModel.Equidistant,
                                    DistortionCoefficients = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs.ToArray(),
                                    Fx = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fx,
                                    Fy = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fy,
                                    Cx = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cx,
                                    Cy = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cy,
                                    RosTopic = "/cam0/image_raw",
                                    Height = Parent.SettingContainer.Settings.CameraSettings.Height,
                                    Width = Parent.SettingContainer.Settings.CameraSettings.Width
                                }
                            }));

                            string options = string.Format(CultureInfo.InvariantCulture, "--dont-show-report --reprojection-sigma {0} {1}", Parent.SettingContainer.Settings.CalibrationSettings.ExtrinsicCalibrationSettings.ReprojectionSigma, Parent.SettingContainer.Settings.CalibrationSettings.ExtrinsicCalibrationSettings.TimeCalibration ? "--time-calibration" : "");

                            remoteDataStore.ExecuteCommands(new List<string>()
                            {
                                string.Format(@"cd {0}",remoteFolder),
                                string.Format(@"unzip {0} -d {1}",Path.GetFileName(localFile),Path.GetFileNameWithoutExtension(localFile)),
                                @"source ~/kalibr_workspace/devel/setup.bash",
                                string.Format(@"kalibr_bagcreater --folder {0} --output-bag {0}.bag", Path.GetFileNameWithoutExtension(localFile)),
                                string.Format(@"kalibr_calibrate_imu_camera --bag {0}.bag --cams cam.yaml --imu imu.yaml --imu-models {1} --target target.yaml {2}",Path.GetFileNameWithoutExtension(localFile),imuModel,options),
                                string.Format("pdftoppm report-imucam-{0}.pdf result -png",Path.GetFileNameWithoutExtension(localFile))
                            }, expactString);

                            CameraChain cameraChain = YamlTranslator.ConvertFromYaml<CameraChain>(remoteDataStore.DownloadFileToMemory(string.Format("{0}/camchain-imucam-{1}.yaml", remoteFolder, Path.GetFileNameWithoutExtension(localFile))));
                            ImuCain imuChain = YamlTranslator.ConvertFromYaml<ImuCain>(remoteDataStore.DownloadFileToMemory(string.Format("{0}/imu-{1}.yaml", remoteFolder, Path.GetFileNameWithoutExtension(localFile))));

                            List<string> availablePlots = new List<string>();

                            foreach (string file in remoteDataStore.GetAllFileNames(remoteFolder))
                            {
                                if (file.Contains(".csv"))
                                {
                                    availablePlots.Add(file.Replace(string.Format("data-imucam-{0}.", Path.GetFileNameWithoutExtension(localFile)), "").Replace(".csv", ""));
                                }
                            }

                            Dictionary<string, string> plotDataImu = new Dictionary<string, string>();

                            foreach (string plot in availablePlots)
                            {
                                plotDataImu.Add(plot, remoteDataStore.DownloadFileToMemory(string.Format("{0}/data-imucam-{1}.{2}.csv", remoteFolder, Path.GetFileNameWithoutExtension(localFile), plot)));
                            }

                            string outputPath = Path.Combine(Path.GetTempPath(), "firefly", guid);

                            if (!Directory.Exists(outputPath))
                            {
                                Directory.CreateDirectory(outputPath);
                            }

                            foreach (string file in remoteDataStore.GetAllFileNames(remoteFolder))
                            {
                                if (!file.Contains(".bag") && !file.Contains(".ffc"))
                                    remoteDataStore.DownloadFile(string.Format("{0}/{1}", remoteFolder, file), Path.Combine(outputPath, file));
                            }

                            CsvToMatlabWritter csvToMatlabWritter = new CsvToMatlabWritter(Path.Combine(outputPath, "plot-data.mat"));
                            csvToMatlabWritter.Write(plotDataImu, "ExtrinsicCalibrationData");
                            csvToMatlabWritter.Save();

                            ShowResults(outputPath, cameraChain, imuChain);

                            remoteDataStore.ExecuteCommands(new List<string>()
                            {
                                string.Format(@"rm -r {0}",remoteFolder)
                            }, expactString);

                            await controller.CloseAsync();
                        }
                    });
                }, null);
            };
        }

        private Action<ImageResultViewerDialogModel> OnShowResultDialogClose(CustomDialog customDialog, string path)
        {
            return obj =>
            {
                Parent.SyncContext.Send(d =>
                {
                    Parent.DialogCoordinator.HideMetroDialogAsync(Parent, customDialog);
                    Directory.Delete(path, true);
                }, null);
            };
        }

        private Action<ImageResultViewerDialogModel> OnShowResultDialogExport(CustomDialog customDialog, string path)
        {
            return obj =>
            {
                Parent.SyncContext.Send(d =>
                {
                    System.Windows.Forms.SaveFileDialog saveFileDialog = saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                    saveFileDialog.Filter = "Archive (*.zip) | *.zip";
                    if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        ZipFile.CreateFromDirectory(path, saveFileDialog.FileName, CompressionLevel.Optimal, false);
                    }
                }, null);
            };
        }

        private Action<ImageResultViewerDialogModel> OnShowResultDialogSave(CustomDialog customDialog, CameraChain cameraChain, ImuCain imuChain)
        {
            return obj =>
            {
                Parent.SyncContext.Send(d =>
                {
                    Parent.SettingContainer.Settings.CalibrationSettings.ExtrinsicCalibrationSettings.T_Cam_Imu = Matrix<double>.Build.DenseOfColumnArrays(cameraChain.Cam0.T_Cam_Imu).ToArray();
                    Parent.SettingContainer.Settings.CalibrationSettings.ExtrinsicCalibrationSettings.R_Acc_Gyro = Matrix<double>.Build.DenseOfColumnArrays(imuChain.Imu0.Gyroscopes.C).Transpose().ToArray();
                    Parent.SettingContainer.Settings.CalibrationSettings.ExtrinsicCalibrationSettings.M_Inv_Acc = Matrix<double>.Build.DenseOfColumnArrays(imuChain.Imu0.Accelerometers.M).Inverse().ToArray();
                    Parent.SettingContainer.Settings.CalibrationSettings.ExtrinsicCalibrationSettings.M_Inv_Gyro = Matrix<double>.Build.DenseOfColumnArrays(imuChain.Imu0.Gyroscopes.M).Inverse().ToArray();
                    Parent.UpdateSettings(false);
                }, null);
            };
        }

        private void ShowResults(string path, CameraChain cameraChain, ImuCain imuChain)
        {
            Parent.SyncContext.Send(f =>
            {
                CustomDialog customDialog;

                customDialog = new CustomDialog() { Title = "Calibration results" };

                var dataContext = new ImageResultViewerDialogModel(OnShowResultDialogClose(customDialog, path), OnShowResultDialogSave(customDialog, cameraChain, imuChain), OnShowResultDialogExport(customDialog, path), path, "result-*.png");
                Parent.ReplayViewModel.Refresh();
                customDialog.Content = new ImageResultViewerDialog
                {
                    DataContext = dataContext
                };

                Parent.DialogCoordinator.ShowMetroDialogAsync(Parent, customDialog);
            }, null);
        }
    }
}