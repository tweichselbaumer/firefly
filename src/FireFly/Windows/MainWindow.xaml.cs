using FireFly.ViewModels;
using MahApps.Metro.Controls;

namespace FireFly.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(string filename)
        {
            InitializeComponent();
            ((MainViewModel)DataContext).ConfigFileName = filename;
        }

        private void HamburgerMenuControl_OnItemClick(object sender, ItemClickEventArgs e)
        {
            this.HamburgerMenuControl.Content = e.ClickedItem;
        }
    }
}