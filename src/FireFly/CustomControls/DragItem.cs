using System.Windows.Controls;

namespace FireFly.CustomControls
{
    public partial class DragItem : UserControl
    {
        public delegate void MovedEventHanlder(MovedEventArgs e);

        public event MovedEventHanlder Moved;

        public void NotifyMovement(double deltaX, double deltaY)
        {
            Moved?.Invoke(new MovedEventArgs() { DeltaX = deltaX, DeltaY = deltaY });
        }
    }
}