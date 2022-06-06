using System;
using System.Collections.Generic;
using System.Text;

namespace AppCenterBuilder
{
    public class BuildInfo
    {
        public string BranchName { get; set; }
        public bool IsSuccessful { get; set; }
        public double ElapsedTime { get; set; }
        public string LogsLink { get; set; }
    }
}
