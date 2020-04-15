using System;

namespace RIS
{
    public class RMessageEventArgs : EventArgs
    {
        public string Message { get; }

        public RMessageEventArgs(string message)
        {
            Message = message;
        }
    }
    public delegate void RMessageHandler(object sender, RMessageEventArgs eventArgs);

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
    public delegate void RErrorHandler(object sender, RErrorEventArgs eventArgs);
}
