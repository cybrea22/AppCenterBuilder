using CommandLine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace AppCenterBuilder
{
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
        public string ApiKeyName
        {
            get { return ConfigurationManager.AppSettings.Get("ApiKeyName").ToString(); }
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
    public class CommandLineSettings: ISettings
    {
        [Option(shortName: 'u', longName: "url", Required = false, HelpText = "Base Url", Default = "https://api.appcenter.ms/")]
        public string BaseUrl { get; set; }

        [Option(shortName: 'a', longName: "app", Required = true, HelpText = "App Name")]
        public string AppName { get; set; }

        [Option(shortName: 'o', longName: "owner", Required = true, HelpText = "Owner Name")]
        public string OwnerName { get; set; }

        [Option(shortName: 'k', longName: "apikey", Required = false, HelpText = "Api Key Name, i.e. X-API-Token", Default = "X-API-Token")]
        public string ApiKeyName { get; set; }

        [Option(shortName: 't', longName: "token", Required = true, HelpText = "Token")]
        public string Token { get; set; }

        [Option(shortName: 'd', longName: "debug", Required = false, HelpText = "Debug", Default = true)]
        public bool Debug { get; set; }
    }
    public class CommandLineSettingsHandler: ISettingsHandler
    {
        private const string settingsName = "Command Line";
        public static void HandleParamErrors(IEnumerable errs)
        {
            Console.WriteLine($"{settingsName} parameters are not valid!");
        }
        public static void UseParams(ISettings opts)
        {
            Console.WriteLine($"{settingsName} parameters are valid.");
        }
    }
}
