using System;

namespace RIS
{
    public static class Events
    {
        private static object LockObjMessageEvent { get; }
        private static event EventHandler<RMessageEventArgs> PShowMessage;
        public static event EventHandler<RMessageEventArgs> ShowMessage
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
        public static EventHandler<RMessageEventArgs> DShowMessage { get; private set; }

        private static object LockObjErrorEvent { get; }
        private static event EventHandler<RErrorEventArgs> PShowError;
        public static event EventHandler<RErrorEventArgs> ShowError
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
        public static EventHandler<RErrorEventArgs> DShowError { get; private set; }

        static Events()
        {
            LockObjMessageEvent = new object();
            LockObjErrorEvent = new object();
        }
    }
}
