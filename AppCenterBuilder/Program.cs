using CommandLine;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AppCenterBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildReporter br;
            if (args == null || args.Length == 0)
            {
                br = new BuildReporter(new ConfSettings());
                br.BuildAndReport().GetAwaiter().GetResult();
            }
            else
            {
               Parser.Default.ParseArguments<CommandLineSettings>(args)
                   .WithParsed(opts => {
                       CommandLineSettingsHandler.UseParams(opts); 
                       br = new BuildReporter(opts);
                       br.BuildAndReport().GetAwaiter().GetResult(); 
                   })
                   .WithNotParsed((errs) => CommandLineSettingsHandler.HandleParamErrors(errs));
            }
            Console.ReadLine();
        }
    }
}
