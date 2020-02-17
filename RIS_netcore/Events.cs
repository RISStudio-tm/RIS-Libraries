using System;

namespace RIS
{
    public static class Events
    {
        private static object LockObjMessageEvent { get; } = new object();
        private static event RMessageHandler PShowMessage;
        public static event RMessageHandler ShowMessage
        {
            add
            {
                lock (LockObjMessageEvent)
                {
                    PShowMessage += value;
                    DShowMessage += value;
                }
            }
            remove
            {
                lock (LockObjMessageEvent)
                {
                    if (PShowMessage != null)
                        PShowMessage -= value;
                    if (DShowMessage != null)
                        DShowMessage -= value;
                }
            }
        }
        public static RMessageHandler DShowMessage { get; private set; }

        private static object LockObjErrorEvent { get; } = new object();
        private static event RErrorHandler PShowError;
        public static event RErrorHandler ShowError
        {
            add
            {
                lock (LockObjErrorEvent)
                {
                    PShowError += value;
                    DShowError += value;
                }
            }
            remove
            {
                lock (LockObjErrorEvent)
                {
                    if (PShowError != null)
                        PShowError -= value;
                    if (DShowError != null)
                        DShowError -= value;
                }
            }
        }
        public static RErrorHandler DShowError { get; private set; }
    }
}
