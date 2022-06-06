using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AppCenterBuilder
{
    interface ISettingsHandler
    {
        static void HandleParamErrors(IEnumerable errs) => throw new NotImplementedException();
        static void UseParams(ISettings opts) => throw new NotImplementedException();
    }
}
