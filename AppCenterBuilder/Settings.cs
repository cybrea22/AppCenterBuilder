using CommandLine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace AppCenterBuilder
{
    interface ISettings
    {
        string BaseUrl { get; }
        string AppName { get; }
        string OwnerName { get; }
        string Token { get; }
        bool Debug { get; }
    }
    public class ConfSettings : ISettings
    {
        public string BaseUrl
        {
            get { return ConfigurationManager.AppSettings.Get("BaseUrl").ToString(); }
        }
        public string AppName
        {
            get { return ConfigurationManager.AppSettings.Get("AppName").ToString(); }
        }
        public string OwnerName
        {
            get { return ConfigurationManager.AppSettings.Get("OwnerName").ToString(); }
        }
        public string Token
        {
            get { return ConfigurationManager.AppSettings.Get("Token").ToString(); }
        }
        public bool Debug
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings.Get("Debug")); }
        }
    }
    public class CommandLineOptions: ISettings
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
