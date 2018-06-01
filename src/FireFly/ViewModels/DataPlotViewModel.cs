using System.Windows;

namespace FireFly.ViewModels
{
    public class DataPlotViewModel : AbstractViewModel
    {
        public DataPlotViewModel(MainViewModel parent) : base(parent)
        {
        }

        internal override void SettingsUpdated()
        {
        }

        internal override void UpdateLinkUpBindings()
        {
            if (Parent.Node != null)
            {
            }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataPlotViewModel dpvm = (d as DataPlotViewModel);
            bool changed = false;
            switch (e.Property.Name)
            {
                //case "Quality":
                //    changed = cvm.Parent.SettingContainer.Settings.StreamingSettings.Quality != cvm.Quality;
                //    cvm.Parent.SettingContainer.Settings.StreamingSettings.Quality = cvm.Quality;
                //    try
                //    {
                //        if (changed)
                //            cvm._QualityLabel.Value = (byte)cvm.Quality;
                //    }
                //    catch (Exception) { }
                //    break;
                //case "Enabled":
                //    changed = cvm.Parent.SettingContainer.Settings.StreamingSettings.Enabled != cvm.Enabled;
                //    cvm.Parent.SettingContainer.Settings.StreamingSettings.Enabled = cvm.Enabled;
                //    try
                //    {
                //        if (changed)
                //        {
                //            if (cvm.Enabled)
                //                cvm._EventLabel.Subscribe();
                //            else
                //                cvm._EventLabel.Unsubscribe();
                //        }

                //    }
                //    catch (Exception) { }
                //    break;

                default:
                    break;
            }
            if (changed)
            {
                dpvm.Parent.SettingsUpdated(false);
            }
        }
    }
}