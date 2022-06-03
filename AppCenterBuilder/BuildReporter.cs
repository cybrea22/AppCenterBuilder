using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace AppCenterBuilder
{
    public class BuildParams
    {
        public string SourceVersion { get; set; }
        public bool Debug { get; set; }
    }
    public class BuildInfo
    {
        public string BranchName { get; set; }
        public bool IsSuccessful { get; set; }
        public double ElapsedTime { get; set; }
        public string LogsLink { get; set; }
    }
    class BuildReporter
    {
        static HttpClient client = new HttpClient();
        public const string ApiKeyName = "X-API-Token";
        private readonly ISettings settings;
        public BuildReporter(ISettings settings)
        {
            this.settings = settings;
            // configure client
            client.BaseAddress = new Uri(this.settings.BaseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add(ApiKeyName, this.settings.Token);
        }
        
        public async Task PrintBuildInfoAsync(int buildNum)
        {
            Console.WriteLine($"PrintBuildInfoAsync started on build {buildNum}");
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
        }

        public async Task<int> RunBuildAsync(string BranchName, BuildParams bp)
        {
            Console.WriteLine($"RunBuildAsync started on branch {BranchName}");
            var content = JsonConvert.SerializeObject(bp);
            var buffer = System.Text.Encoding.UTF8.GetBytes(content);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            string res = null;
            HttpResponseMessage response = await client.PostAsync($"/v0.1/apps/{settings.OwnerName}/{settings.AppName}/branches/{BranchName}/builds", byteContent);
            if (response.IsSuccessStatusCode)
            {
                res = await response.Content.ReadAsStringAsync();

                var data = (JObject)JsonConvert.DeserializeObject(res);
                return int.Parse(data["id"].ToString());
            }
            Console.WriteLine($"RunBuildAsync ended on branch {BranchName}");
            // response.EnsureSuccessStatusCode();

            return 0;
        }

        public async Task<bool> ReportBuildResultAsync(TimeSpan timeout, int buildNum)
        {
            Console.WriteLine($"ReportBuildResultAsync started on build {buildNum}");
            DateTime start = DateTime.Now;
            while (!await IsBuildFinishedAsync(buildNum))
            {
                if (start.Add(timeout).CompareTo(DateTime.Now) <= 0)
                    return false;

                // wait 30 sec
                Thread.Sleep(1000 * 30);
            }

            Console.WriteLine($"ReportBuildResultAsync continued on build {buildNum}");
            await PrintBuildInfoAsync(buildNum);
            Console.WriteLine($"ReportBuildResultAsync ended on build {buildNum}");
            return true;
        }

        public async Task<bool> IsBuildFinishedAsync(int buildNum)
        {
            Console.WriteLine($"IsBuildFinishedAsync started on build {buildNum}");
            string res = null;
            HttpResponseMessage response = await client.GetAsync($"/v0.1/apps/{settings.OwnerName}/{settings.AppName}/builds/{buildNum}");
            if (response.IsSuccessStatusCode)
            {
                res = await response.Content.ReadAsStringAsync();
                var data = (JObject)JsonConvert.DeserializeObject(res);
                return (data["status"].ToString().CompareTo("completed") == 0);
            }
            Console.WriteLine($"IsBuildFinishedAsync ended on build {buildNum}");
            return false;
        }

        public async Task<Dictionary<string, string>> GetBranchesAsync()
        {
            Console.WriteLine($"GetBranchesAsync started");
            Dictionary<string, string> branches = null;
            string res = null;
            HttpResponseMessage response = await client.GetAsync($"v0.1/apps/{settings.OwnerName}/{settings.AppName}/branches");
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
            Console.WriteLine($"GetBranchesAsync ended");
            return branches;
        }

        public async Task DoInSeq(string branch, string sha)
        {
            int buildNum = await RunBuildAsync(branch, new BuildParams
            {
                SourceVersion = sha,
                Debug = settings.Debug
            });
            await ReportBuildResultAsync(new TimeSpan(0, 5, 0), buildNum);
        }
        public async Task BuildAndReport()
        {
            try
            {
                // get list of branches for application
                Dictionary<string, string> branches = await GetBranchesAsync();

                var buildTasks = new List<Task>();
                foreach (var branch in branches)
                {
                    /*
                    int buildNum = await RunBuildAsync(branch.Key, new BuildParams
                    {
                        SourceVersion = branch.Value,
                        Debug = settings.Debug
                    });
                    await ReportBuildResultAsync(new TimeSpan(0, 5, 0), buildNum);
                    */
                    /*
                    Task<int> t1 = Task.Run(() => RunBuildAsync(branch.Key, new BuildParams
                    {
                        SourceVersion = branch.Value,
                        Debug = settings.Debug
                    }));
                    Task t2 = t1.ContinueWith(task => ReportBuildResultAsync(new TimeSpan(0, 5, 0), task.Result));
                    buildTasks.Add(t2);
                    */
                    buildTasks.Add(Task.Run(() => DoInSeq(branch.Key, branch.Value)));
                }
                //await Task.WaitAll(buildTasks);
                Task.WaitAll(buildTasks.ToArray());


                //var results = await Task.WhenAll(buildTasks);
                //Task tt = Task.WhenAll(buildTasks);
                //tt.Wait();
                int i = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
    }
}
