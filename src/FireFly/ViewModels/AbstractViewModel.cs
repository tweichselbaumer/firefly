namespace FireFly.ViewModels
{
    public abstract class AbstractViewModel : AbstractBaseViewModel
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
    }
}