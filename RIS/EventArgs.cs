// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS
{
    public class RInformationEventArgs : EventArgs
    {
        public Exception SourceException { get; }
        public string Message { get; }

        public RInformationEventArgs(string message)
            : this(null, message)
        {

        }
        public RInformationEventArgs(Exception sourceException,
            string message)
        {
            SourceException = sourceException;
            Message = message;
        }
    }

    public class RWarningEventArgs : EventArgs
    {
        public Exception SourceException { get; }
        public string Message { get; }

        public RWarningEventArgs(string message)
            : this(null, message)
        {

        }
        public RWarningEventArgs(Exception sourceException,
            string message)
        {
            SourceException = sourceException;
            Message = message;
        }
    }

    public class RErrorEventArgs : EventArgs
    {
        public Exception SourceException { get; }
        public string Message { get; }
        public string Stacktrace { get; }

        public RErrorEventArgs(string message, string stacktrace)
            : this(null, message, stacktrace)
        {

        }
        public RErrorEventArgs(Exception sourceException,
            string message, string stacktrace)
        {
            SourceException = sourceException;
            Message = message;
            Stacktrace = stacktrace;
        }
    }
}
