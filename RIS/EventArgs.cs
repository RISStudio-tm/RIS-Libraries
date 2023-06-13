// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS
{
    public class RInformationEventArgs : EventArgs
    {
        public Exception SourceException { get; }
        private string _message;
        public string Message
        {
            get
            {
                return _message
                       ?? SourceException?.Message
                       ?? string.Empty;
            }
            private set
            {
                _message = value;
            }
        }

        public RInformationEventArgs(string message)
            : this(null, message)
        {

        }
        public RInformationEventArgs(Exception sourceException,
            string message = null)
        {
            SourceException = sourceException;
            Message = message;
        }
    }

    public class RWarningEventArgs : EventArgs
    {
        public Exception SourceException { get; }
        private string _message;
        public string Message
        {
            get
            {
                return _message
                       ?? SourceException?.Message
                       ?? string.Empty;
            }
            private set
            {
                _message = value;
            }
        }

        public RWarningEventArgs(string message)
            : this(null, message)
        {

        }
        public RWarningEventArgs(Exception sourceException,
            string message = null)
        {
            SourceException = sourceException;
            Message = message;
        }
    }

    public class RErrorEventArgs : EventArgs
    {
        public Exception SourceException { get; }
        private string _message;
        public string Message
        {
            get
            {
                return _message
                       ?? SourceException?.Message
                       ?? string.Empty;
            }
            private set
            {
                _message = value;
            }
        }

        public RErrorEventArgs(string message)
            : this(null, message)
        {

        }
        public RErrorEventArgs(Exception sourceException,
            string message = null)
        {
            SourceException = sourceException;
            Message = message;
        }
    }
}
