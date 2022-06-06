using System;
using System.Collections.Generic;
using System.Text;

namespace AppCenterBuilder
{
    public class BuildParams
    {
        public string BranchName { get; set; }
        public string SourceVersion { get; set; }
        public bool Debug { get; set; }
    }
}
