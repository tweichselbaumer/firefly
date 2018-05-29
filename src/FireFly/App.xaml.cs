using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FireFly
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const int MINIMUM_SPLASH_TIME = 2500;

        protected override void OnStartup(StartupEventArgs e)
        {
            Windows.SplashScreen splash = new Windows.SplashScreen();
            splash.Show();

            Stopwatch timer = new Stopwatch();
            timer.Start();

            Windows.MainWindow home = new Windows.MainWindow();
            timer.Stop();

            int remainingTimeToShowSplash = MINIMUM_SPLASH_TIME - (int)timer.ElapsedMilliseconds;
            if (remainingTimeToShowSplash > 0)
                Thread.Sleep(remainingTimeToShowSplash);

            splash.Close();
            home.Show();
        }
    }
}
