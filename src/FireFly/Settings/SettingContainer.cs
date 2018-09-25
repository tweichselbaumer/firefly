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
                _Settings.SetDefaults();
            }
        }

        public void Save()
        {
            string output = JsonConvert.SerializeObject(_Settings);
            if (!File.Exists(_SettingFileName) || File.ReadAllText(_SettingFileName) != output)
            {
                string backupDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(_SettingFileName)), "Backup");
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }
                File.WriteAllText(Path.Combine(backupDir, string.Format("{0}.{2}{1}", Path.GetFileNameWithoutExtension(_SettingFileName), Path.GetExtension(_SettingFileName), DateTime.Now.ToFileTimeUtc())), output);
                File.WriteAllText(_SettingFileName, output);
            }
        }
    }
}