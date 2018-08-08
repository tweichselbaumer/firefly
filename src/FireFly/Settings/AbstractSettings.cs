using System.Reflection;

namespace FireFly.Settings
{
    public abstract class AbstractSettings
    {
        public virtual void SetDefaults()
        {
            foreach (PropertyInfo property in GetType().GetProperties())
            {
                if (typeof(AbstractSettings).IsAssignableFrom(property.PropertyType))
                {
                    ((AbstractSettings)property.GetValue(this)).SetDefaults();
                }
            }
        }
    }
}