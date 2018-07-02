using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireFly
{
    public class Options
    {
        [Option('c', "config", Required = false, HelpText = "Path to the config file.", Default = "config.json")]
        public String ConfigFileName { get; set; }
    }
}
