// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Text;

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
