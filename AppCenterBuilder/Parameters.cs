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
    
}
