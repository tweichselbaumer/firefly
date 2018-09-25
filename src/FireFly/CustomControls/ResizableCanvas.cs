using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace FireFly.CustomControls
{
    public class ResizableCanvas : Canvas
    {
        #region Fields

        private Point _DragStartMousePosion;
        private bool _IsDragging = false;
        private FrameworkElement _SelectedItem;
        private ResizingAdorner _SelectedItemAdorner;

        #endregion Fields

        #region Methods

        public static double GetBottom(FrameworkElement element)
        {
            return GetTop(element) + element.Height;
        }

        public static Point GetCenter(FrameworkElement element)
        {
            Point result = new Point();
            result.X = GetLeft(element) + (GetRight(element) - GetLeft(element)) / 2;
            result.Y = GetTop(element) + (GetBottom(element) - GetTop(element)) / 2;
            return result;
        }

        public static Point GetDistance(FrameworkElement element, FrameworkElement reference)
        {
            Point result = new Point();

            result.X = Math.Abs(GetCenter(element).X - GetCenter(reference).X);
            result.Y = Math.Abs(GetCenter(element).Y - GetCenter(reference).Y);

            return result;
        }

        public static double GetLeft(FrameworkElement element)
        {
            if (element.Parent is Canvas)
            {
                return Canvas.GetLeft(element);
            }
            else
            {
                Point position = element.PointToScreen(new Point(0, 0));

                FrameworkElement control = element;

                while (!((control = (FrameworkElement)control.Parent) is Canvas) || control == null) { }

                if (control != null)
                {
                    Point controlPosition = control.PointToScreen(new Point(0, 0));

                    position.X -= controlPosition.X;
                    position.Y -= controlPosition.Y;
                    return position.X;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static Point GetMiddleBottom(FrameworkElement element)
        {
            Point result = new Point();
            result.X = GetLeft(element) + (GetRight(element) - GetLeft(element)) / 2;
            result.Y = GetBottom(element);
            return result;
        }

        public static Point GetMiddleLeft(FrameworkElement element)
        {
            Point result = new Point();
            result.X = GetLeft(element);
            result.Y = GetTop(element) + (GetBottom(element) - GetTop(element)) / 2;
            return result;
        }

        public static Point GetMiddleRight(FrameworkElement element)
        {
            Point result = new Point();
            result.X = GetRight(element);
            result.Y = GetTop(element) + (GetBottom(element) - GetTop(element)) / 2;
            return result;
        }

        public static Point GetMiddleTop(FrameworkElement element)
        {
            Point result = new Point();
            result.X = GetLeft(element) + (GetRight(element) - GetLeft(element)) / 2;
            result.Y = GetTop(element);
            return result;
        }

        public static double GetRight(FrameworkElement element)
        {
            return GetLeft(element) + element.Width;
        }

        public static double GetTop(FrameworkElement element)
        {
            if (element.Parent is Canvas)
            {
                return Canvas.GetTop(element);
            }
            else
            {
                Point position = element.PointToScreen(new Point(0, 0));

                FrameworkElement control = element;

                while (!((control = (FrameworkElement)control.Parent) is Canvas) || control == null) { }

                if (control != null)
                {
                    Point controlPosition = control.PointToScreen(new Point(0, 0));

                    position.X -= controlPosition.X;
                    position.Y -= controlPosition.Y;
                    return position.Y;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static bool IsAbove(FrameworkElement element, FrameworkElement reference)
        {
            Point center1 = GetCenter(element);
            Point center2 = GetCenter(reference);

            if (center1.Y < center2.Y)
            {
                return true;
            }
            else return false;
        }

        public static bool IsLeft(FrameworkElement element, FrameworkElement reference)
        {
            Point center1 = GetCenter(element);
            Point center2 = GetCenter(reference);

            if (center1.X < center2.X)
            {
                return true;
            }
            else return false;
        }

        public static bool IsUnder(FrameworkElement element, FrameworkElement reference)
        {
            Point center1 = GetCenter(element);
            Point center2 = GetCenter(reference);

            if (center1.Y > center2.Y)
            {
                return true;
            }
            else return false;
        }

        public static bool Overlap(FrameworkElement element1, FrameworkElement element2)
        {
            return OverlapX(element1, element2) && OverlapY(element1, element2);
        }

        public static bool OverlapX(FrameworkElement element1, FrameworkElement element2)
        {
            bool overlap = false;

            if (IsRight(element1, element2))
            {
                if (GetLeft(element1) < GetRight(element2))
                {
                    overlap = true;
                }
            }
            else if (IsLeft(element1, element2))
            {
                if (GetLeft(element2) < GetRight(element1))
                {
                    overlap = true;
                }
            }
            else
            {
                overlap = true;
            }

            return overlap;
        }

        public static bool OverlapY(FrameworkElement element1, FrameworkElement element2)
        {
            bool overlap = false;

            if (IsAbove(element1, element2))
            {
                if (GetBottom(element1) > GetTop(element2))
                {
                    overlap = true;
                }
            }
            else if (IsUnder(element1, element2))
            {
                if (GetBottom(element2) > GetTop(element1))
                {
                    overlap = true;
                }
            }
            else
            {
                overlap = true;
            }

            return overlap;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            MouseLeftButtonDown += ResizableCanvas_MouseLeftButtonDown;
            MouseLeftButtonUp += ResizableCanvas_MouseLeftButtonUp;
            MouseRightButtonDown += ResizableCanvas_MouseRightButtonDown;
            MouseMove += ResizableCanvas_MouseMove;
        }

        private void ResizableCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement source = GetRecursiveFirstWellKnownParent((FrameworkElement)e.OriginalSource);

            if (_SelectedItem == source)
            {
                if (_SelectedItem != null)
                {
                    DeselectItem();
                }
            }

        }

        private static bool IsRight(FrameworkElement element, FrameworkElement reference)
        {
            Point center1 = GetCenter(element);
            Point center2 = GetCenter(reference);

            if (center1.X > center2.X)
            {
                return true;
            }
            else return false;
        }

        private void _SelectedItem_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.SizeAll;
        }

        private void _SelectedItem_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private void _SelectedItem_MouseMove(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.SizeAll;
        }

        private void DeselectItem()
        {
            if (_SelectedItem != null && _SelectedItem is DragableItem)
            {
                AdornerLayer.GetAdornerLayer(_SelectedItem).Remove(_SelectedItemAdorner);
                _SelectedItem.MouseMove -= _SelectedItem_MouseMove;
                _SelectedItem.MouseEnter -= _SelectedItem_MouseEnter;
                _SelectedItem.MouseLeave -= _SelectedItem_MouseLeave;

                _IsDragging = false;
                Mouse.OverrideCursor = null;
            }
            _SelectedItem = null;
        }

        private FrameworkElement GetRecursiveFirstWellKnownParent(FrameworkElement element)
        {
            if (element != null && element.Parent != null)
            {
                if (element is DragableItem)
                {
                    return element;
                }
                else
                {
                    if (element.Parent is FrameworkElement)
                    {
                        return GetRecursiveFirstWellKnownParent(element.Parent as FrameworkElement);
                    }
                }
            }
            return null;
        }

        private void ResizableCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement source = GetRecursiveFirstWellKnownParent((FrameworkElement)e.OriginalSource);
            if ((_SelectedItem == null || _SelectedItem != source) && (source is DragableItem))
            {
                if (_SelectedItem != null)
                {
                    DeselectItem();
                }
                SelectItem(source);
            }
            else if (_SelectedItem != source)
            {
                if (_SelectedItem != null)
                {
                    DeselectItem();
                }
            }
            else
            {
                _IsDragging = true;
                _DragStartMousePosion = Mouse.GetPosition(this);
            }
        }

        private void ResizableCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _IsDragging = false;
            Mouse.OverrideCursor = null;
        }

        private void ResizableCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = Mouse.GetPosition(this);
            if (_IsDragging && _SelectedItem is DragableItem)
            {
                DragableItem selectedItem = (_SelectedItem as DragableItem);
                SetLeft(selectedItem, GetLeft(selectedItem) - (_DragStartMousePosion.X - currentMousePosition.X));
                selectedItem.NotifyMovement(-(_DragStartMousePosion.X - currentMousePosition.X), 0);
                _DragStartMousePosion.X = Mouse.GetPosition(this).X;
                SetTop(_SelectedItem, GetTop(selectedItem) - (_DragStartMousePosion.Y - currentMousePosition.Y));
                selectedItem.NotifyMovement(0, -(_DragStartMousePosion.Y - currentMousePosition.Y));
                _DragStartMousePosion.Y = Mouse.GetPosition(this).Y;
            }
        }

        private void SelectItem(FrameworkElement item)
        {
            _SelectedItem = item;
            if (item is DragableItem)
            {
                _SelectedItem.MouseMove += _SelectedItem_MouseMove;
                _SelectedItem.MouseEnter += _SelectedItem_MouseEnter;
                _SelectedItem.MouseLeave += _SelectedItem_MouseLeave;
                _SelectedItemAdorner = new ResizingAdorner(_SelectedItem);
                AdornerLayer.GetAdornerLayer(_SelectedItem).Add(_SelectedItemAdorner);
            }
        }

        #endregion Methods
    }
}