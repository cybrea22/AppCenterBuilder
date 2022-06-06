using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AppCenterBuilder
{
    interface IBuildReporter
    {
        Task PrintBuildInfoAsync(int buildNum);
        Task<int> RunBuildAsync(BuildParams bp);
        Task ReportBuildResultAsync(int buildNum);
        Task<bool> IsBuildFinishedAsync(int buildNum);
        Task<Dictionary<string, string>> GetBranchesAsync();
        Task BuildAndReport();
    }
}
