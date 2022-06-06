﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Collections;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace AppCenterBuilder
{
    class BuildReporter: IBuildReporter
    {
        static HttpClient client = new HttpClient();
        private readonly ISettings settings;
        private const int timeout = 8 * 60; // build run timeout in seconds
        public BuildReporter(ISettings settings)
        {
            this.settings = settings;
            // configure client
            client.BaseAddress = new Uri(this.settings.BaseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add(settings.ApiKeyName, settings.Token);
        }
        
        public async Task PrintBuildInfoAsync(int buildNum)
        {
            string res = null;
            HttpResponseMessage response = await client.GetAsync($"/v0.1/apps/{settings.OwnerName}/{settings.AppName}/builds/{buildNum}");
            if (response.IsSuccessStatusCode)
            {
                res = await response.Content.ReadAsStringAsync();
                var data = (JObject)JsonConvert.DeserializeObject(res);
                BuildInfo bi = new BuildInfo
                {
                    BranchName = data["sourceBranch"].ToString(),
                    IsSuccessful = data["result"].ToString().CompareTo("succeeded") == 0,
                    ElapsedTime = (DateTime.Parse(data["finishTime"].ToString()).Subtract(DateTime.Parse(data["startTime"].ToString()))).TotalSeconds,
                    LogsLink = $"{settings.BaseUrl}v0.1/apps/{settings.OwnerName}/{settings.AppName}/builds/{buildNum}/logs"
                };
                Console.WriteLine("{0} build {1} in {2} seconds. Link to build logs: {3}", bi.BranchName, bi.IsSuccessful ? "completed" : "failed", bi.ElapsedTime, bi.LogsLink);
            }
            else
            {
                Console.WriteLine($"Unseccessful API Call: {response.StatusCode} - " +
                    $"{response.ReasonPhrase} at {response.RequestMessage.RequestUri}");
            }
        }

        public async Task<int> RunBuildAsync(BuildParams bp)
        {
            var content = JsonConvert.SerializeObject(bp);
            var buffer = System.Text.Encoding.UTF8.GetBytes(content);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            string res = null;
            HttpResponseMessage response = await client.PostAsync($"/v0.1/apps/{settings.OwnerName}/{settings.AppName}/branches/{bp.BranchName}/builds", byteContent);
            if (response.IsSuccessStatusCode)
            {
                res = await response.Content.ReadAsStringAsync();

                var data = (JObject)JsonConvert.DeserializeObject(res);
                return int.Parse(data["id"].ToString());
            }
            else
            {
                Console.WriteLine($"Unseccessful API Call: {response.StatusCode} - " +
                    $"{response.ReasonPhrase} at {response.RequestMessage.RequestUri}");
            }

            return 0;
        }

        public async Task ReportBuildResultAsync(int buildNum)
        {
            DateTime start = DateTime.Now;
            while (!await IsBuildFinishedAsync(buildNum))
            {
                if (start.Add(TimeSpan.FromSeconds(timeout)).CompareTo(DateTime.Now) <= 0)
                {
                    Console.WriteLine($"ReportBuildResultAsync timed out for {buildNum}");
                    return;
                }
                // wait 20 sec
                Thread.Sleep(1000 * 20);
            }
            await PrintBuildInfoAsync(buildNum);
        }

        public async Task<bool> IsBuildFinishedAsync(int buildNum)
        {
            string res = null;
            HttpResponseMessage response = await client.GetAsync($"/v0.1/apps/{settings.OwnerName}/{settings.AppName}/builds/{buildNum}");
            if (response.IsSuccessStatusCode)
            {
                res = await response.Content.ReadAsStringAsync();
                var data = (JObject)JsonConvert.DeserializeObject(res);
                return (data["status"].ToString().CompareTo("completed") == 0);
            }
            else
            {
                Console.WriteLine($"Unseccessful API Call: {response.StatusCode} - " +
                    $"{response.ReasonPhrase} at {response.RequestMessage.RequestUri}");
            }
            return false;
        }

        public async Task<Dictionary<string, string>> GetBranchesAsync()
        {
            // branches contain branch name and commit hash
            Dictionary<string, string> branches = null;

            string res = null;
            HttpResponseMessage response = await client.GetAsync($"v0.1/apps/{settings.OwnerName}/{settings.AppName}/branches");
            // response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                res = await response.Content.ReadAsStringAsync();

                var data = (JArray)JsonConvert.DeserializeObject(res);
                branches = new Dictionary<string, string>(data.Count);
                foreach (JObject item in data)
                {
                    branches.Add(item["branch"]["name"].ToString(), item["branch"]["commit"]["sha"].ToString());
                }
            }
            else
            {
                Console.WriteLine($"Unseccessful API Call: {response.StatusCode} - " +
                    $"{response.ReasonPhrase} at {response.RequestMessage.RequestUri}");
            }
            
            return branches;
        }
        public async Task BuildAndReport()
        {
            try
            {
                // get list of branches for application
                Dictionary<string, string> branches = await GetBranchesAsync();

                if (branches != null)
                {
                    var buildTasks = new List<Task>();

                    // for each branch independantly: start build and print results consequentially
                    foreach (var branch in branches)
                    {
                        buildTasks.Add(Task.Run(async () =>
                        {
                            int buildNum = await RunBuildAsync(new BuildParams
                            {
                                BranchName = branch.Key,
                                SourceVersion = branch.Value,
                                Debug = settings.Debug
                            });
                            await ReportBuildResultAsync(buildNum);
                        }));
                    }
                    Task.WaitAll(buildTasks.ToArray());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
