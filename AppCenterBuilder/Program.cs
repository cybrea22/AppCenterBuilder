﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
        static HttpClient client = new HttpClient();
        static string baseUrl = "https://api.appcenter.ms/";
        static string ownerName = "lilia.sabitova";
        static string appName = "App5";
        static string appUrl = $"v0.1/apps/{ownerName}/{appName}";
        static bool debug = true;

        static string token = "";
        public const string ApiKeyName = "X-API-Token";
        

        public class BuildParams
        {
            public string sourceVersion { get; set; }
            public bool debug { get; set; }
        }
        public class BuildInfo
        {
            public string branchName { get; set; }
            public bool isSuccessful { get; set; }
            public double elapsedTime { get; set; }
            public string logsLink { get; set; }
        }
        static async Task PrintBuildInfoAsync(int buildNum)
        {
            string res = null;
            HttpResponseMessage response = await client.GetAsync($"/v0.1/apps/{ownerName}/{appName}/builds/{buildNum}");
            if (response.IsSuccessStatusCode)
            {
                res = await response.Content.ReadAsStringAsync();
                var data = (JObject)JsonConvert.DeserializeObject(res);
                BuildInfo bi = new BuildInfo
                {
                    branchName = data["sourceBranch"].ToString(),
                    isSuccessful = data["result"].ToString().CompareTo("succeeded") == 0,
                    elapsedTime = (DateTime.Parse(data["finishTime"].ToString()).Subtract(DateTime.Parse(data["startTime"].ToString()))).TotalSeconds,
                    logsLink = $"{baseUrl}v0.1/apps/{ownerName}/{appName}/builds/{buildNum}/logs"
                };
                Console.WriteLine("{0} build {1} in {2} seconds. Link to build logs: {3}", bi.branchName, bi.isSuccessful ? "completed" : "failed", bi.elapsedTime, bi.logsLink);
            }
        }

        static async Task<int> RunBuildAsync(string branchName, BuildParams bp)
        {
            var content = JsonConvert.SerializeObject(bp);
            var buffer = System.Text.Encoding.UTF8.GetBytes(content);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            string res = null;
            HttpResponseMessage response = await client.PostAsync($"/v0.1/apps/{ownerName}/{appName}/branches/{branchName}/builds", byteContent);
            if (response.IsSuccessStatusCode)
            {
                res = await response.Content.ReadAsStringAsync();

                var data = (JObject)JsonConvert.DeserializeObject(res);
                return int.Parse(data["id"].ToString());
            }
            // response.EnsureSuccessStatusCode();

            return 0;
        }

        static async Task<bool> ReportBuildResultAsync(TimeSpan timeout, int buildNum)
        {
            DateTime start = DateTime.Now;
            while (!await IsBuildFinishedAsync(buildNum))
            {
                if (start.Add(timeout).CompareTo(DateTime.Now) <= 0)
                    return false;

                // wait 30 sec
                Thread.Sleep(1000*30);
            }
            await PrintBuildInfoAsync(buildNum);
            return true;
        }

        static async Task<bool> IsBuildFinishedAsync(int buildNum)
        {
            string res = null;
            HttpResponseMessage response = await client.GetAsync($"/v0.1/apps/{ownerName}/{appName}/builds/{buildNum}");
            if (response.IsSuccessStatusCode)
            {
                res = await response.Content.ReadAsStringAsync();
                var data = (JObject)JsonConvert.DeserializeObject(res);
                return (data["status"].ToString().CompareTo("completed") == 0);
            }
            return false;
        }

        static async Task<Dictionary<string, string>> GetBranchesAsync ()
        {
            Dictionary<string, string> branches = null;
            string res = null;
            HttpResponseMessage response = await client.GetAsync($"v0.1/apps/{ownerName}/{appName}/branches");
            if (response.IsSuccessStatusCode)
            {
                res = await response.Content.ReadAsStringAsync();

                var data = (JArray)JsonConvert.DeserializeObject(res);
                branches = new Dictionary<string, string>(data.Count);
                foreach (JObject item in data)
                {
                    // Console.WriteLine(item["branch"]["name"]);
                    branches.Add(item["branch"]["name"].ToString(), item["branch"]["commit"]["sha"].ToString());
                }
            }
            return branches;
        }

        static async Task RunAsync()
        {
            // configure client
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));            
            client.DefaultRequestHeaders.Add(ApiKeyName, token);

            try
            {
                // get list of branches for application
                Dictionary<string, string> branches = await GetBranchesAsync();

                foreach (var branch in branches)
                {
                    int buildNum = await RunBuildAsync(branch.Key, new BuildParams
                    {
                        sourceVersion = branch.Value,
                        debug = debug
                    });
                    await ReportBuildResultAsync(new TimeSpan(0, 5, 0), buildNum);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }
    }
}