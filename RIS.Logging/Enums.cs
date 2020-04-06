using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIS.Logging
{
    public enum LogSituation
    {
        Unknown = 1,
        ApplicationAction = 2,
        UserAction = 3,
        LogAction = 4,
        Information = 5,
        Warning = 6,
        Error = 7,
        CriticalError = 8
    }
}
