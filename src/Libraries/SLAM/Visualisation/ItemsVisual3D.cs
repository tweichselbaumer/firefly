using HelixToolkit.Wpf;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;

namespace FireFly.VI.SLAM.Visualisation
{
    public class ItemsVisual3D : ModelVisual3D
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<Visual3D>), typeof(ItemsVisual3D), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        public ItemsVisual3D()
        {
            ItemsSource = new ObservableCollection<Visual3D>();
            ItemsSource.CollectionChanged += ItemsSource_CollectionChanged;
        }

        public ObservableCollection<Visual3D> ItemsSource
        {
            get { return (ObservableCollection<Visual3D>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldObservableCollection = e.OldValue as INotifyCollectionChanged;
            if (oldObservableCollection != null)
            {
                oldObservableCollection.CollectionChanged -= (d as ItemsVisual3D).ItemsSource_CollectionChanged;
            }

            var observableCollection = e.NewValue as INotifyCollectionChanged;
            if (observableCollection != null)
            {
                observableCollection.CollectionChanged += (d as ItemsVisual3D).ItemsSource_CollectionChanged;
            }

            if ((d as ItemsVisual3D).ItemsSource != null)
            {
                (d as ItemsVisual3D).AddItems((d as ItemsVisual3D).ItemsSource);
            }
        }

        private void AddItems(IEnumerable items)
        {
            if (items != null && items.Cast<Visual3D>().Any())
            {
                foreach (var item in items)
                {
                    Children.Add(item as Visual3D);
                }
            }
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveItems(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    RemoveItems(e.OldItems);
                    AddItems(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Children.Clear();
                    AddItems(ItemsSource);
                    break;

                default:
                    break;
            }
        }

        private void RemoveItems(IEnumerable items)
        {
            if (items == null)
                return;

            foreach (var rem in items)
            {
                Children.Remove(rem as Visual3D);
            }
        }
    }
}