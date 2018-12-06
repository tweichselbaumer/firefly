using FireFly.Command;
using FireFly.Data.Storage;
using FireFly.Utilities;
using FireFly.VI.Calibration;
using MahApps.Metro.Controls.Dialogs;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace FireFly.ViewModels
{
    public class ImuCalibrationViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty AccelXProperty =
            DependencyProperty.Register("AccelX", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty AccelYProperty =
            DependencyProperty.Register("AccelY", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty AccelZProperty =
            DependencyProperty.Register("AccelZ", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty GyroXProperty =
            DependencyProperty.Register("GyroX", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty GyroYProperty =
            DependencyProperty.Register("GyroY", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty GyroZProperty =
            DependencyProperty.Register("GyroZ", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty RandomWalkAccelProperty =
          DependencyProperty.Register("RandomWalkAccel", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty RandomWalkGyroProperty =
                    DependencyProperty.Register("RandomWalkGyro", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty RandomWalkSlopeAccelProperty =
            DependencyProperty.Register("RandomWalkSlopeAccel", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty RandomWalkSlopeGyroProperty =
                    DependencyProperty.Register("RandomWalkSlopeGyro", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty WhiteNoiseAccelProperty =
             DependencyProperty.Register("WhiteNoiseAccel", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty WhiteNoiseGyroProperty =
                     DependencyProperty.Register("WhiteNoiseGyro", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty WhiteNoiseSlopeAccelProperty =
            DependencyProperty.Register("WhiteNoiseSlopeAccel", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty WhiteNoiseSlopeGyroProperty =
                    DependencyProperty.Register("WhiteNoiseSlopeGyro", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public ImuCalibrationViewModel(MainViewModel parent) : base(parent)
        {
            GyroX = new RangeObservableCollection<DataPoint>();
            GyroY = new RangeObservableCollection<DataPoint>();
            GyroZ = new RangeObservableCollection<DataPoint>();
            WhiteNoiseSlopeGyro = new RangeObservableCollection<DataPoint>();
            WhiteNoiseSigmaGyro = new RangeObservableCollection<DataPoint>();
            RandomWalkSlopeGyro = new RangeObservableCollection<DataPoint>();
            RandomWalkGyro = new RangeObservableCollection<DataPoint>();

            AccelX = new RangeObservableCollection<DataPoint>();
            AccelY = new RangeObservableCollection<DataPoint>();
            AccelZ = new RangeObservableCollection<DataPoint>();
            WhiteNoiseSlopeAccel = new RangeObservableCollection<DataPoint>();
            WhiteNoiseSigmaAccel = new RangeObservableCollection<DataPoint>();
            RandomWalkSlopeAccel = new RangeObservableCollection<DataPoint>();
            RandomWalkAccel = new RangeObservableCollection<DataPoint>();
        }

        public RangeObservableCollection<DataPoint> AccelX
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(AccelXProperty); }
            set { SetValue(AccelXProperty, value); }
        }

        public RangeObservableCollection<DataPoint> AccelY
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(AccelYProperty); }
            set { SetValue(AccelYProperty, value); }
        }

        public RangeObservableCollection<DataPoint> AccelZ
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(AccelZProperty); }
            set { SetValue(AccelZProperty, value); }
        }

        public RangeObservableCollection<DataPoint> GyroX
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(GyroXProperty); }
            set { SetValue(GyroXProperty, value); }
        }

        public RangeObservableCollection<DataPoint> GyroY
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(GyroYProperty); }
            set { SetValue(GyroYProperty, value); }
        }

        public RangeObservableCollection<DataPoint> GyroZ
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(GyroZProperty); }
            set { SetValue(GyroZProperty, value); }
        }

        public RangeObservableCollection<DataPoint> RandomWalkAccel
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(RandomWalkAccelProperty); }
            set { SetValue(RandomWalkAccelProperty, value); }
        }

        public RangeObservableCollection<DataPoint> RandomWalkGyro
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(RandomWalkGyroProperty); }
            set { SetValue(RandomWalkGyroProperty, value); }
        }

        public RangeObservableCollection<DataPoint> RandomWalkSlopeAccel
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(RandomWalkSlopeAccelProperty); }
            set { SetValue(RandomWalkSlopeAccelProperty, value); }
        }

        public RangeObservableCollection<DataPoint> RandomWalkSlopeGyro
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(RandomWalkSlopeGyroProperty); }
            set { SetValue(RandomWalkSlopeGyroProperty, value); }
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

        public RangeObservableCollection<DataPoint> WhiteNoiseSigmaAccel
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(WhiteNoiseAccelProperty); }
            set { SetValue(WhiteNoiseAccelProperty, value); }
        }

        public RangeObservableCollection<DataPoint> WhiteNoiseSigmaGyro
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(WhiteNoiseGyroProperty); }
            set { SetValue(WhiteNoiseGyroProperty, value); }
        }

        public RangeObservableCollection<DataPoint> WhiteNoiseSlopeAccel
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(WhiteNoiseSlopeAccelProperty); }
            set { SetValue(WhiteNoiseSlopeAccelProperty, value); }
        }

        public RangeObservableCollection<DataPoint> WhiteNoiseSlopeGyro
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(WhiteNoiseSlopeGyroProperty); }
            set { SetValue(WhiteNoiseSlopeGyroProperty, value); }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();

            bool changed = false;
            if (WhiteNoiseSigmaAccel.Count == 1)
                changed |= Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AccelerometerNoiseDensity != WhiteNoiseSigmaAccel[0].Y;
            else
                changed = true;

            if (WhiteNoiseSigmaGyro.Count == 1)
                changed |= Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.GyroscopeNoiseDensity != WhiteNoiseSigmaGyro[0].Y;
            else
                changed = true;

            if (RandomWalkAccel.Count == 1)
                changed |= Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AccelerometerRandomWalk != RandomWalkAccel[0].Y;
            else
                changed = true;

            if (RandomWalkGyro.Count == 1)
                changed |= Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.GyroscopeRandomWalk != RandomWalkGyro[0].Y;
            else
                changed = true;

            if (changed)
            {
                /*
                 * Gyro
                 */
                (List<double> timesGyroWN, List<double> sigmasGyroWN) = CalculateSlope(Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationTime, Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.GyroscopeNoiseDensity, -0.5, 1 / Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.SampleTime, 1, 1);
                (List<double> timesGyroRW, List<double> sigmasGyroRW) = CalculateSlope(Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationTime, Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.GyroscopeRandomWalk, +0.5, 1000, 6000, 3);

                AddPoints(timesGyroWN, sigmasGyroWN, WhiteNoiseSlopeGyro);
                WhiteNoiseSigmaGyro.Clear();
                WhiteNoiseSigmaGyro.Add(new DataPoint(1, Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.GyroscopeNoiseDensity));

                AddPoints(timesGyroRW, sigmasGyroRW, RandomWalkSlopeGyro);
                RandomWalkGyro.Clear();
                RandomWalkGyro.Add(new DataPoint(3, Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.GyroscopeRandomWalk));

                AddPoints(Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationTime, Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationGyroscopeX, GyroX);
                AddPoints(Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationTime, Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationGyroscopeY, GyroY);
                AddPoints(Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationTime, Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationGyroscopeZ, GyroZ);

                /*
                 * Accelerometer
                 */
                (List<double> timesAccWN, List<double> sigmasAccWN) = CalculateSlope(Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationTime, Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AccelerometerNoiseDensity, -0.5, 1 / Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.SampleTime, 1, 1);
                (List<double> timesAccRW, List<double> sigmasAccRW) = CalculateSlope(Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationTime, Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AccelerometerRandomWalk, +0.5, 1000, 6000, 3);

                AddPoints(timesAccWN, sigmasAccWN, WhiteNoiseSlopeAccel);
                WhiteNoiseSigmaAccel.Clear();
                WhiteNoiseSigmaAccel.Add(new DataPoint(1, Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AccelerometerNoiseDensity));

                AddPoints(timesAccRW, sigmasAccRW, RandomWalkSlopeAccel);
                RandomWalkAccel.Clear();
                RandomWalkAccel.Add(new DataPoint(3, Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AccelerometerRandomWalk));

                AddPoints(Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationTime, Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationAccelerometerX, AccelX);
                AddPoints(Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationTime, Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationAccelerometerY, AccelY);
                AddPoints(Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationTime, Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationAccelerometerZ, AccelZ);
            }
        }

        private static double CalculateSigma(List<double> times, List<double> sigmasGx, List<double> sigmasGy, List<double> sigmasGz, double slope, double start, double end, double t)
        {
            List<int> ids = times.Where(c => c > start && c < end).Select(d => times.IndexOf(d)).ToList();
            List<double> x = ids.Select(i => times[i]).ToList();
            List<double> y = ids.Select(i => (sigmasGx[i] + sigmasGy[i] + sigmasGz[i]) / 3).ToList();

            double bw = y.Select((c, i) => Math.Log10(y[i]) - slope * Math.Log10(x[i])).Sum() / y.Count;

            return Math.Pow(10, slope * Math.Log10(t) + bw);
        }

        private static (List<double> times, List<double> sigmas) CalculateSlope(List<double> times, double sigma, double slope, double start, double end, double t)
        {
            List<int> ids = times.Where(c => c > start && c < end).Select(d => times.IndexOf(d)).ToList();
            List<double> x = ids.Select(i => times[i]).ToList();

            double bw = Math.Log10(sigma) - slope * Math.Log10(t);

            return (x, x.Select(c => Math.Pow(10, slope * Math.Log10(c) + bw)).ToList());
        }

        private void AddPoints(List<double> times, List<double> sigmas, RangeObservableCollection<DataPoint> collection)
        {
            List<DataPoint> temp = new List<DataPoint>();
            collection.Clear();
            for (int i = 0; i < sigmas.Count; i++)
            {
                temp.Add(new DataPoint(times[i], sigmas[i]));
            }
            collection.AddRange(temp);
        }

        private Task DoStartCalibration(object o)
        {
            return Task.Factory.StartNew(async () =>
            {
                bool open = false;
                string filename = "";

                Parent.SyncContext.Send(c =>
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Matlab (*.mat) | *.mat";
                    open = openFileDialog.ShowDialog() == DialogResult.OK;
                    filename = Path.GetFullPath(openFileDialog.FileName);
                }, null);

                if (open)
                {

                    MetroDialogSettings settings = new MetroDialogSettings()
                    {
                        AnimateShow = false,
                        AnimateHide = false
                    };

                    var controller = await Parent.DialogCoordinator.ShowProgressAsync(Parent, "Please wait...", "Calculation Allan Deviation!", settings: Parent.MetroDialogSettings);

                    controller.SetCancelable(false);
                    controller.SetIndeterminate();

                    RawMatlabImporter matlabImporter = new RawMatlabImporter(filename, MatlabFormat.Imu0);
                    (List<double> time, List<double> gyrox, List<double> gyroy, List<double> gyroz, List<double> accx, List<double> accy, List<double> accz) = matlabImporter.Load();

                    double fs = Math.Floor(1 / (time.Take(time.Count - 1).Select((v, i) => time[i + 1] - v).Sum() / (time.Count - 1)));

                    (List<double> times, List<double> sigmasGx) = AllenDeviation.Calculate(gyrox.Select(c => c / 180 * Math.PI).ToList(), fs);
                    (List<double> _, List<double> sigmasGy) = AllenDeviation.Calculate(gyroy.Select(c => c / 180 * Math.PI).ToList(), fs);
                    (List<double> _, List<double> sigmasGz) = AllenDeviation.Calculate(gyroz.Select(c => c / 180 * Math.PI).ToList(), fs);

                    double sigmaGyroWN = CalculateSigma(times, sigmasGx, sigmasGy, sigmasGz, -0.5, 1 / fs, 1, 1);
                    double sigmaGyroRW = CalculateSigma(times, sigmasGx, sigmasGy, sigmasGz, +0.5, 1000, 6000, 3);

                    (List<double> _, List<double> sigmasAx) = AllenDeviation.Calculate(accx.ToList(), fs);
                    (List<double> _, List<double> sigmasAy) = AllenDeviation.Calculate(accy.ToList(), fs);
                    (List<double> _, List<double> sigmasAz) = AllenDeviation.Calculate(accz.ToList(), fs);

                    double sigmaAccWN = CalculateSigma(times, sigmasAx, sigmasAy, sigmasAz, -0.5, 1 / fs, 1, 1);
                    double sigmaAccRW = CalculateSigma(times, sigmasAx, sigmasAy, sigmasAz, +0.5, 1000, 6000, 3);

                    Parent.SyncContext.Post(async f =>
                    {
                        Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationGyroscopeX = sigmasGx;
                        Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationGyroscopeY = sigmasGy;
                        Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationGyroscopeZ = sigmasGz;

                        Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationAccelerometerX = sigmasAx;
                        Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationAccelerometerY = sigmasAy;
                        Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationAccelerometerZ = sigmasAz;

                        Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AllanDeviationTime = times;
                        Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.SampleTime = fs;

                        Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AccelerometerNoiseDensity = sigmaAccWN;
                        Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.AccelerometerRandomWalk = sigmaAccRW;

                        Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.GyroscopeNoiseDensity = sigmaGyroWN;
                        Parent.SettingContainer.Settings.CalibrationSettings.ImuCalibration.GyroscopeRandomWalk = sigmaGyroRW;

                        Parent.UpdateSettings(false);

                        await controller.CloseAsync();
                    }, null);
                }
            });
        }
    }
}