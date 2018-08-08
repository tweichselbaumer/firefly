using System.Reflection;
using System.Windows;

namespace FireFly.ViewModels
{
    public abstract class AbstractBaseViewModel : DependencyObject
    {
        internal virtual void SettingsUpdated()
        {
            foreach (PropertyInfo property in GetType().GetProperties())
            {
                if (typeof(AbstractViewModel).IsAssignableFrom(property.PropertyType))
                {
                    ((AbstractViewModel)property.GetValue(this)).SettingsUpdated();
                }
            }
        }
    }
}