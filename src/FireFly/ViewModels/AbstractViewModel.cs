using System.Windows;

namespace FireFly.ViewModels
{
    public abstract class AbstractViewModel : DependencyObject
    {
        private MainViewModel _Parent;

        public AbstractViewModel(MainViewModel parent)
        {
            _Parent = parent;
        }

        public MainViewModel Parent
        {
            get
            {
                return _Parent;
            }
        }

        internal abstract void SettingsUpdated();

        internal abstract void UpdateLinkUpBindings();
    }
}