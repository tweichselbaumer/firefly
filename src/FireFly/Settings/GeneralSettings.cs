using System.Collections.Generic;

namespace FireFly.Settings
{
    public class FileLocation
    {
        private string _Name;
        private string _Path;

        public string Name
        {
            get
            {
                return _Name;
            }

            set
            {
                _Name = value;
            }
        }

        public string Path
        {
            get
            {
                return _Path;
            }

            set
            {
                _Path = value;
            }
        }
    }

    public class GeneralSettings : AbstractSettings
    {
        private List<FileLocation> _FileLocations = new List<FileLocation>();

        public List<FileLocation> FileLocations
        {
            get
            {
                return _FileLocations;
            }

            set
            {
                _FileLocations = value;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            FileLocations.Add(new FileLocation() { Name = "Default", Path = "./output" });
        }
    }
}