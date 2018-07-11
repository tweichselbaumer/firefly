using FireFly.Utilities;
using FireFly.ViewModels;
using OxyPlot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace FireFly.Models
{
    public class LineSeriesContainer : DependencyObject
    {
        public static readonly DependencyProperty MajorStepYProperty =
            DependencyProperty.Register("MajorStepY", typeof(double), typeof(LineSeriesContainer), new PropertyMetadata(10.0));

        public static readonly DependencyProperty MaximumYProperty =
            DependencyProperty.Register("MaximumY", typeof(double), typeof(LineSeriesContainer), new PropertyMetadata(10.0));

        public static readonly DependencyProperty MinimumYProperty =
            DependencyProperty.Register("MinimumY", typeof(double), typeof(LineSeriesContainer), new PropertyMetadata(10.0));

        public static readonly DependencyProperty MinorStepYProperty =
            DependencyProperty.Register("MinorStepY", typeof(double), typeof(LineSeriesContainer), new PropertyMetadata(10.0));

        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(RangeObservableCollection<DataPoint>), typeof(LineSeriesContainer), new PropertyMetadata(new RangeObservableCollection<DataPoint>()));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(LineSeriesContainer), new PropertyMetadata(""));

        private long _AddedPoints = 0;
        private double _MaxY = 0.0;
        private double _MinY = 0.0;
        private DataPlotViewModel _Parent;

        private BlockingCollection<DataPoint> _BlockingCollection = new BlockingCollection<DataPoint>();

        private double _TimeOffset = 0;

        public LineSeriesContainer(DataPlotViewModel parent, string title)
        {
            Parent = parent;
            Title = title;
            Points = new RangeObservableCollection<DataPoint>();
        }

        public double MajorStepY
        {
            get { return (double)GetValue(MajorStepYProperty); }
            set { SetValue(MajorStepYProperty, value); }
        }

        public double MaximumY
        {
            get { return (double)GetValue(MaximumYProperty); }
            set { SetValue(MaximumYProperty, value); }
        }

        public double MinimumY
        {
            get { return (double)GetValue(MinimumYProperty); }
            set { SetValue(MinimumYProperty, value); }
        }

        public double MinorStepY
        {
            get { return (double)GetValue(MinorStepYProperty); }
            set { SetValue(MinorStepYProperty, value); }
        }

        public DataPlotViewModel Parent
        {
            get
            {
                return _Parent;
            }

            set
            {
                _Parent = value;
            }
        }

        public RangeObservableCollection<DataPoint> Points
        {
            get { return (RangeObservableCollection<DataPoint>)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public void AddDataPoint(double time, double value)
        {
            if (_AddedPoints % 2000 == 0)
            {
                _TimeOffset = time;
            }

            time -= _TimeOffset;
            _BlockingCollection.Add(new DataPoint(time, value));
            _AddedPoints++;
        }

        public void DrawPoints()
        {
            int count = _BlockingCollection.Count;
            bool clear = false;
            List<DataPoint> points = new List<DataPoint>();


            for (int i = 0; i < count; i++)
            {
                DataPoint point = _BlockingCollection.Take();
                if (point.Y > _MaxY)
                {
                    _MaxY = point.Y;
                }
                else if (point.Y < _MinY)
                {
                    _MinY = point.Y;
                }

                points.Add(point);
                if (point.X == 0)
                {
                    clear = true;
                    points.Clear();
                    _MinY = 0;
                    _MaxY = 0;
                }
            }

            Parent.Parent.SyncContext.Post(o =>
            {
                if (clear)
                    Points.Clear();
                Points.AddRange(points);
                if (Points.Count > 0)
                {
                    double max = Math.Max(Math.Abs(_MinY), Math.Abs(_MaxY));
                    int pow = (int)Math.Ceiling(Math.Log(max) / Math.Log(2));

                    double newMaxY = Math.Pow(2, pow);
                    if (MaximumY != newMaxY)
                    {
                        MaximumY = newMaxY;
                        MinimumY = -newMaxY;
                        MajorStepY = newMaxY / 2;
                        MinorStepY = newMaxY / (2 * 5);
                    }
                }
            }, null);
        }
    }
}