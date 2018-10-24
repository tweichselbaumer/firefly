using FireFly.Command;
using FireFly.Data.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FireFly.ViewModels
{
    public class DebugViewModel : AbstractViewModel
    {
        public DebugViewModel(MainViewModel parent) : base(parent)
        {
        }

        public RelayCommand<object> StartCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoStart(o);
                    });
            }
        }

        public RelayCommand<object> StopCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoStop(o);
                    });
            }
        }

        public RelayCommand<object> ShutdownCommand
        {
            get
            {
                return new RelayCommand<object>(
                    async (object o) =>
                    {
                        await DoShutdown(o);
                    });
            }
        }

        private Task DoShutdown(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                string expactString = string.Format("{0}@{1}:.{{0,}}[$]", Parent.SettingContainer.Settings.ConnectionSettings.Username, Parent.SettingContainer.Settings.ConnectionSettings.Hostname);

                RemoteDataStore remoteDataStore = new RemoteDataStore(Parent.SettingContainer.Settings.ConnectionSettings.IpAddress, Parent.SettingContainer.Settings.ConnectionSettings.Username, Parent.SettingContainer.Settings.ConnectionSettings.Password);
                remoteDataStore.ExecuteCommands(new List<string>() { string.Format("sudo shutdown -t 0\n{0}\n", Parent.SettingContainer.Settings.ConnectionSettings.Password) }, expactString);
            });
        }

        private Task DoStart(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                string expactString = string.Format("{0}@{1}:.{{0,}}[$]", Parent.SettingContainer.Settings.ConnectionSettings.Username, Parent.SettingContainer.Settings.ConnectionSettings.Hostname);

                RemoteDataStore remoteDataStore = new RemoteDataStore(Parent.SettingContainer.Settings.ConnectionSettings.IpAddress, Parent.SettingContainer.Settings.ConnectionSettings.Username, Parent.SettingContainer.Settings.ConnectionSettings.Password);
                remoteDataStore.ExecuteCommands(new List<string>() { "nohup /opt/firefly/computer-vision </dev/null >/opt/firefly/log.log 2>&1 &" }, expactString);
            });
        }

        private Task DoStop(object o)
        {
            return Task.Factory.StartNew(() =>
            {
                string expactString = string.Format("{0}@{1}:.{{0,}}[$]", Parent.SettingContainer.Settings.ConnectionSettings.Username, Parent.SettingContainer.Settings.ConnectionSettings.Hostname);

                RemoteDataStore remoteDataStore = new RemoteDataStore(Parent.SettingContainer.Settings.ConnectionSettings.IpAddress, Parent.SettingContainer.Settings.ConnectionSettings.Username, Parent.SettingContainer.Settings.ConnectionSettings.Password);
                remoteDataStore.ExecuteCommands(new List<string>() { "killall -9 computer-vision" }, expactString);
            });
        }
    }
}