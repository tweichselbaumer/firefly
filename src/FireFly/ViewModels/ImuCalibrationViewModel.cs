using FireFly.Command;
using FireFly.Data.Storage;
using FireFly.Utilities;
using FireFly.VI.Calibration;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FireFly.ViewModels
{
    public class ImuCalibrationViewModel : AbstractViewModel
    {
        public static readonly DependencyProperty GyroXProperty =
            DependencyProperty.Register("GyroX", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty GyroYProperty =
            DependencyProperty.Register("GyroY", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty GyroZProperty =
            DependencyProperty.Register("GyroZ", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty RandomWalkGyroProperty =
            DependencyProperty.Register("RandomWalkGyro", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty RandomWalkSlopeGyroProperty =
            DependencyProperty.Register("RandomWalkSlopeGyro", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty WhiteNoiseGyroProperty =
             DependencyProperty.Register("WhiteNoiseGyro", typeof(RangeObservableCollection<DataPoint>), typeof(ImuCalibrationViewModel), new PropertyMetadata(null));

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

        public RangeObservableCollection<DataPoint> RandomWalkGyro
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(RandomWalkGyroProperty); }
            set { SetValue(RandomWalkGyroProperty, value); }
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

        public RangeObservableCollection<DataPoint> WhiteNoiseSigmaGyro
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(WhiteNoiseGyroProperty); }
            set { SetValue(WhiteNoiseGyroProperty, value); }
        }

        public RangeObservableCollection<DataPoint> WhiteNoiseSlopeGyro
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(WhiteNoiseSlopeGyroProperty); }
            set { SetValue(WhiteNoiseSlopeGyroProperty, value); }
        }

        internal override void SettingsUpdated()
        {
            base.SettingsUpdated();
        }

        private static (double sigma, List<double> times, List<double> sigmas) CalculateSlope(List<double> times, List<double> sigmasGx, List<double> sigmasGy, List<double> sigmasGz, double slope, double start, double end, double t)
        {
            List<int> ids = times.Where(c => c > start && c < end).Select(d => times.IndexOf(d)).ToList();
            List<double> x = ids.Select(i => times[i]).ToList();
            List<double> y = ids.Select(i => (sigmasGx[i] + sigmasGy[i] + sigmasGz[i]) / 3).ToList();

            double bw = y.Select((c, i) => Math.Log10(y[i]) - slope * Math.Log10(x[i])).Sum() / y.Count;
            double sigma = Math.Pow(10, slope * Math.Log10(t) + bw);

            return (sigma, x, x.Select(c => Math.Pow(10, slope * Math.Log10(c) + bw)).ToList());
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
            return Task.Factory.StartNew(() =>
            {
                string filename = @"C:\Users\thoma\OneDrive\Mechatronik\Masterarbeit\Matlab\data\static_imu_4g_500dps.mat";

                MatlabImporter matlabImporter = new MatlabImporter(filename, MatlabFormat.Imu0);
                (List<double> time, List<double> gyrox, List<double> gyroy, List<double> gyroz, List<double> accx, List<double> accy, List<double> accz) = matlabImporter.Load();

                double fs = Math.Floor(1 / (time.Take(time.Count - 1).Select((v, i) => time[i + 1] - v).Sum() / (time.Count - 1)));

                (List<double> times, List<double> sigmasGx) = AllenDeviation.Calculate(gyrox.Select(c => c / 180 * Math.PI).ToList(), fs);
                (List<double> _, List<double> sigmasGy) = AllenDeviation.Calculate(gyroy.Select(c => c / 180 * Math.PI).ToList(), fs);
                (List<double> _, List<double> sigmasGz) = AllenDeviation.Calculate(gyroz.Select(c => c / 180 * Math.PI).ToList(), fs);

                (double sigmaGyroWN, List<double> timesGyroWN, List<double> sigmasGyroWN) = CalculateSlope(times, sigmasGx, sigmasGy, sigmasGz, -0.5, 1 / fs, 1, 1);
                (double sigmaGyroRW, List<double> timesGyroRW, List<double> sigmasGyroRW) = CalculateSlope(times, sigmasGx, sigmasGy, sigmasGz, +0.5, 1000, 6000, 3);

                (List<double> _, List<double> sigmasAx) = AllenDeviation.Calculate(accx.ToList(), fs);
                (List<double> _, List<double> sigmasAy) = AllenDeviation.Calculate(accy.ToList(), fs);
                (List<double> _, List<double> sigmasAz) = AllenDeviation.Calculate(accz.ToList(), fs);

                (double sigmaAccWN, List<double> timesAccWN, List<double> sigmasAccWN) = CalculateSlope(times, sigmasGx, sigmasGy, sigmasGz, -0.5, 1 / fs, 1, 1);
                (double sigmaAccRW, List<double> timesAccRW, List<double> sigmasAccRW) = CalculateSlope(times, sigmasAx, sigmasAy, sigmasAz, +0.5, 1000, 6000, 3);

                Parent.SyncContext.Post(f =>
                {
                    AddPoints(timesGyroWN, sigmasGyroWN, WhiteNoiseSlopeGyro);
                    WhiteNoiseSigmaGyro.Clear();
                    WhiteNoiseSigmaGyro.Add(new DataPoint(1, sigmaGyroWN));

                    AddPoints(timesGyroRW, sigmasGyroRW, RandomWalkSlopeGyro);
                    RandomWalkGyro.Clear();
                    RandomWalkGyro.Add(new DataPoint(3, sigmaGyroRW));

                    AddPoints(times, sigmasGx, GyroX);
                    AddPoints(times, sigmasGy, GyroY);
                    AddPoints(times, sigmasGz, GyroZ);
                }, null);
            });
        }
    }
}