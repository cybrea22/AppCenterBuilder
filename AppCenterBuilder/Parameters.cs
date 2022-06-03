using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using CommandLine;

namespace AppCenterBuilder
{
    class Parameters
    {
        public string BaseUrl { get; set; }
        public string AppName { get; set; }
        public string OwnerName { get; set; }
        public string Token { get; set; }
        public bool Debug { get; set; }
        public Parameters()
        {
            this.BaseUrl = ConfigurationManager.AppSettings.Get("BaseUrl");
            this.AppName = ConfigurationManager.AppSettings.Get("AppName");
            this.OwnerName = ConfigurationManager.AppSettings.Get("OwnerName");
            this.Token = ConfigurationManager.AppSettings.Get("Token");
            this.Debug = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("Debug"));
        }
        public Parameters(CommandLineOptions opts)
        {
            this.BaseUrl = opts.BaseUrl;
            this.AppName = opts.AppName;
            this.OwnerName = opts.OwnerName;
            this.Token = opts.Token;
            this.Debug = opts.Debug;
        }
    }
    public class CommandLineOptions
    {
        [Option(shortName: 'u', longName: "url", Required = false, HelpText = "Base Url", Default = "https://api.appcenter.ms/")]
        public string BaseUrl { get; set; }

        [Option(shortName: 'a', longName: "app", Required = true, HelpText = "App Name")]
        public string AppName { get; set; }

        [Option(shortName: 'o', longName: "owner", Required = true, HelpText = "Owner Name")]
        public string OwnerName { get; set; }

        [Option(shortName: 't', longName: "token", Required = true, HelpText = "Token")]
        public string Token { get; set; }

        [Option(shortName: 'd', longName: "debug", Required = false, HelpText = "Debug", Default = true)]
        public bool Debug { get; set; }
    }
}
