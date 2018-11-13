using CommandLine;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
        private const int ATTACH_PARENT_PROCESS = -1;

        private const int MINIMUM_SPLASH_TIME = 1500;

        protected override void OnStartup(StartupEventArgs e)
        {
            AttachConsole(ATTACH_PARENT_PROCESS);

            Windows.MainWindow home = null;
            Windows.SplashScreen splash = null;
            Stopwatch timer;

            splash = new Windows.SplashScreen();
            splash.Show();

            timer = new Stopwatch();
            timer.Start();

            Func<Options, int> setOptions = opts =>
            {
                home = new Windows.MainWindow(opts.ConfigFileName);
                return 0;
            };

            Parser parser = new Parser(config => config.HelpWriter = Console.Out);

            parser.ParseArguments<Options>(e.Args).MapResult((Options opts) => setOptions(opts), errs => 1);

            timer.Stop();

            int remainingTimeToShowSplash = MINIMUM_SPLASH_TIME - (int)timer.ElapsedMilliseconds;
            if (remainingTimeToShowSplash > 0)
                Thread.Sleep(remainingTimeToShowSplash);

            splash.Close();

            if (home != null)
            {
                home.Show();
            }
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();
    }
}