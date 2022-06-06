using System;
using System.Collections.Generic;
using System.Text;

namespace AppCenterBuilder
{
    public interface ISettings
    {
        string BaseUrl { get; }
        string AppName { get; }
        string OwnerName { get; }
        string ApiKeyName { get; }
        string Token { get; }
        bool Debug { get; }
    }
}
