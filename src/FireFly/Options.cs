using CommandLine;
using System;

namespace FireFly
{
    public class Options
    {
        [Option('c', "config", Required = false, HelpText = "Path to the config file.", Default = "config.json")]
        public String ConfigFileName { get; set; }
    }
}