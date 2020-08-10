// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS
{
    public class RInformationEventArgs : EventArgs
    {
        public string Message { get; }

        public RInformationEventArgs(string message)
        {
            Message = message;
        }
    }

    public class RWarningEventArgs : EventArgs
    {
        public string Message { get; }

        public RWarningEventArgs(string message)
        {
            Message = message;
        }
    }

    public class RErrorEventArgs : EventArgs
    {
        public string Message { get; }
        public string Stacktrace { get; }

        public RErrorEventArgs(string message, string stacktrace)
        {
            Message = message;
            Stacktrace = stacktrace;
        }
    }
}
