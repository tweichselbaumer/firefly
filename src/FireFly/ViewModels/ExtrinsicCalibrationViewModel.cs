using FireFly.Command;
using FireFly.CustomDialogs;
using FireFly.Data.Storage;
using FireFly.Data.Storage.Model;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FireFly.ViewModels
{
    public class ExtrinsicCalibrationViewModel : AbstractViewModel
    {
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

        private Task DoStartCalibration(object o)
        {
            return Task.Factory.StartNew(async () =>
            {
                Parent.SyncContext.Post(f =>
                {
                    CustomDialog customDialog;

                    customDialog = new CustomDialog() { Title = "Select extrinsic calibration file" };

                    var dataContext = new ReplaySelectDialogModel(obj =>
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
                                    RemoteDataStore remoteDataStore = new RemoteDataStore(Parent.SettingContainer.Settings.ConnectionSettings.IpAddress, Parent.SettingContainer.Settings.ConnectionSettings.Username, Parent.SettingContainer.Settings.ConnectionSettings.Password);

                                    string guid = Guid.NewGuid().ToString();
                                    string localFile = "";
                                    string remoteFile = "";
                                    string remoteFolder = string.Format(@"/var/tmp/firefly/{0}", guid);
                                    string expactString = string.Format("{0}@{1}:.{{0,}}[$]", Parent.SettingContainer.Settings.ConnectionSettings.Username, Parent.SettingContainer.Settings.ConnectionSettings.Hostname);

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
                                    //TODO:
                                    remoteDataStore.UploadContentToFile(string.Format(@"{0}/target.yaml", remoteFolder), YamlTranslator.ConvertToYaml(new CalibrationTarget()
                                    {
                                        TargetType = CalibrationTargetType.Aprilgrid,
                                        TagSize = 0.11611,
                                        TagSpacing = 0.3,
                                        TagCols = 5,
                                        TagRows = 7
                                    }));

                                    remoteDataStore.UploadContentToFile(string.Format(@"{0}/imu.yaml", remoteFolder), YamlTranslator.ConvertToYaml(new Imu()
                                    {
                                        RosTopic = "/imu0",
                                        UpdateRate = 200.0,
                                        AccelerometerNoiseDensity = 0.0028,
                                        AccelerometerRandomWalk = 0.00086,
                                        GyroscopeNoiseDensity = 0.00016,
                                        GyroscopeRandomWalk = 0.000022
                                    }));

                                    remoteDataStore.UploadContentToFile(string.Format(@"{0}/cam.yaml", remoteFolder), YamlTranslator.ConvertToYaml(new CameraChain()
                                    {
                                        Cam0 = new Camera()
                                        {
                                            CameraModel = CameraModel.Pinhole,
                                            DistortionModel = DistortionModel.Equidistant,
                                            DistortionCoefficients = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.DistCoeffs.ToArray(),
                                            Fx = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fx,
                                            Fy = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Fy,
                                            Cx = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cx,
                                            Cy = Parent.SettingContainer.Settings.CalibrationSettings.IntrinsicCalibrationSettings.Cy,
                                            RosTopic = "/cam0/image_raw",
                                            Height = 512,
                                            Width = 512
                                        }
                                    }));

                                    remoteDataStore.ExecuteCommands(new List<string>()
                                    {
                                        string.Format(@"cd {0}",remoteFolder),
                                        string.Format(@"unzip {0} -d {1}",Path.GetFileName(localFile),Path.GetFileNameWithoutExtension(localFile)),
                                        @"source ~/kalibr_workspace/devel/setup.bash",
                                        string.Format(@"kalibr_bagcreater --folder {0} --output-bag {0}.bag", Path.GetFileNameWithoutExtension(localFile)),
                                        string.Format(@"kalibr_calibrate_imu_camera --bag {0}.bag --cams cam.yaml --imu imu.yaml --imu-models {1} --target target.yaml --time-calibration --dont-show-report",Path.GetFileNameWithoutExtension(localFile),"scale-misalignment-size-effect"),
                                        string.Format("pdftoppm report-imucam-{0}.pdf result -png",Path.GetFileNameWithoutExtension(localFile))
                                     }, expactString);

                                    CameraChain cameraChain = YamlTranslator.ConvertFromYaml<CameraChain>(remoteDataStore.DownloadFileToMemory(string.Format("{0}/camchain-imucam-{1}.yaml", remoteFolder, Path.GetFileNameWithoutExtension(localFile))));
                                    ImuCain imuChain = YamlTranslator.ConvertFromYaml<ImuCain>(remoteDataStore.DownloadFileToMemory(string.Format("{0}/imu-{1}.yaml", remoteFolder, Path.GetFileNameWithoutExtension(localFile))));

                                    string outputPath = Path.Combine(Path.GetTempPath(), "firefly", guid);

                                    if (!Directory.Exists(outputPath))
                                    {
                                        Directory.CreateDirectory(outputPath);
                                    }

                                    foreach (string file in remoteDataStore.GetAllFileNames(remoteFolder))
                                    {
                                        remoteDataStore.DownloadFile(string.Format("{0}/{1}", remoteFolder, file), Path.Combine(outputPath, file));
                                    }

                                    remoteDataStore.ExecuteCommands(new List<string>()
                                    {
                                        string.Format(@"rm -r {0}",remoteFolder)
                                     }, expactString);

                                    Directory.Delete(outputPath, true);

                                    await controller.CloseAsync();
                                }
                            });
                        }, null);
                    });
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
    }
}