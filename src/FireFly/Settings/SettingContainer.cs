using Newtonsoft.Json;
using System;
using System.IO;

namespace FireFly.Settings
{
    public class SettingContainer
    {
        private string _SettingFileName;

        private Settings _Settings;

        public string SettingFileName
        {
            get
            {
                return _SettingFileName;
            }

            set
            {
                _SettingFileName = value;
            }
        }

        public Settings Settings
        {
            get
            {
                return _Settings;
            }

            set
            {
                _Settings = value;
            }
        }

        public void Load()
        {
            try
            {
                string input = File.ReadAllText(_SettingFileName);
                _Settings = JsonConvert.DeserializeObject<Settings>(input);
            }
            catch (Exception)
            {
                _Settings = new Settings();
            }
        }

        public void Save()
        {
            string output = JsonConvert.SerializeObject(_Settings);
            File.WriteAllText(_SettingFileName, output);
        }
    }
}